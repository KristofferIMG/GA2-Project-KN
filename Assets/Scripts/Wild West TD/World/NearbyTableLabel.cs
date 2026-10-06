using UnityEngine;

namespace WildWestTD
{
    /// <summary>A depth-tested label appears above a nearby table while exploring.</summary>
    public class NearbyTableLabel : MonoBehaviour
    {
        public SaloonWalkthrough player;
        public FrontierGame game;
        public Transform table;
        public TextMesh label;
        public int difficulty;
        public float revealDistance = 3.8f;

        void LateUpdate()
        {
            Vector3 offset = table.position - player.transform.position;
            offset.y = 0;
            bool nearby = player.enabled && !player.Paused && player.SeatedTable < 0 && !game.Active
                && offset.magnitude < revealDistance && Vector3.Dot(player.transform.forward, offset.normalized) > .05f;
            label.GetComponent<Renderer>().enabled = nearby;
            if (!nearby) return;
            string[] names = { "DUSTY JOE - EASY", "ROSE BLACKTHORN - MEDIUM", "THE UNDERTAKER - HARD", "MARSHAL FINCH - TRAINING" };
            label.text = names[difficulty] + "\n" + (difficulty == 3 ? "10 waves - Learn the ropes" : FrontierRules.TotalWaves(difficulty) + " waves - " + SaloonEconomy.VictoryReward(difficulty) + " Gold Coins");
            transform.rotation = Quaternion.LookRotation(transform.position - player.transform.position, Vector3.up);
        }
    }
}

