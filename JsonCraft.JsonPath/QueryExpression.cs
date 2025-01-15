using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace JsonCraft.JsonPath
{
    internal enum QueryOperator
    {
        None = 0,
        Equals = 1,
        NotEquals = 2,
        Exists = 3,
        LessThan = 4,
        LessThanOrEquals = 5,
        GreaterThan = 6,
        GreaterThanOrEquals = 7,
        And = 8,
        Or = 9,
        RegexEquals = 10,
        StrictEquals = 11,
        StrictNotEquals = 12
    }

    internal abstract class QueryExpression
    {
        internal QueryOperator Operator;

        public QueryExpression(QueryOperator @operator)
        {
            Operator = @operator;
        }

        public abstract bool IsMatch(JsonElement root, JsonElement t, JsonSelectSettings? settings = null);
    }

    internal class CompositeExpression : QueryExpression
    {
        public List<QueryExpression> Expressions { get; set; }

        public CompositeExpression(QueryOperator @operator) : base(@operator)
        {
            Expressions = new List<QueryExpression>();
        }

        public override bool IsMatch(JsonElement root, JsonElement t, JsonSelectSettings? settings = null)
        {
            switch (Operator)
            {
                case QueryOperator.And:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (!e.IsMatch(root, t, settings))
                        {
                            return false;
                        }
                    }
                    return true;
                case QueryOperator.Or:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (e.IsMatch(root, t, settings))
                        {
                            return true;
                        }
                    }
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal class BooleanQueryExpression : QueryExpression
    {
        public readonly object Left;
        public readonly object? Right;

        public BooleanQueryExpression(QueryOperator @operator, object left, object? right) : base(@operator)
        {
            Left = left;
            Right = right;
        }

        public override bool IsMatch(JsonElement root, JsonElement t, JsonSelectSettings? settings = null)
        {
            if (Operator == QueryOperator.Exists)
            {
                return Left is List<PathFilter> left ? JPath.Evaluate(left, root, t, settings).Any() : true;
            }

            if (Left is List<PathFilter> leftPath)
            {
                foreach (var leftResult in JPath.Evaluate(leftPath, root, t, settings))
                {
                    if (EvaluateMatch(root, t, settings, leftResult))
                    {
                        return true;
                    }
                }
            }
            else if (Left is JsonElement left)
            {
                return EvaluateMatch(root, t, settings, left);
            }

            return false;

            bool EvaluateMatch(JsonElement root, JsonElement t, JsonSelectSettings? settings, JsonElement leftResult)
            {
                if (Right is List<PathFilter> right)
                {
                    foreach (var rightResult in JPath.Evaluate(right, root, t, settings))
                    {
                        if (MatchTokens(leftResult, rightResult, settings))
                        {
                            return true;
                        }
                    }
                }
                else if (Right is JsonElement rightNode)
                {
                    return MatchTokens(leftResult, rightNode, settings);
                }

                return false;
            }
        }

        private bool MatchTokens(JsonElement leftResult, JsonElement rightResult, JsonSelectSettings? settings)
        {
            if (!IsJsonContainer(leftResult) && !IsJsonContainer(rightResult))
            {
                switch (Operator)
                {
                    case QueryOperator.RegexEquals:
                        return RegexEquals(leftResult, rightResult, settings);
                    case QueryOperator.Equals:
                        return EqualsWithStringCoercion(leftResult, rightResult);
                    case QueryOperator.StrictEquals:
                        return EqualsWithStrictMatch(leftResult, rightResult);
                    case QueryOperator.NotEquals:
                        return !EqualsWithStringCoercion(leftResult, rightResult);
                    case QueryOperator.StrictNotEquals:
                        return !EqualsWithStrictMatch(leftResult, rightResult);
                    case QueryOperator.GreaterThan:
                        return CompareTo(leftResult, rightResult) > 0;
                    case QueryOperator.GreaterThanOrEquals:
                        return CompareTo(leftResult, rightResult) >= 0;
                    case QueryOperator.LessThan:
                        return CompareTo(leftResult, rightResult) < 0;
                    case QueryOperator.LessThanOrEquals:
                        return CompareTo(leftResult, rightResult) <= 0;
                    case QueryOperator.Exists:
                        return true;
                }
            }
            else
            {
                switch (Operator)
                {
                    case QueryOperator.Exists:
                    case QueryOperator.NotEquals:
                        return true;
                }
            }

            return false;
        }

        internal static int CompareTo(JsonElement leftValue, JsonElement rightValue)
        {
            if (leftValue.ValueKind == rightValue.ValueKind)
            {
                switch (leftValue.ValueKind)
                {
                    case JsonValueKind.False:
                    case JsonValueKind.True:
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                        return 0;
                    case JsonValueKind.String:
                        return leftValue.GetString()!.CompareTo(rightValue.GetString());
                    case JsonValueKind.Number:
                        return leftValue.GetDouble().CompareTo(rightValue.GetDouble());
                    default:
                        throw new InvalidOperationException($"Can compare only value types, but the current type is: {leftValue.ValueKind}");
                }
            }

            if (IsBoolean(leftValue) && IsBoolean(rightValue))
            {
                return leftValue.GetBoolean().CompareTo(rightValue.GetBoolean());
            }

            if (TryGetAsDouble(leftValue, out double leftNum) && TryGetAsDouble(rightValue, out double rightNum))
            {
                return leftNum.CompareTo(rightNum);
            }

            return leftValue.GetRawText().CompareTo(rightValue.GetRawText());

            bool TryGetAsDouble(JsonElement value, out double num)
            {
                if (value.ValueKind == JsonValueKind.Number)
                {
                    num = value.GetDouble();
                    return true;
                }

                if (value.ValueKind == JsonValueKind.String && double.TryParse(JsonMarshal.GetRawUtf8Value(value)[1..^1], out num))
                {
                    return true;
                }

                num = default;
                return false;
            }
        }

        private static bool RegexEquals(JsonElement input, JsonElement pattern, JsonSelectSettings? settings)
        {
            if (input.ValueKind != JsonValueKind.String || pattern.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            string regexText = pattern.GetString()!;
            int patternOptionDelimiterIndex = regexText.LastIndexOf('/');

            string patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
            string optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

            TimeSpan timeout = settings?.RegexMatchTimeout ?? Regex.InfiniteMatchTimeout;
            return Regex.IsMatch(input.GetString()!, patternText, GetRegexOptions(optionsText), timeout);

            RegexOptions GetRegexOptions(string optionsText)
            {
                RegexOptions options = RegexOptions.None;

                for (int i = 0; i < optionsText.Length; i++)
                {
                    switch (optionsText[i])
                    {
                        case 'i':
                            options |= RegexOptions.IgnoreCase;
                            break;
                        case 'm':
                            options |= RegexOptions.Multiline;
                            break;
                        case 's':
                            options |= RegexOptions.Singleline;
                            break;
                        case 'x':
                            options |= RegexOptions.ExplicitCapture;
                            break;
                    }
                }

                return options;
            }
        }

        internal static bool EqualsWithStringCoercion(JsonElement value, JsonElement queryValue)
        {
            return JsonMarshal.GetRawUtf8Value(value).SequenceEqual(JsonMarshal.GetRawUtf8Value(queryValue));
        }

        internal static bool EqualsWithStrictMatch(JsonElement value, JsonElement queryValue)
        {

            if (IsBoolean(value) && IsBoolean(queryValue))
            {
                return value.GetBoolean() == queryValue.GetBoolean();
            }

            if (value.ValueKind != queryValue.ValueKind)
            {
                return false;
            }

            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetDouble() == queryValue.GetDouble();
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return JsonMarshal.GetRawUtf8Value(value).SequenceEqual(JsonMarshal.GetRawUtf8Value(queryValue));
            }

            if (value.ValueKind == JsonValueKind.Null && queryValue.ValueKind == JsonValueKind.Null)
            {
                return true;
            }

            if (value.ValueKind == JsonValueKind.Undefined && queryValue.ValueKind == JsonValueKind.Undefined)
            {
                return true;
            }

            return false; // For Object and Array
        }

        private static bool IsBoolean(JsonElement v) => v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False;
        private static bool IsJsonContainer(JsonElement v) => v.ValueKind == JsonValueKind.Array || v.ValueKind == JsonValueKind.Object;
    }
}