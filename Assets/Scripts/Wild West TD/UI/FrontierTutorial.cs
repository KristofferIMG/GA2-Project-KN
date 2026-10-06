using UnityEngine;

// Provides one readable lesson before each of the ten training waves.
namespace WildWestTD
{
    public partial class FrontierGame
    {
        int acknowledgedTutorialWave = -1;
        public bool IsTutorial
        {
            get
            {
                return Active && battle.difficulty < 0;
            }
        }

        public bool TutorialNeedsExplanation
        {
            get
            {
                return IsTutorial && !battle.running && !battle.won && !battle.lost && acknowledgedTutorialWave < battle.wave;
            }
        }

        readonly string[] lessonTitles =
        {
            "Welcome to Marshal Finch's table",
            "Gold, health and the upgrade menu",
            "The Sharpshooter",
            "Terrain and placement",
            "Roadrunners and the Lasso Deputy",
            "The Dynamite Prospector",
            "Investing in a Frontier Bank",
            "Manual waves and Auto Start",
            "Pause, speed and escape",
            "Final lesson: the Ironhide"
        };
        readonly string[] lessons =
        {
            "Protect the town for 10 waves to unlock Easy. You have $200 for training and 5 health. Buy a Revolver from the left-side list, then click an open tile near the trail. It fires quickly at short-to-medium range. One escaping enemy costs one health.",
            "The top-left panel shows gold and health. Kills and cleared waves earn gold. Click a placed tower to open its details: three upgrades improve it, and selling returns 70% of your investment. A Revolver upgrade is a useful early purchase.",
            "The Sharpshooter costs $75 and hits much harder but fires slowly. It must stand on height 3 and can shoot across the whole board. The grey three-tile circle is a blind zone: it cannot shoot enemies inside it. It prioritizes tough enemies.",
            "Higher ground gives normal towers more range: +15% at height 2 and +30% at height 3. You cannot build on trails, bridges, water or another tower. A red preview means the tile is invalid. Right click cancels placement.",
            "Fast Roadrunners start on wave 5 with less health. The $65 Lasso Deputy slows targets by 45% for 2.2 seconds and spreads slows between enemies. Pair it with a Revolver or Sharpshooter. Every fifth wave also has tougher Wanted enemies.",
            "The $95 Dynamite Prospector deals splash damage within 1.35 tiles of its target. Put it near bends or intersections where enemies bunch together. Slows help keep groups close enough for an explosion to hit several bandits.",
            "The Frontier Bank in the list is an optional income building, not a combat tower. Unlock it at Otis the bartender for 50 Gold Coins after winning Easy. A $400 Frontier Bank earns $40 after each clear, with upgrades to $60/$80/$100. Buy defenses first: banks cannot stop enemies. Gatling, Shotgun and Bounty Hunter are also unlocked at the bar; each costs 50 Gold Coins.",
            "START WAVE or Space begins the next wave when you are ready. AUTO START has an explicit ON/OFF button and waits 1.8 seconds after a clear. During training it also waits for you to acknowledge each lesson, so explanations cannot be skipped by auto start.",
            "Use the 1x/2x button to change battle speed. Escape or the pause button stops combat and opens music and sound switches. The preview lists the next enemies and rewards. Plan upgrades before pressing Start; losing all five health means retrying.",
            "Ironhides arrive on wave 10: much more health, slower movement. Sharpshooters help against them, while lassos buy time and dynamite handles nearby groups. Clear this final wave to finish training, then visit Dusty Joe for Easy difficulty."
        };
        public string LessonTitle => "TRAINING " + (battle.wave + 1) + " / 10 - " + lessonTitles[Mathf.Clamp(battle.wave, 0, 9)];
        public string LessonText => lessons[Mathf.Clamp(battle.wave, 0, 9)];
        public bool CanAcknowledgeLesson => battle.wave > 0 || battle.towers.Exists(tower => tower.type != 4);

        public void AcknowledgeLesson()
        {
            if (TutorialNeedsExplanation && CanAcknowledgeLesson)
            {
                acknowledgedTutorialWave = battle.wave;
                SaloonAudio.Play("click");
            }
        }
    }
}

