using UnityEngine;
using RaisingSim;

namespace RaisingSim.Core
{
    public class FestivalRunner
    {
        private readonly FestivalDatabase festivalDatabase;
        private readonly FestivalRewardDatabase rewardDatabase;
        private readonly PlayerState playerState;

        public FestivalRunner(
            FestivalDatabase festivalDatabase,
            FestivalRewardDatabase rewardDatabase,
            PlayerState playerState)
        {
            this.festivalDatabase = festivalDatabase;
            this.rewardDatabase = rewardDatabase;
            this.playerState = playerState;
        }

        public FestivalRewardData RunFestival(string festivalId, EventSelector eventSelector)
        {
            return RunFestivalDetailed(festivalId, eventSelector).reward;
        }

        public FestivalRunResult RunFestivalDetailed(string festivalId, EventSelector eventSelector)
        {
            FestivalData festival = FindFestival(festivalId);
            if (festival == null)
            {
                Debug.LogError($"[FestivalRunner] festival_id를 찾을 수 없습니다: {festivalId}");
                return FestivalRunResult.Fail($"festival_id를 찾을 수 없습니다: {festivalId}");
            }

            float score = NumericExpressionEvaluator.Evaluate(festival.score_formula, playerState);
            playerState.currentFestivalId = festival.festival_id;
            playerState.lastFestivalScore = score;

            FestivalRewardData reward = FindReward(festival.result_group, score);
            if (reward == null)
            {
                Debug.LogError($"[FestivalRunner] 축제 보상 구간을 찾을 수 없습니다: {festival.festival_id}, score={score}, group={festival.result_group}");
                return FestivalRunResult.Fail($"축제 보상 구간을 찾을 수 없습니다: {festival.festival_id}, score={score}");
            }

            Debug.Log($"[FestivalRunner] Festival: {festival.festival_id} / {festival.name}");
            Debug.Log($"[FestivalRunner] Score: {score}, result_group: {festival.result_group}");
            Debug.Log($"[FestivalRunner] Reward delta: {reward.delta}, result_event_id: {reward.result_event_id}");

            DeltaApplier.Apply(playerState, reward.delta);

            EventData resultEvent = null;
            if (eventSelector != null)
            {
                resultEvent = eventSelector.SelectEvent("festival_result", playerState);
                eventSelector.LogEvent(resultEvent, "festival_result");

                if (resultEvent != null && !string.IsNullOrWhiteSpace(reward.result_event_id) && resultEvent.event_id != reward.result_event_id)
                {
                    Debug.LogError($"[FestivalRunner] reward result_event_id와 선택된 festival_result 이벤트가 다릅니다: reward={reward.result_event_id}, selected={resultEvent.event_id}");
                }
            }

            return new FestivalRunResult
            {
                success = true,
                festival = festival,
                score = score,
                reward = reward,
                resultEvent = resultEvent
            };
        }

        private FestivalData FindFestival(string festivalId)
        {
            if (festivalDatabase == null || festivalDatabase.festivals == null)
            {
                Debug.LogError("[FestivalRunner] FestivalDatabase가 비어 있습니다.");
                return null;
            }

            for (int i = 0; i < festivalDatabase.festivals.Length; i++)
            {
                FestivalData festival = festivalDatabase.festivals[i];
                if (festival != null && festival.festival_id == festivalId)
                {
                    return festival;
                }
            }

            return null;
        }

        private FestivalRewardData FindReward(string resultGroup, float score)
        {
            if (rewardDatabase == null || rewardDatabase.rewards == null)
            {
                Debug.LogError("[FestivalRunner] FestivalRewardDatabase가 비어 있습니다.");
                return null;
            }

            for (int i = 0; i < rewardDatabase.rewards.Length; i++)
            {
                FestivalRewardData reward = rewardDatabase.rewards[i];
                if (reward == null || reward.result_group != resultGroup)
                {
                    continue;
                }

                if (score >= reward.min_score && score <= reward.max_score)
                {
                    return reward;
                }
            }

            return null;
        }
    }

    public class FestivalRunResult
    {
        public bool success;
        public FestivalData festival;
        public float score;
        public FestivalRewardData reward;
        public EventData resultEvent;
        public string errorMessage;

        public static FestivalRunResult Fail(string errorMessage)
        {
            return new FestivalRunResult
            {
                success = false,
                errorMessage = errorMessage
            };
        }
    }
}
