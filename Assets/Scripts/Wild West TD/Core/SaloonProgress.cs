using UnityEngine;

// Stores the small set of local progression flags. Audio preferences are kept separately.
namespace WildWestTD
{
    public static class SaloonProgress
    {
        public const string KeyPreference = "LastChance.EntranceKey";
        public const string DoorPreference = "LastChance.EntranceOpen";
        public static bool TutorialComplete
        {
            get
            {
                return PlayerPrefs.GetInt("LastChance.TutorialComplete", 0) == 1;
            }
        }

        public static void CompleteTutorial()
        {
            PlayerPrefs.SetInt("LastChance.TutorialComplete", 1);
            PlayerPrefs.Save();
        }

        public static void WipeProgress()
        {
            SaloonEconomy.Reset();
            PlayerPrefs.DeleteKey("LastChance.TutorialComplete");
            PlayerPrefs.DeleteKey("LastChance.Unlocked");
            PlayerPrefs.DeleteKey(KeyPreference);
            PlayerPrefs.DeleteKey(DoorPreference);
            PlayerPrefs.Save();
        }

        public static bool HasEntranceKey
        {
            get
            {
                return PlayerPrefs.GetInt(KeyPreference, 0) == 1;
            }
        }

        public static bool EntranceOpen
        {
            get
            {
                return HasEntranceKey && PlayerPrefs.GetInt(DoorPreference, 0) == 1;
            }
        }

        public static void AwardEntranceKey()
        {
            PlayerPrefs.SetInt(KeyPreference, 1);
            PlayerPrefs.Save();
        }

        public static bool UnlockEntrance()
        {
            if (!HasEntranceKey)
            {
                return false;
            }

            PlayerPrefs.SetInt(DoorPreference, 1);
            PlayerPrefs.Save();
            return true;
        }
    }
}

