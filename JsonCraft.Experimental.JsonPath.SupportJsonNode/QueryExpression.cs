using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Linq;
using System.Text.Json;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
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

        // For unit tests
        public bool IsMatch(JsonNode root, JsonNode? t)
        {
            return IsMatch(root, t, null);
        }

        public abstract bool IsMatch(JsonNode root, JsonNode? t, JsonSelectSettings? settings);
    }

    internal class CompositeExpression : QueryExpression
    {
        public List<QueryExpression> Expressions { get; set; }

        public CompositeExpression(QueryOperator @operator) : base(@operator)
        {
            Expressions = new List<QueryExpression>();
        }

        public override bool IsMatch(JsonNode root, JsonNode? t, JsonSelectSettings? settings)
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
        public readonly object? Left;
        public readonly object? Right;

        public BooleanQueryExpression(QueryOperator @operator, object? left, object? right) : base(@operator)
        {
            Left = left;
            Right = right;
        }

        public override bool IsMatch(JsonNode root, JsonNode? t, JsonSelectSettings? settings)
        {
            if (Operator == QueryOperator.Exists)
            {
                return Left is List<PathFilter> left ? JPath.Evaluate(left, root, t, settings).Any() : true;
            }

            if (Left is List<PathFilter> leftPath)
            {
                foreach (var leftResult in JPath.Evaluate(leftPath, root, t, settings))
                {
                    return EvaluateMatch(root, t, settings, leftResult);
                }
            }
            else if (Left is JsonNode left)
            {
                return EvaluateMatch(root, t, settings, left);
            }
            else
            {
                return EvaluateMatch(root, t, settings, null);
            }

            return false;

            bool EvaluateMatch(JsonNode root, JsonNode? t, JsonSelectSettings? settings, JsonNode? leftResult)
            {
                if (Right is List<PathFilter> right)
                {
                    IEnumerable<JsonNode?> rightResults = JPath.Evaluate(right, root, t, settings);

                    foreach (var rightResult in rightResults)
                    {
                        if (MatchTokens(leftResult, rightResult, settings))
                        {
                            return true;
                        }
                    }
                }
                else if (Right is JsonNode rightNode)
                {
                    return MatchTokens(leftResult, rightNode, settings);
                }
                else
                {
                    return MatchTokens(leftResult, null, settings);
                }

                return false;
            }
        }

        private bool MatchTokens(JsonNode? leftResult, JsonNode? rightResult, JsonSelectSettings? settings)
        {
            if ((leftResult is JsonValue || leftResult is null) && (rightResult is JsonValue || rightResult is null))
            {
                var leftValue = leftResult is JsonValue ? (JsonValue)leftResult : null;
                var rightValue = rightResult is JsonValue ? (JsonValue)rightResult : null;
                switch (Operator)
                {
                    case QueryOperator.RegexEquals:
                        if (RegexEquals(leftValue, rightValue, settings))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.Equals:
                        if (EqualsWithStringCoercion(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.StrictEquals:
                        if (EqualsWithStrictMatch(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.NotEquals:
                        if (!EqualsWithStringCoercion(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.StrictNotEquals:
                        if (!EqualsWithStrictMatch(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThan:
                        if (Compare(leftValue, rightValue) > 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThanOrEquals:
                        if (Compare(leftValue, rightValue) >= 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThan:
                        if (Compare(leftValue, rightValue) < 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThanOrEquals:
                        if (Compare(leftValue, rightValue) <= 0)
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

        private static bool RegexEquals(JsonValue? input, JsonValue? pattern, JsonSelectSettings? settings)
        {
            if (input is null || pattern is null)
            {
                return false;
            }

            if (input.GetValueKind() != JsonValueKind.String || pattern.GetValueKind() != JsonValueKind.String)
            {
                return false;
            }

            var regexText = (string)pattern!;
            int patternOptionDelimiterIndex = regexText.LastIndexOf('/');

            string patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
            string optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

            TimeSpan timeout = settings?.RegexMatchTimeout ?? Regex.InfiniteMatchTimeout;
            return Regex.IsMatch((string)input!, patternText, GetRegexOptions(optionsText), timeout);

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

        internal static bool EqualsWithStringCoercion(JsonValue? value, JsonValue? queryValue)
        {
            if (value is null && queryValue is null)
            {
                return true;
            }

            if (value is null || queryValue is null)
            {
                return false;
            }

            if (JsonValue.DeepEquals(value, queryValue))
            {
                return true;
            }

            // Handle comparing an integer with a float
            // e.g. Comparing 1 and 1.0
            if (value.GetValueKind() == JsonValueKind.Number && queryValue.GetValueKind() == JsonValueKind.Number)
            {
                return value.GetValue<double>() == queryValue.GetValue<double>();
            }

            if (queryValue.GetValueKind() != JsonValueKind.String)
            {
                return false;
            }

            string queryValueString = (string)queryValue!;

            var currentValueString = (string?)value;

            return string.Equals(currentValueString, queryValueString, StringComparison.Ordinal);
        }

        internal static bool EqualsWithStrictMatch(JsonValue? value, JsonValue? queryValue)
        {
            if (value is null && queryValue is null)
            {
                return true;
            }

            if (value is null || queryValue is null)
            {
                return false;
            }

            if (value.GetValueKind() == queryValue.GetValueKind())
            {
                return JsonValue.DeepEquals(value, queryValue);
            }

            // num/string comparison
            if (TryGetNumberValue(value, out var leftNum) &&
                TryGetNumberValue(queryValue, out var rightNum))
            {
                return leftNum == rightNum;
            }

            return value.Equals(queryValue);
        }

        internal static int Compare(JsonValue? leftValue, JsonValue? rightValue)
        {
            if (leftValue is null && rightValue is null)
            {
                return 0;
            }

            if (leftValue is null)
            {
                return -1;
            }

            if (rightValue is null)
            {
                return 1;
            }

            if (leftValue.GetValueKind() == rightValue.GetValueKind())
            {
                switch (leftValue.GetValueKind())
                {
                    case JsonValueKind.False:
                    case JsonValueKind.True:
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                        return 0;
                    case JsonValueKind.String:
                        return ((string?)leftValue)!.CompareTo(((string?)rightValue));
                    case JsonValueKind.Number:
                        return ((double)leftValue).CompareTo(((double)rightValue));
                    default:
                        throw new InvalidOperationException($"Unknown json value kind: {leftValue.GetValueKind()}");
                }
            }

            // num/string comparison
            if (TryGetNumberValue(leftValue, out var leftNum) &&
                TryGetNumberValue(rightValue, out var rightNum))
            {
                return leftNum.CompareTo(rightNum);
            }

            return -1;
        }

        internal static bool TryGetNumberValue(JsonValue value, out double num)
        {
            if (value.GetValueKind() == JsonValueKind.Number)
            {
                num = ((double)value);
                return true;
            }
            if (value.GetValueKind() == JsonValueKind.String &&
                Double.TryParse(((string?)value), out num))
            {
                return true;
            }
            num = default;
            return false;
        }
    }
}