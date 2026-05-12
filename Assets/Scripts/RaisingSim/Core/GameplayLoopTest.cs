using UnityEngine;
using RaisingSim;

namespace RaisingSim.Core
{
    public class GameplayLoopTest : MonoBehaviour
    {
        private readonly string[] scriptedActions =
        {
            "lesson_etiquette_day",
            "study_history_day",
            "speech_day",
            "sewing_day",
            "care_mother_day",
            "rest_night"
        };

        private void Start()
        {
            GameDataSet data = GameDataLoader.LoadAll();
            RaisingSimGameController controller = new RaisingSimGameController(data);

            Debug.Log("[GameplayLoopTest] Start minimal non-UI gameplay loop.");

            for (int i = 0; i < scriptedActions.Length; i++)
            {
                string actionId = scriptedActions[i];
                Debug.Log($"[GameplayLoopTest] Action {i + 1}/{scriptedActions.Length}: {actionId}");

                bool didRun = controller.RunAction(actionId);
                if (!didRun)
                {
                    Debug.LogError($"[GameplayLoopTest] Scripted action failed: {actionId}");
                    break;
                }

                if (controller.IsEnded)
                {
                    Debug.Log("[GameplayLoopTest] Game ended by instant ending.");
                    break;
                }
            }

            if (!controller.IsEnded)
            {
                controller.RunSpringFestivalIfReady();
            }

            controller.LogFinalStats();
            Debug.Log("[GameplayLoopTest] End minimal non-UI gameplay loop.");
        }
    }
}
