using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace WildWestTD
{
    /// <summary>
    /// Connects the saved Canvas to the game. Edit layout, colours and static text in the prefab.
    /// This component only changes visibility, live values and button availability.
    /// Button actions are wired in the Inspector, so they remain easy to follow.
    /// </summary>
    public class SaloonInterface : MonoBehaviour
    {
        [Serializable]
        public class Binding
        {
            public string key;
            public GameObject target;
            public Text text;
            public Button button;
        }

        [Header("Scene connections")]
        public FrontierGame game;
        public SaloonMenu menu;
        public SaloonWalkthrough player;
        public SaloonMaps maps;
        public SaloonShop shop;
        [Header("Saved Canvas elements - keys stay stable if objects are renamed")]
        public Binding[] bindings;
        public Text incomeLabelPrefab;
        public RectTransform incomeLayer;
        public GraphicRaycaster raycaster;
        readonly Dictionary<string, Binding> elements = new Dictionary<string, Binding>();
        readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        void Awake()
        {
            if (!game)
            {
                game = GetComponentInParent<FrontierGame>();
            }

            if (!menu)
            {
                menu = game.GetComponent<SaloonMenu>();
            }

            if (!player)
            {
                player = game.GetComponentInChildren<SaloonWalkthrough>();
            }

            if (!maps)
            {
                maps = game.GetComponent<SaloonMaps>();
            }

            if (!shop)
            {
                shop = game.GetComponentInChildren<SaloonShop>();
            }

            foreach (var binding in bindings)
            {
                elements.Add(binding.key, binding);
            }
        }

        public void Show(string key, bool visible)
        {
            var target = elements[key].target;
            if (target.activeSelf != visible)
            {
                target.SetActive(visible);
            }
        }

        public void SetText(string key, string value)
        {
            var label = elements[key].text;
            if (label.text != value)
            {
                label.text = value;
            }
        }

        public void SetEnabled(string key, bool enabled)
        {
            elements[key].button.interactable = enabled;
        }

        public void Highlight(string key, bool selected)
        {
            var button = elements[key].button;
            // Use the Inspector's selected colour, rather than overriding the authored palette.
            button.targetGraphic.color = selected ? button.colors.selectedColor : button.colors.normalColor;
        }

        public bool ContainsScreenPoint(Vector2 point)
        {
            if (!EventSystem.current)
            {
                return false;
            }

            raycastResults.Clear();
            raycaster.Raycast(new PointerEventData(EventSystem.current) { position = point }, raycastResults);
            return raycastResults.Count > 0;
        }

        public Text CreateIncomeLabel(int amount)
        {
            // Repeated feedback uses a saved prefab, just like enemies use a saved model.
            Text label = Instantiate(incomeLabelPrefab, incomeLayer);
            label.text = "+$" + amount;
            label.gameObject.SetActive(true);
            return label;
        }

        void LateUpdate()
        {
            Show("Home", menu.Visible && !menu.WipeConfirmationVisible);
            Show("Wipe", menu.Visible && menu.WipeConfirmationVisible);
            Show("Battle", game.Active);
            Show("Exploration", !menu.Visible && !game.Active && player.enabled && !shop.Visible);
            Show("BartenderShop", shop.Visible);
            SetText("HomeCoins", "GOLD COINS  " + SaloonEconomy.GoldCoins);
            SetText("ShopCoins", "YOUR GOLD COINS  " + SaloonEconomy.GoldCoins);
            SetText("ShopMessage", shop.Message);
            SetText("TestUnlock", "TEST: UNLOCK ALL TOWERS - " + (SaloonEconomy.TemporaryUnlockAll ? "ON" : "OFF"));
            for (int type = 0; type < FrontierRules.Prices.Length; type++)
            {
                bool owned = SaloonEconomy.IsPermanentlyUnlocked(type);
                SetText("Unlock" + type, owned ? "OWNED" : "UNLOCK - 50 COINS");
                SetEnabled("Unlock" + type, !owned && SaloonEconomy.GoldCoins >= 50);
            }

            Show("HomeSkip", !SaloonProgress.TutorialComplete);
            SetText("HomeHint", SaloonProgress.TutorialComplete ? "Explore the saloon and challenge an opponent." : "Your first Play begins ten-wave training. Complete it or skip to unlock Easy.");
            var audio = SaloonAudio.Instance;
            if (audio)
            {
                foreach (string key in new[]
                {
                    "HomeMusic",
                    "HubMusic",
                    "BattleMusic"
                }

                )
                {
                    SetText(key, "MUSIC: " + (audio.MusicOn ? "ON" : "OFF"));
                }

                foreach (string key in new[]
                {
                    "HomeEffects",
                    "HubEffects",
                    "BattleEffects"
                }

                )
                {
                    SetText(key, "SOUND FX: " + (audio.EffectsOn ? "ON" : "OFF"));
                }
            }

            if (game.Active)
            {
                game.RefreshHUD(this);
            }

            if (menu.Visible || game.Active || !player.enabled)
            {
                return;
            }

            Show("HubPause", player.Paused);
            Show("TableSelection", player.SeatedTable >= 0 && !player.Paused);
            Show("Walking", player.SeatedTable < 0 && !player.Paused);
            SetText("Interaction", player.InteractionPrompt);
            Show("InteractionPanel", player.InteractionPrompt.Length > 0);
            Show("ExploreHint", Cursor.lockState != CursorLockMode.Locked);
            int difficulty = player.SeatedTable;
            if (difficulty < 0)
            {
                return;
            }

            bool available = difficulty == 3 || (SaloonProgress.TutorialComplete && difficulty <= game.Unlocked);
            SetText("Opponent", player.OpponentName);
            SetText("Difficulty", new[] { "EASY", "MEDIUM", "HARD", "TRAINING" }[difficulty]);
            SetText("Availability", available ? FrontierRules.TotalWaves(difficulty == 3 ? -1 : difficulty) + " waves / Available" : !SaloonProgress.TutorialComplete ? "Locked / Complete or skip training" : difficulty == 1 ? "Locked / Win Easy first" : "Locked / Win Medium first");
            SetText("MapName", SaloonMaps.Names[maps.activeMap]);
            SetText("MapDescription", SaloonMaps.Descriptions[maps.activeMap] + "\n\n20 x 20 tiles / 3 elevations\n" + SaloonMaps.SpawnCount(maps.activeMap) + " enemy entrance(s)");
            SetEnabled("StartMatch", available);
        }

        // Named methods keep every Button's On Click list understandable in the Inspector.
        public void UnlockTower(int type)
        {
            shop.Buy(type);
        }

        public void ToggleTestUnlock()
        {
            SaloonEconomy.ToggleTemporaryUnlock();
            SaloonAudio.Play("click");
        }

        public void CloseShop()
        {
            shop.Close();
        }

        public void Play()
        {
            menu.Play();
        }

        public void Home()
        {
            player.HomeScreen();
        }

        public void SkipTutorial()
        {
            menu.SkipTutorial();
        }

        public void RequestWipe()
        {
            menu.RequestWipeProgress();
        }

        public void CancelWipe()
        {
            menu.CancelWipeProgress();
        }

        public void ConfirmWipe()
        {
            menu.ConfirmWipeProgress();
        }

        public void Music()
        {
            SaloonAudio.Instance.ToggleMusic();
        }

        public void Effects()
        {
            SaloonAudio.Instance.ToggleEffects();
        }

        public void ResumeHub()
        {
            player.Resume();
        }

        public void PreviousDifficulty()
        {
            player.ChangeDifficulty(-1);
        }

        public void NextDifficulty()
        {
            player.ChangeDifficulty(1);
        }

        public void PreviousMap()
        {
            maps.ChangeMap(-1);
        }

        public void NextMap()
        {
            maps.ChangeMap(1);
        }

        public void StandUp()
        {
            player.LeaveTable();
        }

        public void StartMatch()
        {
            game.BeginMatch(player.SeatedTable == 3 ? -1 : player.SeatedTable);
        }

        public void BuyTower(int type)
        {
            if (game.CanManageTowers)
            {
                game.ChooseTower(type);
            }
        }

        public void StartWave()
        {
            game.StartWaveFromButton();
        }

        public void Speed()
        {
            game.ToggleSpeed();
        }

        public void PauseBattle()
        {
            game.TogglePause();
        }

        public void AutoStart()
        {
            game.ToggleAutoStart();
        }

        public void LeaveMatch()
        {
            game.RequestLeaveMatch();
        }

        public void KeepPlaying()
        {
            game.CancelLeaveMatch();
        }

        public void ContinueEndless()
        {
            game.ContinueEndless();
        }

        public void ReturnSaloon()
        {
            game.EndMatch();
        }

        public void Upgrade()
        {
            game.UpgradeSelectedTower();
        }

        public void Sell()
        {
            game.SellSelectedTower();
        }

        public void CloseDetails()
        {
            game.CloseTowerDetails();
        }

        public void Acknowledge()
        {
            game.AcknowledgeLesson();
        }
    }
}


