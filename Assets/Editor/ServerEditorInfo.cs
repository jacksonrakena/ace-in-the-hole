using System.Collections.Generic;
using System.Linq;
using AceInTheHole.Tables.Poker.Server;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PokerTableState))]
    public class ServerEditorInfo : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var go = serializedObject.targetObject.GetComponent<PokerTableState>();
            var leader = go.CurrentLeaders();
            
            GUILayout.Label($"Current leading players:\n" + 
                            string.Join("\n", leader.Select(a => $"{a.Key} with set {a.Value.ToString()}")));
            GUILayout.Label($"Current player: " + (go.currentPlayerSeatId.Value != -1 ? go.CurrentPlayer : "None"));
            GUILayout.Label($"Stage: {go.stage.Value}");
            GUILayout.Label($"Minimum bet: {go.currentRequiredBet.Value}");
            GUILayout.Label($"Pot size: {go.pot.Value}");
            GUILayout.Label("Bet states:\n"+string.Join("\n", go.AllPlayersAtTable.Select(d => $"{d.tablePosition.Value}: {d.betState?.ToString() ?? "Not in"}")));
            DrawDefaultInspector();
        }
    }
}