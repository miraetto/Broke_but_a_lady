using System.Collections.Generic;
using UnityEngine;
using RaisingSim;

namespace RaisingSim.Core
{
    public class EventSelector
    {
        private readonly EventDatabase eventDatabase;
        private readonly List<string> seenEventIds = new List<string>();

        public EventSelector(EventDatabase eventDatabase)
        {
            this.eventDatabase = eventDatabase;
        }

        public EventData SelectEvent(string trigger, PlayerState playerState)
        {
            if (eventDatabase == null || eventDatabase.events == null)
            {
                Debug.LogError("[EventSelector] EventDatabase가 비어 있습니다.");
                return null;
            }

            List<EventData> candidates = new List<EventData>();
            for (int i = 0; i < eventDatabase.events.Length; i++)
            {
                EventData eventData = eventDatabase.events[i];
                if (eventData == null || eventData.trigger != trigger)
                {
                    continue;
                }

                candidates.Add(eventData);
            }

            candidates.Sort((left, right) => right.priority.CompareTo(left.priority));

            for (int i = 0; i < candidates.Count; i++)
            {
                EventData eventData = candidates[i];
                if (eventData.once && HasSeen(eventData.event_id))
                {
                    continue;
                }

                if (!ConditionEvaluator.Evaluate(eventData.condition, playerState))
                {
                    continue;
                }

                if (eventData.once)
                {
                    seenEventIds.Add(eventData.event_id);
                }

                return eventData;
            }

            return null;
        }

        public void LogEvent(EventData eventData, string label)
        {
            if (eventData == null)
            {
                Debug.Log($"[EventSelector] No event selected for {label}");
                return;
            }

            Debug.Log($"[EventSelector] {label}: {eventData.event_id} / {eventData.name}");
            if (!string.IsNullOrWhiteSpace(eventData.dialogue_text))
            {
                Debug.Log($"[EventSelector] Dialogue: {eventData.dialogue_text}");
            }
        }

        private bool HasSeen(string eventId)
        {
            for (int i = 0; i < seenEventIds.Count; i++)
            {
                if (seenEventIds[i] == eventId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
