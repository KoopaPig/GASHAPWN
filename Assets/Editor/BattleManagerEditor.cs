using GASHAPWN;
using UnityEditor;
using UnityEngine;

namespace GASHAPWN
{
    [CustomEditor(typeof(BattleManager))]
    public class BattleManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            BattleManager battleManager = (BattleManager)target;

            if (GUILayout.Button("Simulate Player 2 Win"))
            {
                battleManager.SimulatePlayer2Win();
            }

            GUILayout.Space(10);

            // Draw the default inspector afterwards
            DrawDefaultInspector();
        }
    }
}
