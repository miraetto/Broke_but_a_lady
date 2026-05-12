using UnityEngine;
using RaisingSim;

namespace RaisingSim.Core
{
    public static class DeltaApplier
    {
        public static void Apply(PlayerState playerState, string deltaText)
        {
            Apply(playerState, DeltaParser.Parse(deltaText));
        }

        public static void Apply(PlayerState playerState, StatDelta[] deltas)
        {
            if (playerState == null)
            {
                Debug.LogError("[DeltaApplier] PlayerState가 null입니다.");
                return;
            }

            if (deltas == null)
            {
                return;
            }

            for (int i = 0; i < deltas.Length; i++)
            {
                StatDelta delta = deltas[i];
                if (delta == null)
                {
                    continue;
                }

                if (!StatManager.IsSupportedStat(delta.stat))
                {
                    Debug.LogError($"[DeltaApplier] 알 수 없는 스탯 ID입니다: {delta.stat}");
                    continue;
                }

                playerState.AddStat(delta.stat, delta.amount);
            }
        }
    }
}
