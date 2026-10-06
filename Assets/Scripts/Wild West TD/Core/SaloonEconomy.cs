using UnityEngine;

namespace WildWestTD
{
    /// <summary>
    /// Permanent shop progress. Gold Coins are earned by winning matches;
    /// they are separate from the cash used to build towers during a match.
    /// </summary>
    public static class SaloonEconomy
    {
        public const string CoinsPreference = "LastChance.GoldCoins";
        public const string TowersPreference = "LastChance.UnlockedTowers";
        public const int UnlockPrice = 50;
        const int StartingTowers = 15; // The first four bits: Revolver, Sniper, Dynamite, Lasso.
        public static int GoldCoins => Mathf.Max(0, PlayerPrefs.GetInt(CoinsPreference, 0));

        // Test access lasts only for this Play session and never changes saved ownership.
        public static bool TemporaryUnlockAll { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetTestSession() { TemporaryUnlockAll = false; }
        public static void ToggleTemporaryUnlock() { TemporaryUnlockAll = !TemporaryUnlockAll; }
        public static bool IsUnlocked(int type) { return type >= 0 && type < FrontierRules.Prices.Length && (TemporaryUnlockAll || IsPermanentlyUnlocked(type)); }

        public static bool IsPermanentlyUnlocked(int type)
        {
            if (type < 0 || type >= FrontierRules.Prices.Length)
            {
                return false;
            }

            int owned = PlayerPrefs.GetInt(TowersPreference, StartingTowers) | StartingTowers;
            return (owned & (1 << type)) != 0;
        }

        public static int VictoryReward(int difficulty)
        {
            switch (difficulty)
            {
                case 0:
                    return 50;
                case 1:
                    return 75;
                case 2:
                    return 100;
                default:
                    return 0; // Training, defeats and abandoned matches earn no coins.
            }
        }

        public static void AwardVictory(int difficulty)
        {
            int reward = VictoryReward(difficulty);
            if (reward == 0)
            {
                return;
            }

            PlayerPrefs.SetInt(CoinsPreference, GoldCoins + reward);
            PlayerPrefs.Save();
        }

        public static bool UnlockTower(int type)
        {
            if (type < 4 || type >= FrontierRules.Prices.Length || IsPermanentlyUnlocked(type) || GoldCoins < UnlockPrice)
            {
                return false;
            }

            int owned = PlayerPrefs.GetInt(TowersPreference, StartingTowers) | StartingTowers;
            PlayerPrefs.SetInt(CoinsPreference, GoldCoins - UnlockPrice);
            PlayerPrefs.SetInt(TowersPreference, owned | (1 << type));
            PlayerPrefs.Save();
            return true;
        }

        public static void Reset()
        {
            TemporaryUnlockAll = false;
            PlayerPrefs.DeleteKey(CoinsPreference);
            PlayerPrefs.DeleteKey(TowersPreference);
        }
    }
}

