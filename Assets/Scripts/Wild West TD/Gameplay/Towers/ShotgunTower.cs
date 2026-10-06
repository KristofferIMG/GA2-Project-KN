using UnityEngine;

namespace WildWestTD
{
    /// <summary>Short-range cone with strongest damage near its centre. The shared battle reads the settings below, so the model and simulation use the same values.</summary>
    [AddComponentMenu("Wild West TD/Towers/ShotgunTower")]
    public sealed class ShotgunTower : TowerView
    {
        // Starting balance. Change these named settings to tune this tower.
        // The purchase price is match cash, separate from the permanent shop unlock.
        public const int Cost = 150;
        public const float StartingDamage = 6;
        public const float StartingFireRate = .8f;
        public const float StartingRange = 2.35f;
        public const int PelletCount = 5;
        public const float HalfConeAngle = 30;
        public override int TowerType => 6;
        public override string Role => "Short-range cone with strongest damage near its centre.";
    }
}

