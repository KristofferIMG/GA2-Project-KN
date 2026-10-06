using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace WildWestTD
{
    /// <summary>Attached to the bartender. E opens his saved Canvas shop; Escape closes it.</summary>
    public class SaloonShop : MonoBehaviour
    {
        public SaloonWalkthrough player;
        public FrontierGame game;
        public Transform interactionPoint;
        public bool Visible { get; private set; }
        public string Message { get; private set; } = "Win matches to earn Gold Coins. Each new tower costs 50.";
        void Awake()
        {
            if (!game) game = GetComponentInParent<FrontierGame>();
            if (!player && game) player = game.GetComponentInChildren<SaloonWalkthrough>();
        }

        public bool IsLookedAt(Transform camera)
        {
            if (!interactionPoint || Visible)
            {
                return false;
            }

            Vector3 offset = interactionPoint.position - camera.position;
            if (offset.magnitude > 4.2f || Vector3.Angle(camera.forward, offset) > 14)
            {
                return false;
            }

            // The counter is intentionally between the player and the bartender.
            // Aim at his upper body, with a narrow cone, rather than requiring a ray through the counter.
            return true;
        }

        public void Open()
        {
            if (!game || !player || game.Active || !player.enabled || player.Paused)
            {
                return;
            }

            if (player.SeatedTable >= 0)
            {
                player.LeaveTable();
            }

            Visible = true;
            player.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Message = "Win matches to earn Gold Coins. Each new tower costs 50.";
            SaloonAudio.Play("click");
        }

        public void Close()
        {
            if (!Visible)
            {
                return;
            }

            Visible = false;
            player.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Buy(int type)
        {
            if (!Visible)
            {
                return;
            }

            if (SaloonEconomy.UnlockTower(type))
            {
                Message = FrontierRules.TowerNames[type] + " unlocked permanently! Buy it with cash during a match.";
                SaloonAudio.Play("upgrade");
            }
            else
            {
                Message = SaloonEconomy.IsUnlocked(type) ? "You already own that tower." : "You need 50 Gold Coins to unlock this tower.";
            }
        }

        void Update()
        {
            if (!Visible)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            bool escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            bool escape = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (escape)
            {
                Close();
            }
        }
    }
}

