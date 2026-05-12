using System.Text;
using UnityEngine;
using RaisingSim;

namespace RaisingSim.Core
{
    public class ActionRunner
    {
        private readonly ActionDatabase actionDatabase;
        private readonly PlayerState playerState;

        public ActionRunner(ActionDatabase actionDatabase, PlayerState playerState)
        {
            this.actionDatabase = actionDatabase;
            this.playerState = playerState;
        }

        public bool RunAction(string actionId)
        {
            return RunActionDetailed(actionId).success;
        }

        public ActionRunResult RunActionDetailed(string actionId)
        {
            ActionData action = FindAction(actionId);
            if (action == null)
            {
                Debug.LogError($"[ActionRunner] action_id를 찾을 수 없습니다: {actionId}");
                return ActionRunResult.Fail($"action_id를 찾을 수 없습니다: {actionId}");
            }

            if (!ConditionEvaluator.Evaluate(action.unlock_condition, playerState))
            {
                Debug.LogError($"[ActionRunner] 행동 조건을 만족하지 못했습니다: {action.action_id} / {action.unlock_condition}");
                return ActionRunResult.Fail($"행동 조건을 만족하지 못했습니다: {action.action_id}");
            }

            Debug.Log($"[ActionRunner] Run action: {action.action_id} / {action.name}");
            string statChanges = ApplyActionDeltas(action);
            playerState.actionCount++;

            if (!string.IsNullOrWhiteSpace(action.result_text))
            {
                Debug.Log($"[ActionRunner] Result: {action.result_text}");
            }

            return new ActionRunResult
            {
                success = true,
                action = action,
                statChangesText = statChanges
            };
        }

        private ActionData FindAction(string actionId)
        {
            if (actionDatabase == null || actionDatabase.actions == null)
            {
                Debug.LogError("[ActionRunner] ActionDatabase가 비어 있습니다.");
                return null;
            }

            for (int i = 0; i < actionDatabase.actions.Length; i++)
            {
                ActionData action = actionDatabase.actions[i];
                if (action != null && action.action_id == actionId)
                {
                    return action;
                }
            }

            return null;
        }

        private string ApplyActionDeltas(ActionData action)
        {
            if (action.deltas == null || action.deltas.Length == 0)
            {
                Debug.Log("[ActionRunner] Applied stat changes: none");
                return "none";
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < action.deltas.Length; i++)
            {
                StatDelta delta = action.deltas[i];
                if (delta == null)
                {
                    continue;
                }

                if (!playerState.HasStat(delta.stat))
                {
                    Debug.LogError($"[ActionRunner] 알 수 없는 스탯 ID입니다: {delta.stat}");
                    continue;
                }

                float before = playerState.GetStat(delta.stat);
                playerState.AddStat(delta.stat, delta.amount);
                float after = playerState.GetStat(delta.stat);

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(delta.stat);
                builder.Append(": ");
                builder.Append(before);
                builder.Append(" -> ");
                builder.Append(after);
                builder.Append(" (");
                builder.Append(delta.amount >= 0f ? "+" : "");
                builder.Append(delta.amount);
                builder.Append(")");
            }

            string statChanges = builder.Length == 0 ? "none" : builder.ToString();
            Debug.Log($"[ActionRunner] Applied stat changes: {statChanges}");
            return statChanges;
        }
    }

    public class ActionRunResult
    {
        public bool success;
        public ActionData action;
        public string statChangesText;
        public string errorMessage;

        public static ActionRunResult Fail(string errorMessage)
        {
            return new ActionRunResult
            {
                success = false,
                errorMessage = errorMessage
            };
        }
    }
}
