using UnityEngine;

namespace WildWestTD
{
    /// <summary>Area damage for groups near bends and intersections. The shared battle reads the settings below, so the model and simulation use the same values.</summary>
    [AddComponentMenu("Wild West TD/Towers/DynamiteTower")]
    public sealed class DynamiteTower : TowerView
    {
        // Starting balance. Change these named settings to tune this tower.
        // The purchase price is match cash, separate from the permanent shop unlock.
        public const int Cost = 95;
        public const float StartingDamage = 16;
        public const float StartingFireRate = .55f;
        public const float StartingRange = 3.6f;
        public const float SplashRadius = 1.35f;
        public override int TowerType => 2;
        public override string Role => "Area damage for groups near bends and intersections.";
    }
}

