using AceInTheHole.Tables.Blackjack.Server;
using TMPro;
using Unity.Netcode;
using UnityEngine;
namespace AceInTheHole.Tables.Base.UI
{
    public class GenericTableInfoUI : NetworkBehaviour
    {
        public GameObject TableState;
        public GameObject rootPanel;

        IPlayableTable PlayableTable => (IPlayableTable)TableState.GetComponent(typeof(IPlayableTable));

        public void FixedUpdate()
        {
            if (IsClient && IsSpawned)
            {
                if (NetworkManager.Singleton.gameObject && NetworkManager.Singleton.LocalClient.PlayerObject != null)
                {
                    // TODO: Move this somewhere else because it's expensive as fuck.
                    if (rootPanel.activeSelf)
                    {
                        var c = NetworkManager.Singleton.LocalClient.PlayerObject.transform.GetComponentInChildren<IPlayableTablePlayerState>();
                        if (c != null && c.PlayableTable == PlayableTable)
                        {
                            rootPanel.SetActive(false);
                        }
                    }
                    else
                    {
                        var c = NetworkManager.Singleton.LocalClient.PlayerObject.transform.GetComponentInChildren<IPlayableTablePlayerState>();
                        if (c == null || c.PlayableTable != PlayableTable) rootPanel.SetActive(true);
                    }
                }
            }
        }
        public void Start()
        {
            var info = PlayableTable.TableInfo;

            rootPanel = transform.Find("Panel").gameObject;
            var name = rootPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            name.text = info.Name;

            var desc = rootPanel.transform.Find("Description").GetComponent<TextMeshProUGUI>();
            desc.text = info.Description;

            var min = rootPanel.transform.Find("Player Counts/Minimum Players").GetComponent<TextMeshProUGUI>();
            min.text = $"Minimum players: {info.MinimumPlayers}";

            var max = rootPanel.transform.Find("Player Counts/Maximum Players").GetComponent<TextMeshProUGUI>();
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
