using System.Collections.Generic;
using System.Linq;
using AceInTheHole.Tables.Poker.Client;
using JetBrains.Annotations;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
namespace AceInTheHole.Tables.Base
{
    public abstract class OrdinalTableBase<TPlayerState, TTableBase> : NetworkBehaviour, IPlayableTable
        where TPlayerState : OrdinalTablePlayerStateBase<TPlayerState, TTableBase>
        where TTableBase : OrdinalTableBase<TPlayerState, TTableBase>
    {
        [Tooltip("The GameObject on the table model containing all of the seat objects as children.")]
        public GameObject SeatContainer;
        
        /*
         * The host (dealer) of the table.
         * At the moment, this variable only changes when the table host disconnects.
         */
        public NetworkVariable<int> tableHost = new NetworkVariable<int>(-1);

        /*
         * The number of players at this table.
         */
        public NetworkVariable<int> playerCount = new NetworkVariable<int>(0);
        
        /*
         * The seat position of the current player. (-1 if no player is playing)
         */
        public NetworkVariable<int> currentPlayerSeatId = new NetworkVariable<int>(-1);
        
        protected readonly Dictionary<int, TPlayerState> _playersBySeatPosition = new Dictionary<int, TPlayerState>();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _playersBySeatPosition.Clear();
                var i = 0;
                foreach (Transform child in SeatContainer.transform)
                {
                    _playersBySeatPosition[i] = null;
                    i++;
                }
            }
        }
        
        [CanBeNull] public TPlayerState CurrentPlayer => currentPlayerSeatId.Value == -1 ? null : _playersBySeatPosition[currentPlayerSeatId.Value];
        public IEnumerable<TPlayerState> AllPlayersAtTable => _playersBySeatPosition.Values.NotNull();

        TPlayerState NextOccupiedSeatAfter(TPlayerState player = null)
        {
            var nextOrdinalPosition = _playersBySeatPosition
                .Where(e => e.Value != null)
                .OrderBy(e => e.Key)
                .Cast<KeyValuePair<int, TPlayerState>?>()
                .FirstOrDefault(e => e != null && (player == null || e.Value.Key > player.tablePosition.Value));

            if (nextOrdinalPosition != null) return nextOrdinalPosition.Value.Value;
            
            return _playersBySeatPosition
                .OrderBy(e => e.Key)
                .Where(e => e.Value != null)
                .FirstOrDefault(e => player == null || e.Key != player.tablePosition.Value).Value;
        }

        public GameObject PlayerStatePrefab;
        
        [CanBeNull]
        public void JoinClientToTable(ulong clientId)
        {
            if (AllPlayersAtTable.Any(e => e.OwnerClientId == clientId))
            {
                return;
            }
            
            // Create the state object for the player being at this specific table
            var playerStateObject = Instantiate(PlayerStatePrefab, 
                NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform);
            
            // Spawn the state object over the network and pass ownership to the player
            var playerStateNetworking = playerStateObject.GetComponent<NetworkObject>();
            playerStateNetworking.SpawnWithOwnership(clientId);
            
            // Parent the state object under the player
            playerStateNetworking.TrySetParent(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform, worldPositionStays:false);

            var pokerPlayer = playerStateObject.GetComponent<TPlayerState>();
            
            if (_playersBySeatPosition.Any(e => e.Value == null))
            {
                var position = _playersBySeatPosition.First(e => e.Value == null).Key;
                _playersBySeatPosition[position] = pokerPlayer;
                pokerPlayer.tablePosition.Value = position;
                Log($"{pokerPlayer} joined table, assigned seat position {position}");
                if (tableHost.Value == -1)
                {
                    AssignHost(pokerPlayer, position);
                }
                var targetSeat = SeatContainer.transform.Find("Seat" + position);
                if (!pokerPlayer.transform.parent.GetComponent<NetworkObject>().TrySetParent(targetSeat.transform))
                {
                    Debug.Log($"Failed to move {pokerPlayer} to seat {targetSeat}");
                }
                playerCount.Value++;
                OnClientJoinTable(pokerPlayer);
                pokerPlayer.Server_Initialize((TTableBase) this);
                return;
            }
            Log($"Player {pokerPlayer} can't join the table as there are no position available.");
        }
        public abstract PlayableTableInfo TableInfo { get; }

        public abstract void OnClientJoinTable(TPlayerState player);

        public abstract void OnClientLeaveTable(TPlayerState player);

        public void RemoveClientFromTable(TPlayerState player)
        {
            Log($"{player} left");
            
            playerCount.Value--;
            _playersBySeatPosition[player.tablePosition.Value] = null;
            
            if (tableHost.Value == player.tablePosition.Value)
            {
                var otherPlayers = _playersBySeatPosition
                    .Where(d => d.Value != null).ToList();
                if (otherPlayers.Any())
                {
                    AssignHost(otherPlayers.First().Value, otherPlayers.First().Key);
                }
            }

            OnClientLeaveTable(player);
        }
        
        public void Log(string message)
        {
            Debug.Log($"{gameObject.name}: {message}");
        }
        
        public void AssignHost(TPlayerState pokerPlayer, int position)
        {
            tableHost.Value = position;
            Log($"{pokerPlayer} assigned as table host");
        }
        
        
        // TODO: REMOVE WHEN GAMEOBJECTS 1.7.1 RELEASES
        [ServerRpc(RequireOwnership = false)]
        public void RequestGameStartServerRpc(ServerRpcParams prams = default)
        {
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void ConfirmPlayInOptionServerRpc()
        {
        }

        [ServerRpc(RequireOwnership = false)]
        public void ConfirmHandOptionServerRpc()
        {
        }
    }
}