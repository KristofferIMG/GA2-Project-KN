using UnityEngine;

namespace WildWestTD
{
    /// <summary>Long-range high-ground defense with a close blind spot. The shared battle reads the settings below, so the model and simulation use the same values.</summary>
    [AddComponentMenu("Wild West TD/Towers/SharpshooterTower")]
    public sealed class SharpshooterTower : TowerView
    {
        // Starting balance. Change these named settings to tune this tower.
        // The purchase price is match cash, separate from the permanent shop unlock.
        public const int Cost = 75;
        public const float StartingDamage = 21;
        public const float StartingFireRate = .43f;
        public const float StartingRange = 99;
        public override int TowerType => 1;
        public override string Role => "Long-range high-ground defense with a close blind spot.";
    }
}

