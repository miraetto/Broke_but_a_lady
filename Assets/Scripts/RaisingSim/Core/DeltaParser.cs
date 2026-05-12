using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using RaisingSim;

namespace RaisingSim.Core
{
    public static class DeltaParser
    {
        public static StatDelta[] Parse(string deltaText)
        {
            if (string.IsNullOrWhiteSpace(deltaText) || deltaText.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                return new StatDelta[0];
            }

            string[] parts = deltaText.Split(';');
            List<StatDelta> deltas = new List<StatDelta>();

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                if (part.Length == 0)
                {
                    continue;
                }

                int separatorIndex = part.IndexOf(':');
                if (separatorIndex <= 0 || separatorIndex == part.Length - 1)
                {
                    Debug.LogError($"[DeltaParser] 잘못된 delta 형식입니다: {part}");
                    continue;
                }

                string statId = part.Substring(0, separatorIndex).Trim();
                string amountText = part.Substring(separatorIndex + 1).Trim();

                if (!StatManager.IsSupportedStat(statId))
                {
                    Debug.LogError($"[DeltaParser] 알 수 없는 스탯 ID입니다: {statId}");
                    continue;
                }

                if (!float.TryParse(amountText, NumberStyles.Float, CultureInfo.InvariantCulture, out float amount))
                {
                    Debug.LogError($"[DeltaParser] delta 숫자를 읽을 수 없습니다: {part}");
                    continue;
                }

                deltas.Add(new StatDelta { stat = statId, amount = amount });
            }

            return deltas.ToArray();
        }
    }
}
