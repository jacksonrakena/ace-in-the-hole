using AceInTheHole.Tables.Blackjack.Server;
using Unity.Netcode;
using UnityEngine;
namespace AceInTheHole.Tables.Base.UI
{
    public class GenericTableInfoUI : NetworkBehaviour
    {
        public GameObject TableState;

        public void Awake()
        {
        }
        public void PlayerRequestsJoin()
        {
            if (IsClient) JoinTableServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void JoinTableServerRpc(ServerRpcParams prams = default)
        {
            ((IPlayableTable) TableState.GetComponent(typeof(IPlayableTable))).JoinClientToTable(prams.Receive.SenderClientId);
        }
    }
}
