using UnityEngine;

namespace WildWestTD
{
    /// <summary>One shared Wanted mark increases all incoming damage by 50%. The shared battle reads the settings below, so the model and simulation use the same values.</summary>
    [AddComponentMenu("Wild West TD/Towers/BountyHunterTower")]
    public sealed class BountyHunterTower : TowerView
    {
        // Starting balance. Change these named settings to tune this tower.
        // The purchase price is match cash, separate from the permanent shop unlock.
        public const int Cost = 300;
        public const float StartingDamage = 15;
        public const float StartingFireRate = .85f;
        public const float StartingRange = 4;
        public const float WantedDamageMultiplier = 1.5f;
        public const int StartingBonusRewards = 2;
        public override int TowerType => 7;
        public override string Role => "One shared Wanted mark increases all incoming damage by 50%.";
    }
}

