using System;
using System.Globalization;
using UnityEngine;

namespace RaisingSim.Core
{
    public static class ConditionEvaluator
    {
        private static readonly string[] Operators = { ">=", "<=", "!=", "=", ">", "<" };

        public static bool Evaluate(string condition, PlayerState playerState)
        {
            if (string.IsNullOrWhiteSpace(condition) || condition.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (playerState == null)
            {
                Debug.LogError("[ConditionEvaluator] PlayerState가 null입니다.");
                return false;
            }

            string[] orGroups = condition.Split('|');
            for (int i = 0; i < orGroups.Length; i++)
            {
                if (EvaluateAndGroup(orGroups[i], playerState))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EvaluateAndGroup(string group, PlayerState playerState)
        {
            string[] clauses = group.Split(';');
            for (int i = 0; i < clauses.Length; i++)
            {
                string clause = clauses[i].Trim();
                if (clause.Length == 0 || clause.Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!EvaluateClause(clause, playerState))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool EvaluateClause(string clause, PlayerState playerState)
        {
            string op = FindOperator(clause, out int operatorIndex);
            if (op == null)
            {
                Debug.LogError($"[ConditionEvaluator] 조건 연산자를 찾을 수 없습니다: {clause}");
                return false;
            }

            string leftText = clause.Substring(0, operatorIndex).Trim();
            string rightText = clause.Substring(operatorIndex + op.Length).Trim();

            if (leftText.Length == 0 || rightText.Length == 0)
            {
                Debug.LogError($"[ConditionEvaluator] 비어 있는 조건 항입니다: {clause}");
                return false;
            }

            ConditionValue left = ResolveValue(leftText, playerState, true);
            ConditionValue right = ResolveValue(rightText, playerState, false);

            if (!left.isValid || !right.isValid)
            {
                return false;
            }

            if (left.isNumeric && right.isNumeric)
            {
                return CompareNumbers(left.numberValue, right.numberValue, op);
            }

            return CompareStrings(left.stringValue, right.stringValue, op);
        }

        private static string FindOperator(string clause, out int operatorIndex)
        {
            for (int i = 0; i < Operators.Length; i++)
            {
                operatorIndex = clause.IndexOf(Operators[i], StringComparison.Ordinal);
                if (operatorIndex >= 0)
                {
                    return Operators[i];
                }
            }

            operatorIndex = -1;
            return null;
        }

        private static ConditionValue ResolveValue(string text, PlayerState playerState, bool requireKnownIdentifier)
        {
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float numberValue))
            {
                return ConditionValue.Number(numberValue);
            }

            switch (text)
            {
                case "year":
                    return ConditionValue.Number(playerState.year);
                case "actionCount":
                    return ConditionValue.Number(playerState.actionCount);
                case "season":
                    return ConditionValue.Text(playerState.season);
                case "festival":
                    return ConditionValue.Text(playerState.currentFestivalId);
                case "score":
                    return ConditionValue.Number(playerState.lastFestivalScore);
            }

            if (playerState.HasStat(text))
            {
                return ConditionValue.Number(playerState.GetStat(text));
            }

            if (requireKnownIdentifier)
            {
                Debug.LogError($"[ConditionEvaluator] 알 수 없는 조건 변수입니다: {text}");
                return ConditionValue.Invalid();
            }

            return ConditionValue.Text(text.Trim('"'));
        }

        private static bool CompareNumbers(float left, float right, string op)
        {
            const float epsilon = 0.0001f;

            switch (op)
            {
                case ">=":
                    return left >= right;
                case "<=":
                    return left <= right;
                case "!=":
                    return Mathf.Abs(left - right) > epsilon;
                case "=":
                    return Mathf.Abs(left - right) <= epsilon;
                case ">":
                    return left > right;
                case "<":
                    return left < right;
                default:
                    Debug.LogError($"[ConditionEvaluator] 지원하지 않는 숫자 연산자입니다: {op}");
                    return false;
            }
        }

        private static bool CompareStrings(string left, string right, string op)
        {
            bool isEqual = string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

            switch (op)
            {
                case "=":
                    return isEqual;
                case "!=":
                    return !isEqual;
                default:
                    Debug.LogError($"[ConditionEvaluator] 문자열 비교에는 = 또는 != 만 사용할 수 있습니다: {op}");
                    return false;
            }
        }

        private struct ConditionValue
        {
            public bool isValid;
            public bool isNumeric;
            public float numberValue;
            public string stringValue;

            public static ConditionValue Number(float value)
            {
                return new ConditionValue
                {
                    isValid = true,
                    isNumeric = true,
                    numberValue = value,
                    stringValue = value.ToString(CultureInfo.InvariantCulture)
                };
            }

            public static ConditionValue Text(string value)
            {
                return new ConditionValue
                {
                    isValid = true,
                    isNumeric = false,
                    stringValue = value ?? string.Empty
                };
            }

            public static ConditionValue Invalid()
            {
                return new ConditionValue { isValid = false };
            }
        }
    }
}
