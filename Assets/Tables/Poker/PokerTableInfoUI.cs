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
            if (IsClient) JoinTableServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void JoinTableServerRpc(ServerRpcParams prams = default)
        {
            if (TableState.AllPlayersAtTable.Values.Any(e => e.OwnerClientId == prams.Receive.SenderClientId))
            {
                return;
            }
            
            // Create the state object for the player being at this specific table
            var playerStateObject = Instantiate(PlayerStatePrefab, 
                NetworkManager.Singleton.ConnectedClients[prams.Receive.SenderClientId].PlayerObject.transform);
            
            // Spawn the state object over the network and pass ownership to the player
            var playerStateNetworking = playerStateObject.GetComponent<NetworkObject>();
            playerStateNetworking.SpawnWithOwnership(prams.Receive.SenderClientId);
            
            // Parent the state object under the player
            playerStateNetworking.TrySetParent(NetworkManager.Singleton.ConnectedClients[prams.Receive.SenderClientId].PlayerObject.transform, worldPositionStays:false);
            
            // Access the player state script, and configure it on the server
            TableState.JoinTable(playerStateObject.GetComponent<PokerPlayerState>());
        }
    }
}
