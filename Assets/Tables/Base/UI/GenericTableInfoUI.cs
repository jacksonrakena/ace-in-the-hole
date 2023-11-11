using AceInTheHole.Tables.Blackjack.Server;
using TMPro;
using Unity.Netcode;
using UnityEngine;
namespace AceInTheHole.Tables.Base.UI
{
    public class GenericTableInfoUI : NetworkBehaviour
    {
        public GameObject TableState;

        IPlayableTable PlayableTable => (IPlayableTable)TableState.GetComponent(typeof(IPlayableTable));
        
        public void Start()
        {
            var info = PlayableTable.TableInfo;

            var panel = transform.Find("Panel");
            var name = panel.Find("Name").GetComponent<TextMeshProUGUI>();
            name.text = info.Name;

            var desc = panel.Find("Description").GetComponent<TextMeshProUGUI>();
            desc.text = info.Description;

            var min = panel.Find("Player Counts/Minimum Players").GetComponent<TextMeshProUGUI>();
            min.text = $"Minimum players: {info.MinimumPlayers}";

            var max = panel.Find("Player Counts/Maximum Players").GetComponent<TextMeshProUGUI>();
            max.text = $"Maximum players: {info.MaximumPlayers}";
        }
        public void PlayerRequestsJoin()
        {
            if (IsClient) JoinTableServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void JoinTableServerRpc(ServerRpcParams prams = default)
        {
            PlayableTable.JoinClientToTable(prams.Receive.SenderClientId);
        }
    }
}
