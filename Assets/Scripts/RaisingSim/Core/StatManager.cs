namespace RaisingSim.Core
{
    public static class StatManager
    {
        public const string Money = "money";
        public const string Intelligence = "intelligence";
        public const string Elegance = "elegance";
        public const string Charm = "charm";
        public const string Reputation = "reputation";
        public const string Stress = "stress";
        public const string Pride = "pride";
        public const string MotherHealth = "motherHealth";

        public static readonly string[] SupportedStats =
        {
            Money,
            Intelligence,
            Elegance,
            Charm,
            Reputation,
            Stress,
            Pride,
            MotherHealth
        };

        public static bool IsSupportedStat(string statId)
        {
            if (string.IsNullOrWhiteSpace(statId))
            {
                return false;
            }

            for (int i = 0; i < SupportedStats.Length; i++)
            {
                if (SupportedStats[i] == statId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
