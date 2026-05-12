using UnityEngine;
using RaisingSim;

namespace RaisingSim.Core
{
    public class CoreLogicTest : MonoBehaviour
    {
        private void Start()
        {
            GameDataSet data = GameDataLoader.LoadAll();
            PlayerState playerState = new PlayerState(data.initialStats)
            {
                year = 1,
                season = "spring",
                actionCount = 0
            };

            ExpectTrue(
                "year=1;season=spring",
                ConditionEvaluator.Evaluate("year=1;season=spring", playerState)
            );

            ExpectFalse(
                "motherHealth<=25",
                ConditionEvaluator.Evaluate("motherHealth<=25", playerState)
            );

            DeltaApplier.Apply(playerState, "money:+140;stress:+12;reputation:-6");

            Debug.Log(
                "[CoreLogicTest] Delta 적용 후 - " +
                $"money: {playerState.GetStat(StatManager.Money)}, " +
                $"stress: {playerState.GetStat(StatManager.Stress)}, " +
                $"reputation: {playerState.GetStat(StatManager.Reputation)}"
            );

            string formula = "intelligence*0.5+elegance*0.3+reputation*0.2-stress*4";
            float score = NumericExpressionEvaluator.Evaluate(formula, playerState);
            Debug.Log($"[CoreLogicTest] Festival formula result: {score}");
        }

        private static void ExpectTrue(string label, bool value)
        {
            if (value)
            {
                Debug.Log($"[CoreLogicTest] PASS: {label}");
            }
            else
            {
                Debug.LogError($"[CoreLogicTest] FAIL: {label} should be true");
            }
        }

        private static void ExpectFalse(string label, bool value)
        {
            if (!value)
            {
                Debug.Log($"[CoreLogicTest] PASS: {label}");
            }
            else
            {
                Debug.LogError($"[CoreLogicTest] FAIL: {label} should be false");
            }
        }
    }
}
