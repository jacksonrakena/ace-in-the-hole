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

        public void PlayerRequestsJoin()
        {
            if (IsClient) JoinTableServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void JoinTableServerRpc(ServerRpcParams prams = default)
        {
            TableState.JoinTable(prams.Receive.SenderClientId);
        }
    }
}
