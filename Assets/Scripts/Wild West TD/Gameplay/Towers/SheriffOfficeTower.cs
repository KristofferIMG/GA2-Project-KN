using UnityEngine;

namespace WildWestTD
{
    /// <summary>A support building. It never attacks; nearby towers receive the strongest office bonus.</summary>
    [AddComponentMenu("Wild West TD/Towers/Sheriff Office")]
    public sealed class SheriffOfficeTower : TowerView
    {
        public const int Cost = 250;
        public const float StartingDamage = 0;
        public const float StartingFireRate = 0;
        public const float StartingRange = ShotgunTower.StartingRange;
        public static int UpgradeCost(int level) => 100 + 50 * level;
        public static float RangeBonus(int level) => .10f + .05f * level;
        public static float DamageBonus(int level) => .05f * level;
        public override int TowerType => 8;
        public override string Role => "Buffs nearby towers. Strongest office bonus applies; no attacks.";
        public override void AimAt(Vector3 target) { }
    }
}

