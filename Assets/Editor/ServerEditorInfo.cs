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
            GUILayout.Label($"Current player: " + (go.currentPlayerSeatId.Value != -1 ? go.CurrentPokerPlayer : "None"));
            GUILayout.Label($"Stage: {go.stage.Value}");
            GUILayout.Label($"Minimum bet: {go.potState.Value.CurrentRequiredBet}");
            GUILayout.Label($"Pot size: {go.potState.Value.Pot}");
            GUILayout.Label("Bet states:\n"+string.Join("\n", go.potState.Value.PlayerBetStates?
                .Select(d => $"{d.Key}: {(d.Value.InRound ? "In":"Folded")} #{(d.Value.Amount)}") ?? new List<string>()));
            DrawDefaultInspector();
        }
    }
}