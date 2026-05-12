using UnityEngine;

namespace RaisingSim
{
    public class GameDataLoadTest : MonoBehaviour
    {
        private void Start()
        {
            GameDataSet data = GameDataLoader.LoadAll();

            Debug.Log(
                "[GameDataLoadTest] Loaded GameData counts - " +
                $"initial_stats: {Count(data.initialStats?.stats)}, " +
                $"actions: {Count(data.actions?.actions)}, " +
                $"events: {Count(data.events?.events)}, " +
                $"festivals: {Count(data.festivals?.festivals)}, " +
                $"festival_rewards: {Count(data.festivalRewards?.rewards)}, " +
                $"endings: {Count(data.endings?.endings)}"
            );
        }

        private static int Count<T>(T[] items)
        {
            return items == null ? 0 : items.Length;
        }
    }
}
