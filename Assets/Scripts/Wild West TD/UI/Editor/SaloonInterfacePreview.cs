#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace WildWestTD
{
    /// <summary>Editor-only preview controls. No effect on a running game.</summary>
    [CustomEditor(typeof(SaloonInterface))]
    public class SaloonInterfacePreview : Editor
    {
        void Preview(string screen)
        {
            var ui = (SaloonInterface)target;
            Undo.RecordObjects(ui.GetComponentsInChildren<Transform>(true), "Preview UI screen");
            foreach (var entry in ui.bindings)
            {
                if (entry.key == "Home" || entry.key == "Battle" || entry.key == "Exploration" || entry.key == "BartenderShop" || entry.key == "Wipe")
                { Undo.RecordObject(entry.target, "Preview UI screen"); entry.target.SetActive(entry.key == screen); }
                if (screen == "Battle" && (entry.key == "BattleModal" || entry.key == "Lesson" || entry.key == "SkipTraining")) entry.target.SetActive(false);
                if (screen == "Exploration")
                {
                    if (entry.key == "TableSelection") entry.target.SetActive(true);
                    if (entry.key == "HubPause" || entry.key == "Walking") entry.target.SetActive(false);
                }
            }
            if(screen == "Battle")
            {
                foreach(var entry in ui.bindings) if(entry.text && string.IsNullOrWhiteSpace(entry.text.text))
                {
                    Undo.RecordObject(entry.text,"Preview sample text");
                    switch(entry.key) {
                        case "Resources": entry.text.text="TOWN 5 / 5      CASH $100";break;
                        case "MatchName": entry.text.text="EASY / Dusty Crossing";break;
                        case "WaveTitle": entry.text.text="WAVE 0 / 20    Alive: 0";break;
                        case "WavePreview": entry.text.text="NEXT WAVE 1\n6 Bandits / 14 HP";break;
                        case "TowerName": entry.text.text="Revolver Man";break;
                        case "TowerStats": entry.text.text="UPGRADES 0 / 3\n\n6 damage / 1.35 shots/sec\n4.25 tile range";break;
                    }
                }
            }
            EditorUtility.SetDirty(ui); SceneView.RepaintAll();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("All UI lives here. These buttons reveal saved panels in Edit Mode. Expand Battle for the in-game HUD, shop, details and tutorial. Play Mode automatically restores the correct screen.", MessageType.Info);
            using(new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if(GUILayout.Button("Preview In-Game UI")) Preview("Battle");
                if(GUILayout.Button("Preview Main Menu")) Preview("Home");
                if(GUILayout.Button("Preview Table Selection")) Preview("Exploration");
                if(GUILayout.Button("Preview Bartender Shop")) Preview("BartenderShop");
                if(GUILayout.Button("Preview Wipe Confirmation")) Preview("Wipe");
            }
            DrawDefaultInspector();
        }
    }
}
#endif

