using System;
using System.Globalization;
using UnityEngine;

namespace RaisingSim.Core
{
    public static class NumericExpressionEvaluator
    {
        public static float Evaluate(string expression, PlayerState playerState)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                Debug.LogError("[NumericExpressionEvaluator] 수식이 비어 있습니다.");
                return 0f;
            }

            if (playerState == null)
            {
                Debug.LogError("[NumericExpressionEvaluator] PlayerState가 null입니다.");
                return 0f;
            }

            Parser parser = new Parser(expression, playerState);
            float value = parser.ParseExpression();
            parser.SkipWhitespace();

            if (!parser.IsAtEnd)
            {
                Debug.LogError($"[NumericExpressionEvaluator] 수식 끝에 읽지 못한 문자가 있습니다: {expression}");
                return 0f;
            }

            return value;
        }

        private class Parser
        {
            private readonly string expression;
            private readonly PlayerState playerState;
            private int index;

            public Parser(string expression, PlayerState playerState)
            {
                this.expression = expression;
                this.playerState = playerState;
            }

            public bool IsAtEnd => index >= expression.Length;

            public float ParseExpression()
            {
                float value = ParseTerm();

                while (true)
                {
                    SkipWhitespace();
                    if (Match('+'))
                    {
                        value += ParseTerm();
                    }
                    else if (Match('-'))
                    {
                        value -= ParseTerm();
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            public void SkipWhitespace()
            {
                while (!IsAtEnd && char.IsWhiteSpace(expression[index]))
                {
                    index++;
                }
            }

            private float ParseTerm()
            {
                float value = ParseFactor();

                while (true)
                {
                    SkipWhitespace();
                    if (Match('*'))
                    {
                        value *= ParseFactor();
                    }
                    else if (Match('/'))
                    {
                        float divisor = ParseFactor();
                        if (Mathf.Abs(divisor) <= 0.0001f)
                        {
                            Debug.LogError($"[NumericExpressionEvaluator] 0으로 나눌 수 없습니다: {expression}");
                            return 0f;
                        }

                        value /= divisor;
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            private float ParseFactor()
            {
                SkipWhitespace();

                if (Match('+'))
                {
                    return ParseFactor();
                }

                if (Match('-'))
                {
                    return -ParseFactor();
                }

                if (Match('('))
                {
                    float value = ParseExpression();
                    SkipWhitespace();
                    if (!Match(')'))
                    {
                        Debug.LogError($"[NumericExpressionEvaluator] 닫는 괄호가 없습니다: {expression}");
                    }

                    return value;
                }

                if (IsAtEnd)
                {
                    Debug.LogError($"[NumericExpressionEvaluator] 수식이 갑자기 끝났습니다: {expression}");
                    return 0f;
                }

                if (char.IsDigit(expression[index]) || expression[index] == '.')
                {
                    return ParseNumber();
                }

                if (char.IsLetter(expression[index]) || expression[index] == '_')
                {
                    return ParseVariable();
                }

                Debug.LogError($"[NumericExpressionEvaluator] 알 수 없는 토큰입니다: {expression[index]} in {expression}");
                index++;
                return 0f;
            }

            private float ParseNumber()
            {
                int startIndex = index;
                while (!IsAtEnd && (char.IsDigit(expression[index]) || expression[index] == '.'))
                {
                    index++;
                }

                string numberText = expression.Substring(startIndex, index - startIndex);
                if (!float.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                {
                    Debug.LogError($"[NumericExpressionEvaluator] 숫자를 읽을 수 없습니다: {numberText}");
                    return 0f;
                }

                return value;
            }

            private float ParseVariable()
            {
                int startIndex = index;
                while (!IsAtEnd && (char.IsLetterOrDigit(expression[index]) || expression[index] == '_'))
                {
                    index++;
                }

                string variableName = expression.Substring(startIndex, index - startIndex);
                if (!playerState.HasStat(variableName))
                {
                    Debug.LogError($"[NumericExpressionEvaluator] 알 수 없는 변수입니다: {variableName}");
                    return 0f;
                }

                return playerState.GetStat(variableName);
            }

            private bool Match(char expected)
            {
                if (IsAtEnd || expression[index] != expected)
                {
                    return false;
                }

                index++;
                return true;
            }
        }
    }
}
