using System.Linq;
using AceInTheHole.Tables.Poker.Client;
using AceInTheHole.Tables.Poker.Server;
using Unity.Netcode;
using UnityEngine;
namespace AceInTheHole.Tables.Poker
{
    public class PokerTableInfoUI : NetworkBehaviour
    {
        public PokerTableState TableState;
        public GameObject PlayerStatePrefab;

        public void PlayerRequestsJoin()
        {
            if (IsClient)
            {
                Debug.Log("Player requests to join table");
                JoinTableServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void JoinTableServerRpc(ServerRpcParams prams = default)
        {
            Debug.Log("Server acknowledging JoinTable request from " +prams.Receive.SenderClientId);

            if (TableState.AllPlayersAtTable.Values.Any(e => e.OwnerClientId == prams.Receive.SenderClientId))
            {
                return;
            }
            var playerStatePrefab = Instantiate(PlayerStatePrefab, 
                NetworkManager.Singleton.ConnectedClients[prams.Receive.SenderClientId].PlayerObject.transform);
            var no = playerStatePrefab.GetComponent<NetworkObject>();
            //no.SpawnWithObservers = false;
            no.SpawnWithOwnership(prams.Receive.SenderClientId);
            no.TrySetParent(NetworkManager.Singleton.ConnectedClients[prams.Receive.SenderClientId].PlayerObject.transform, worldPositionStays:false);
            //no.NetworkShow(prams.Receive.SenderClientId);
            TableState.JoinTable(playerStatePrefab.GetComponent<PokerPlayerState>());
        }
    }
}
