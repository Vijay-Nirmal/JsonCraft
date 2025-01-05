using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
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
                return Left is List<PathFilter> left ? JsonPath.Evaluate(left, root, t, settings).Any() : true;
            }

            if (Left is List<PathFilter> leftPath)
            {
                foreach (var leftResult in JsonPath.Evaluate(leftPath, root, t, settings))
                {
                    return EvaluateMatch(root, t, settings, leftResult);
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
                    foreach (var rightResult in JsonPath.Evaluate(right, root, t, settings))
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
                        if (RegexEquals(leftResult, rightResult, settings))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.Equals:
                        if (EqualsWithStringCoercion(leftResult, rightResult))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.StrictEquals:
                        if (EqualsWithStrictMatch(leftResult, rightResult))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.NotEquals:
                        if (!EqualsWithStringCoercion(leftResult, rightResult))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.StrictNotEquals:
                        if (!EqualsWithStrictMatch(leftResult, rightResult))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThan:
                        if (CompareTo(leftResult, rightResult) > 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThanOrEquals:
                        if (CompareTo(leftResult, rightResult) >= 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThan:
                        if (CompareTo(leftResult, rightResult) < 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThanOrEquals:
                        if (CompareTo(leftResult, rightResult) <= 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.Exists:
                        return true;
                }
            }
            else
            {
                switch (Operator)
                {
                    case QueryOperator.Exists:
                    // you can only specify primitive types in a comparison
                    // notequals will always be true
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

            return leftValue.GetRawText().CompareTo(rightValue.GetRawText());
        }

        private static bool RegexEquals(JsonElement input, JsonElement pattern, JsonSelectSettings? settings)
        {
            if (input.ValueKind != JsonValueKind.String || pattern.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            // TODO: Use JsonMarshal.GetRawUtf8Value for temp string allocation
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
            // Handle comparing an integer with a float
            // e.g. Comparing 1 and 1.0
            if (value.ValueKind == JsonValueKind.Number && queryValue.ValueKind == JsonValueKind.Number)
            {
                return value.GetDouble() == queryValue.GetDouble();
            }

            if (IsBoolean(value) && IsBoolean(queryValue))
            {
                return value.GetBoolean() == queryValue.GetBoolean();
            }

            if (IsNull(value) == IsNull(value))
            {
                return true;
            }

            // we handle floats and integers the exact same way, so they are pseudo equivalent
            if (value.ValueKind != queryValue.ValueKind)
            {
                return false;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return JsonMarshal.GetRawUtf8Value(value).SequenceEqual(JsonMarshal.GetRawUtf8Value(queryValue));
            }

            return false; // For Object and Array
        }
            
        private static bool IsBoolean(JsonElement v) => v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False;
        private static bool IsNull(JsonElement v) => v.ValueKind == JsonValueKind.Null || v.ValueKind == JsonValueKind.Undefined;
        private static bool IsJsonContainer(JsonElement v) => v.ValueKind == JsonValueKind.Array || v.ValueKind == JsonValueKind.Object;
    }
}