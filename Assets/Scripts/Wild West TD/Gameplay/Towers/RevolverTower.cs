using UnityEngine;

namespace WildWestTD
{
    /// <summary>Fast single-target defense. The shared battle reads the settings below, so the model and simulation use the same values.</summary>
    [AddComponentMenu("Wild West TD/Towers/RevolverTower")]
    public sealed class RevolverTower : TowerView
    {
        // Starting balance. Change these named settings to tune this tower.
        // The purchase price is match cash, separate from the permanent shop unlock.
        public const int Cost = 50;
        public const float StartingDamage = 6;
        public const float StartingFireRate = 1.35f;
        public const float StartingRange = 4.25f;
        public override int TowerType => 0;
        public override string Role => "Fast single-target defense.";
    }
}

