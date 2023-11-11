using AceInTheHole.Player;
using Unity.Netcode;
using UnityEngine;
namespace AceInTheHole.Tables.Base
{
    public abstract class OrdinalTablePlayerStateBase<TPlayerState, TTableBase> : NetworkBehaviour, IPlayableTablePlayerState
        where TPlayerState : OrdinalTablePlayerStateBase<TPlayerState, TTableBase>
        where TTableBase : OrdinalTableBase<TPlayerState, TTableBase>
    {
        public NetworkVariable<int> tablePosition = new NetworkVariable<int>(-1);

        public IPlayableTable PlayableTable => TableState;

        protected TTableBase TableState;
        public bool IsTableHost => TableState.tableHost.Value == this.tablePosition.Value;
        
        public void Server_Initialize(TTableBase pts)
        {
            TableState = pts;
            Client_InitializeClientRpc();
            name = ToString();
            
            OnJoinTable();
        }
        
        [ClientRpc]
        public void Client_InitializeClientRpc()
        {
            TableState = gameObject.GetComponentInParent<TTableBase>();
            name = ToString();

            if (IsOwner)
            {
                var player = gameObject.transform.parent;
                player.GetComponent<ThirdPersonController>().movementEnabled = false;
                player.localPosition = new Vector3(0, 0, 0);   
            }

            OnJoinTable();
        }
        public abstract void OnJoinTable();
        public abstract void OnLeaveTable();

        public void Client_RequestLeaveTable()
        {
            Server_LeaveTableServerRpc();
            if (IsOwner)
            {
                var player = gameObject.transform.parent;
                player.GetComponent<ThirdPersonController>().movementEnabled = true;
            }
            OnLeaveTable();
        }

        [ServerRpc]
        private void Server_LeaveTableServerRpc()
        {
            TableState.RemoveClientFromTable((TPlayerState) this);
            transform.parent.GetComponent<NetworkObject>().TrySetParent((GameObject) null);
            NetworkObject.Despawn();
            OnLeaveTable();
        }

        public override string ToString()
        {
            if (TableState == null) return $"Client {OwnerClientId}";
            return $"Client {OwnerClientId} at seat {tablePosition.Value} on {TableState.name}";
        }
    }
}