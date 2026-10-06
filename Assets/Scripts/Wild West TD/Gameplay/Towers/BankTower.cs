using UnityEngine;

namespace WildWestTD
{
    /// <summary>Earns gold once after each successful wave. Does not attack. The shared battle reads the settings below, so the model and simulation use the same values.</summary>
    [AddComponentMenu("Wild West TD/Towers/BankTower")]
    public sealed class BankTower : TowerView
    {
        // Starting balance. Change these named settings to tune this tower.
        // The purchase price is match cash, separate from the permanent shop unlock.
        public const int Cost = 400;
        public const float StartingDamage = 0;
        public const float StartingFireRate = 0;
        public const float StartingRange = 0;
        public const int StartingIncome = 40;
        public const int IncomePerUpgrade = 20;
        public const int FirstUpgradePrice = 80;
        public const int UpgradePriceStep = 20;
        public override int TowerType => 4;
        public override string Role => "Earns gold once after each successful wave. Does not attack.";
        public int IncomePerWave => Tower == null ? 40 : Tower.income;

        public override void AimAt(Vector3 target)
        { /* Buildings keep their original orientation. */
        }
    }
}

