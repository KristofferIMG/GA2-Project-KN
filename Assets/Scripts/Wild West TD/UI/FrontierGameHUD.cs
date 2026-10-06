using UnityEngine;

namespace WildWestTD
{
    // Only values and actions live here. Layout, colours and fonts are saved in the Canvas prefab.
    public partial class FrontierGame
    {
        public bool IsPaused => paused;
        public bool CanManageTowers => Active && !paused && !confirmExit && !battle.won && !battle.lost;

        public bool PointerOverHUD(Vector2 screen)
        {
            var ui = GetComponentInChildren<SaloonInterface>(true);
            return ui && ui.ContainsScreenPoint(screen);
        }

        public void TogglePause()
        {
            if (Active && !confirmExit && !battle.won && !battle.lost)
            {
                paused = !paused;
            }
        }

        public void ToggleSpeed()
        {
            if (CanManageTowers)
            {
                speed = speed == 1 ? 2 : 1;
            }
        }

        public void ToggleAutoStart()
        {
            if (CanManageTowers)
            {
                auto = !auto;
                SaloonAudio.Play("click");
            }
        }

        public void RequestLeaveMatch()
        {
            if (Active)
            {
                confirmExit = true;
            }
        }

        public void CancelLeaveMatch()
        {
            confirmExit = false;
        }

        public void StartWaveFromButton()
        {
            if (CanManageTowers)
            {
                StartNext();
            }
        }

        public void CloseTowerDetails()
        {
            selected = -1;
        }

        public void UpgradeSelectedTower()
        {
            if (!CanManageTowers)
            {
                return;
            }

            var tower = Selected();
            if (!battle.Upgrade(tower))
            {
                return;
            }

            Destroy(models[tower.id].gameObject);
            models.Remove(tower.id);
            CreateTower(tower);
            SaloonAudio.Play("upgrade");
        }

        public void SellSelectedTower()
        {
            if (!CanManageTowers)
            {
                return;
            }

            var tower = Selected();
            if (!battle.Sell(tower))
            {
                return;
            }

            Destroy(models[tower.id].gameObject);
            models.Remove(tower.id);
            selected = -1;
            SaloonAudio.Play("sell");
        }

        public void RefreshHUD(SaloonInterface ui)
        {
            if (!Active)
            {
                return;
            }

            ui.SetText("Resources", "TOWN  " + battle.lives + " / 5       CASH  $" + battle.gold + (SaloonProgress.HasEntranceKey ? "     KEY" : ""));
            ui.SetText("MatchName", (IsTutorial ? "TRAINING" : new[] { "EASY", "MEDIUM", "HARD" }[battle.difficulty]) + " / " + SaloonMaps.Names[battle.map]);
            ui.SetText("WaveTitle", "WAVE " + battle.wave + (battle.endless ? " / INFINITE" : " / " + FrontierRules.TotalWaves(battle.difficulty)) + "     Alive: " + battle.enemies.Count);
            int next = battle.running ? battle.wave : battle.wave + 1;
            if (!battle.endless) next = Mathf.Min(next, FrontierRules.TotalWaves(battle.difficulty));
            int[] counts = new int[3];
            for (int index = 0; index < FrontierRules.Count(next); index++)
            {
                counts[FrontierRules.Kind(next, index)]++;
            }

            string preview = battle.running ? "FIGHTING" : "NEXT WAVE " + next;
            string[] names =
            {
                "Bandit",
                "Roadrunner",
                "Ironhide"
            };
            for (int kind = 0; kind < 3; kind++)
            {
                if (counts[kind] > 0)
                {
                    preview += "\n" + counts[kind] + " " + names[kind] + " / " + FrontierRules.HP(next, battle.difficulty, kind) + " HP / " + FrontierRules.Speed(next, battle.difficulty, kind).ToString("0.0") + " speed";
                }
            }

            ui.SetText("WavePreview", preview);
            ui.SetText("Rewards", "$" + FrontierRules.Reward(next) + " / kill + $25 clear\nBANK INCOME +$" + battle.BankIncome + " / clear");
            ui.SetText("StartWave", battle.running ? "WAVE IN PROGRESS" : "START WAVE");
            ui.SetEnabled("StartWave", CanManageTowers && !battle.running && !TutorialNeedsExplanation);
            ui.SetText("Speed", speed == 1 ? "1x" : "2x");
            ui.SetText("AutoStart", "AUTO START: " + (auto ? "ON" : "OFF"));
            for (int type = 0; type < FrontierRules.Prices.Length; type++)
            {
                ui.SetEnabled("Tower" + type, CanManageTowers && SaloonEconomy.IsUnlocked(type) && battle.gold >= FrontierRules.Prices[type]);
                ui.Highlight("Tower" + type, build == type);
                ui.SetText("Tower" + type, FrontierRules.TowerNames[type] + (SaloonEconomy.IsUnlocked(type) ? "  $" + FrontierRules.Prices[type] : "  LOCKED"));
            }

            var tower = Selected();
            ui.Show("Details", tower != null);
            if (tower != null)
            {
                ui.SetText("TowerName", FrontierRules.TowerNames[tower.type]);
                ui.SetText("TowerStats", "UPGRADES " + tower.level + " / 3  / HEIGHT " + (battle.heights[tower.x, tower.z] + 1) + "\n\n" + TowerDetails(tower));
                ui.SetText("Upgrade", tower.level == 3 ? "MAXIMUM LEVEL" : "UPGRADE $" + FrontierRules.UpgradePrice(tower.type, tower.level));
                ui.SetEnabled("Upgrade", CanManageTowers && tower.level < 3 && battle.gold >= FrontierRules.UpgradePrice(tower.type, tower.level));
                ui.SetText("Sell", "SELL +$" + tower.sell);
                ui.SetEnabled("Sell", CanManageTowers);
            }

            ui.Show("NoticePanel", noticeTime > 0 || build >= 0);
            ui.SetText("Notice", build >= 0 ? (hoverX >= 0 ? battle.PlacementError(build, hoverX, hoverZ) : "Choose an open tile. Right click cancels.") : notice);
            ui.Show("Lesson", TutorialNeedsExplanation && CanManageTowers);
            ui.Show("SkipTraining", IsTutorial && CanManageTowers);
            if (IsTutorial)
            {
                ui.SetText("LessonTitle", LessonTitle);
                ui.SetText("LessonText", LessonText);
                ui.SetEnabled("Acknowledge", CanAcknowledgeLesson);
                ui.SetText("Acknowledge", CanAcknowledgeLesson ? "UNDERSTOOD - READY FOR THE WAVE" : "Place a combat tower to continue");
            }

            bool modal = paused || confirmExit || battle.won || battle.lost;
            ui.Show("BattleModal", modal);
            ui.SetText("BattleModalTitle", confirmExit ? "LEAVE THIS MATCH?" : battle.won ? "THE TOWN IS SAFE" : battle.lost ? "THE GANG WON" : "TAKE A BREATHER");
            ui.SetText("BattleModalText", confirmExit ? "This match will end. Your unlocks and entrance key stay saved." : battle.won ? (IsTutorial ? "Training complete! Easy is now unlocked." : battle.difficulty < 2 ? "Victory! The next opponent is unlocked." : "You earned the entrance key! Press E at the lock in the saloon.") : battle.lost ? "Try upgrades, covering the exits and investing in banks between waves." : "The battle is paused.");
            if (battle.won && !IsTutorial)
            {
                ui.SetText("BattleModalText", "Victory! +" + SaloonEconomy.VictoryReward(battle.difficulty) + " Gold Coins for the bartender shop. " + (battle.difficulty < 2 ? "The next opponent is unlocked." : "You earned the entrance key!"));
            }

            bool pauseOnly = paused && !confirmExit && !battle.won && !battle.lost;
            ui.Show("BattleResume", pauseOnly);
            ui.Show("BattleHome", pauseOnly);
            ui.Show("BattleMusic", pauseOnly);
            ui.Show("BattleEffects", pauseOnly);
            ui.Show("KeepPlaying", confirmExit);
            ui.Show("ReturnSaloon", confirmExit || battle.won || battle.lost);
            ui.Show("ContinueEndless", battle.won && !IsTutorial && !confirmExit);
        }

        string TowerDetails(FrontierRules.Tower t)
        {
            if (t.type == 8)
            {
                return "SUPPORT ONLY / " + SheriffOfficeTower.StartingRange.ToString("0.00") + " tile aura\n+" + Mathf.RoundToInt(SheriffOfficeTower.RangeBonus(t.level) * 100) + "% range / +" + Mathf.RoundToInt(SheriffOfficeTower.DamageBonus(t.level) * 100) + "% damage\nStrongest bonus only / no attacks";
            }
            if (t.type == 4)
            {
                return "+$" + t.income + " after every cleared wave\n" + (t.level < 3 ? "Next: +$" + (t.income + 20) + " / wave" : "Maximum income");
            }

            string range = (t.type == 1 ? "Infinite / 3-tile blind zone" : battle.TowerRange(t).ToString("0.00") + " tile range");
            if (t.type == 5)
            {
                return battle.TowerDamage(t).ToString("0.#") + " damage / shot\n" + t.currentRate.ToString("0.0") + " shots/sec / " + Mathf.RoundToInt(t.spin * 100) + "% spin\n4 sec to full speed; cools when idle\n" + range;
            }

            if (t.type == 6)
            {
                return "5 pellets x " + battle.TowerDamage(t).ToString("0.#") + " damage\n60-degree cone / " + t.rate.ToString("0.00") + " shots/sec\nPellet damage falls toward cone edges\n" + range;
            }

            if (t.type == 7)
            {
                return battle.TowerDamage(t).ToString("0.#") + " damage / " + t.rate.ToString("0.00") + " shots/sec\nOne Wanted enemy / all damage x1.5\nHunter kill: +$" + (FrontierRules.Reward(Mathf.Max(1, battle.wave)) * (2 + t.level)) + " bonus\n" + range;
            }

            return battle.TowerDamage(t).ToString("0.#") + " damage / " + t.rate.ToString("0.00") + " shots/sec\n" + range;
        }
    }
}


