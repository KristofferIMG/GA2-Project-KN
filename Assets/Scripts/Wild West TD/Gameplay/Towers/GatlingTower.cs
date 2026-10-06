using UnityEngine;

namespace WildWestTD
{
    /// <summary>Spins up during sustained attacks; cools down while idle. The shared battle reads the settings below, so the model and simulation use the same values.</summary>
    [AddComponentMenu("Wild West TD/Towers/GatlingTower")]
    public sealed class GatlingTower : TowerView
    {
        // Starting balance. Change these named settings to tune this tower.
        // The purchase price is match cash, separate from the permanent shop unlock.
        public const int Cost = 750;
        public const float StartingDamage = 10;
        public const float StartingFireRate = 1.25f;
        public const float StartingRange = 3.9f;
        public const float MaximumFireRate = 9;
        public const float SpinUpSeconds = 4;
        public const float CoolDownSeconds = 2;
        public override int TowerType => 5;
        public override string Role => "Spins up during sustained attacks; cools down while idle.";

        [Header("Moving mesh")]
        public Transform barrelRotor;
        public override void Animate(float deltaTime)
        {
            if (barrelRotor && Tower != null)
            {
                barrelRotor.Rotate(0, 0, Tower.spin * 900 * deltaTime, Space.Self);
            }
        }
    }
}

