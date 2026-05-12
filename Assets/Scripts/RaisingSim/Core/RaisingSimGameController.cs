using System.Text;
using UnityEngine;
using RaisingSim;

namespace RaisingSim.Core
{
    public class RaisingSimGameController
    {
        private readonly ActionRunner actionRunner;
        private readonly EventSelector eventSelector;
        private readonly FestivalRunner festivalRunner;

        public PlayerState PlayerState { get; private set; }
        public bool IsEnded { get; private set; }

        public RaisingSimGameController(GameDataSet data)
        {
            PlayerState = new PlayerState(data.initialStats)
            {
                year = 1,
                season = "spring",
                actionCount = 0
            };

            actionRunner = new ActionRunner(data.actions, PlayerState);
            eventSelector = new EventSelector(data.events);
            festivalRunner = new FestivalRunner(data.festivals, data.festivalRewards, PlayerState);
        }

        public bool RunAction(string actionId)
        {
            return RunActionDetailed(actionId).success;
        }

        public GameplayActionResult RunActionDetailed(string actionId)
        {
            if (IsEnded)
            {
                Debug.LogError($"[RaisingSimGameController] 이미 종료된 게임에서 행동을 실행하려 했습니다: {actionId}");
                return GameplayActionResult.Fail($"이미 종료된 게임입니다: {actionId}");
            }

            ActionRunResult actionResult = actionRunner.RunActionDetailed(actionId);
            if (!actionResult.success)
            {
                return GameplayActionResult.Fail(actionResult.errorMessage);
            }

            EventData instantEnding = eventSelector.SelectEvent("instant_ending", PlayerState);
            if (instantEnding != null)
            {
                eventSelector.LogEvent(instantEnding, "instant_ending");
                IsEnded = true;
                return new GameplayActionResult
                {
                    success = true,
                    actionResult = actionResult,
                    instantEndingEvent = instantEnding,
                    endedGame = true
                };
            }

            EventData afterAction = eventSelector.SelectEvent("after_action", PlayerState);
            eventSelector.LogEvent(afterAction, "after_action");
            ApplyEventDefaultDelta(afterAction);

            return new GameplayActionResult
            {
                success = true,
                actionResult = actionResult,
                afterActionEvent = afterAction
            };
        }

        public void RunSpringFestivalIfReady()
        {
            RunSpringFestivalIfReadyDetailed();
        }

        public FestivalRunResult RunSpringFestivalIfReadyDetailed()
        {
            if (IsEnded)
            {
                Debug.Log("[RaisingSimGameController] 게임이 이미 종료되어 축제를 실행하지 않습니다.");
                return FestivalRunResult.Fail("게임이 이미 종료되어 축제를 실행하지 않습니다.");
            }

            if (PlayerState.actionCount < 6)
            {
                Debug.LogError($"[RaisingSimGameController] 행동 6회 전에는 봄 축제를 실행할 수 없습니다. actionCount={PlayerState.actionCount}");
                return FestivalRunResult.Fail($"행동 6회 전에는 봄 축제를 실행할 수 없습니다. actionCount={PlayerState.actionCount}");
            }

            return festivalRunner.RunFestivalDetailed("spring_y1", eventSelector);
        }

        public void LogFinalStats()
        {
            Debug.Log($"[RaisingSimGameController] Final stats: {BuildStatsText()}");
        }

        private void ApplyEventDefaultDelta(EventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(eventData.default_delta) || eventData.default_delta == "none")
            {
                return;
            }

            Debug.Log($"[RaisingSimGameController] Apply event default_delta: {eventData.default_delta}");
            DeltaApplier.Apply(PlayerState, eventData.default_delta);
        }

        private string BuildStatsText()
        {
            if (PlayerState.stats == null || PlayerState.stats.Length == 0)
            {
                return "none";
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < PlayerState.stats.Length; i++)
            {
                RuntimeStat stat = PlayerState.stats[i];
                if (stat == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(stat.stat);
                builder.Append("=");
                builder.Append(stat.value);
            }

            return builder.ToString();
        }
    }

    public class GameplayActionResult
    {
        public bool success;
        public ActionRunResult actionResult;
        public EventData instantEndingEvent;
        public EventData afterActionEvent;
        public bool endedGame;
        public string errorMessage;

        public static GameplayActionResult Fail(string errorMessage)
        {
            return new GameplayActionResult
            {
                success = false,
                errorMessage = errorMessage
            };
        }
    }
}
