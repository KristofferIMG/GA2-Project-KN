using UnityEngine;

namespace WildWestTD
{
    /// <summary>
    /// The component on each saved tower mesh. The battle owns damage and money;
    /// this component owns the matching model, its selection data and animation.
    /// Keeping these separate means changing a model cannot change the balance.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class TowerView : MonoBehaviour
    {
        public abstract int TowerType { get; }
        public abstract string Role { get; }
        public int PurchasePrice => FrontierRules.Prices[TowerType];
        public FrontierRules.Tower Tower { get; private set; }

        public void Bind(FrontierRules.Tower tower)
        {
            if (tower.type != TowerType)
            {
                throw new System.ArgumentException("Tower model does not match its battle record.");
            }

            Tower = tower;
        }

        public virtual void Animate(float deltaTime)
        {
        }

        public virtual void AimAt(Vector3 target)
        {
            Vector3 direction = target - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > .00001f)
            {
                transform.rotation = Quaternion.LookRotation(-direction);
            }
        }
    }
}

