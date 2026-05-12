using System;
using UnityEngine;
using RaisingSim;

namespace RaisingSim.Core
{
    [Serializable]
    public class PlayerState
    {
        public int year = 1;
        public string season = "spring";
        public int actionCount;
        public string currentFestivalId = "";
        public float lastFestivalScore;
        public RuntimeStat[] stats = new RuntimeStat[0];

        public PlayerState()
        {
        }

        public PlayerState(InitialStatsDatabase initialStatsDatabase)
        {
            Initialize(initialStatsDatabase);
        }

        public void Initialize(InitialStatsDatabase initialStatsDatabase)
        {
            if (initialStatsDatabase == null || initialStatsDatabase.stats == null)
            {
                Debug.LogError("[PlayerState] InitialStatsDatabase가 비어 있어 스탯을 초기화할 수 없습니다.");
                stats = new RuntimeStat[0];
                return;
            }

            stats = new RuntimeStat[initialStatsDatabase.stats.Length];
            for (int i = 0; i < initialStatsDatabase.stats.Length; i++)
            {
                StatValue source = initialStatsDatabase.stats[i];
                stats[i] = new RuntimeStat
                {
                    stat = source.stat,
                    value = source.initial_value,
                    hasMin = source.has_min,
                    min = source.min,
                    hasMax = source.has_max,
                    max = source.max
                };

                stats[i].value = ClampStat(stats[i]);
            }
        }

        public float GetStat(string statId)
        {
            int index = FindStatIndex(statId);
            if (index < 0)
            {
                Debug.LogError($"[PlayerState] 알 수 없는 스탯입니다: {statId}");
                return 0f;
            }

            return stats[index].value;
        }

        public void SetStat(string statId, float value)
        {
            int index = FindStatIndex(statId);
            if (index < 0)
            {
                Debug.LogError($"[PlayerState] 알 수 없는 스탯입니다: {statId}");
                return;
            }

            stats[index].value = ClampStat(stats[index], value);
        }

        public void AddStat(string statId, float amount)
        {
            int index = FindStatIndex(statId);
            if (index < 0)
            {
                Debug.LogError($"[PlayerState] 알 수 없는 스탯입니다: {statId}");
                return;
            }

            stats[index].value = ClampStat(stats[index], stats[index].value + amount);
        }

        public bool HasStat(string statId)
        {
            return FindStatIndex(statId) >= 0;
        }

        private int FindStatIndex(string statId)
        {
            if (string.IsNullOrWhiteSpace(statId) || stats == null)
            {
                return -1;
            }

            for (int i = 0; i < stats.Length; i++)
            {
                if (stats[i] != null && stats[i].stat == statId)
                {
                    return i;
                }
            }

            return -1;
        }

        private float ClampStat(RuntimeStat stat)
        {
            return ClampStat(stat, stat.value);
        }

        private float ClampStat(RuntimeStat stat, float value)
        {
            if (stat.hasMin && stat.stat != StatManager.Money)
            {
                value = Mathf.Max(stat.min, value);
            }

            if (stat.hasMax && stat.stat != StatManager.Stress)
            {
                value = Mathf.Min(stat.max, value);
            }

            return value;
        }
    }

    [Serializable]
    public class RuntimeStat
    {
        public string stat;
        public float value;
        public bool hasMin;
        public float min;
        public bool hasMax;
        public float max;
    }
}
