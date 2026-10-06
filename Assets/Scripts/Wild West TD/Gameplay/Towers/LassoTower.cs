using UnityEngine;

namespace WildWestTD
{
    /// <summary>Slows enemies so other defenders have more time to fire. The shared battle reads the settings below, so the model and simulation use the same values.</summary>
    [AddComponentMenu("Wild West TD/Towers/LassoTower")]
    public sealed class LassoTower : TowerView
    {
        // Starting balance. Change these named settings to tune this tower.
        // The purchase price is match cash, separate from the permanent shop unlock.
        public const int Cost = 65;
        public const float StartingDamage = 3;
        public const float StartingFireRate = .9f;
        public const float StartingRange = 3.75f;
        public const float SlowDuration = 2.2f;
        public const float SlowedSpeed = .55f;
        public override int TowerType => 3;
        public override string Role => "Slows enemies so other defenders have more time to fire.";
    }
}

