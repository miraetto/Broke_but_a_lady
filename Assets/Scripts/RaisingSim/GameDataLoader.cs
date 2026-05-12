using System;
using UnityEngine;

namespace RaisingSim
{
    public class GameDataSet
    {
        public InitialStatsDatabase initialStats;
        public ActionDatabase actions;
        public EventDatabase events;
        public FestivalDatabase festivals;
        public FestivalRewardDatabase festivalRewards;
        public EndingDatabase endings;
    }

    public static class GameDataLoader
    {
        private const string ResourceRoot = "GameData/";

        public static GameDataSet LoadAll()
        {
            return new GameDataSet
            {
                initialStats = LoadDatabase<InitialStatsDatabase>("initial_stats"),
                actions = LoadDatabase<ActionDatabase>("actions"),
                events = LoadDatabase<EventDatabase>("events"),
                festivals = LoadDatabase<FestivalDatabase>("festivals"),
                festivalRewards = LoadDatabase<FestivalRewardDatabase>("festival_rewards"),
                endings = LoadDatabase<EndingDatabase>("endings")
            };
        }

        public static T LoadDatabase<T>(string resourceName) where T : class, new()
        {
            string resourcePath = ResourceRoot + resourceName;
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);

            if (asset == null)
            {
                Debug.LogError($"[GameDataLoader] Resources/{resourcePath}.json 파일을 찾을 수 없습니다.");
                return new T();
            }

            if (string.IsNullOrWhiteSpace(asset.text))
            {
                Debug.LogError($"[GameDataLoader] Resources/{resourcePath}.json 파일이 비어 있습니다.");
                return new T();
            }

            try
            {
                T database = JsonUtility.FromJson<T>(asset.text);
                if (database == null)
                {
                    Debug.LogError($"[GameDataLoader] Resources/{resourcePath}.json 파싱 결과가 null입니다.");
                    return new T();
                }

                return database;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[GameDataLoader] Resources/{resourcePath}.json 파싱 실패: {exception.Message}");
                return new T();
            }
        }
    }
}
