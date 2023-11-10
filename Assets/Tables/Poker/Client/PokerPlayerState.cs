using AceInTheHole.Engine;
using AceInTheHole.Player;
using AceInTheHole.Tables.Poker.Client.UI;
using AceInTheHole.Tables.Poker.Server;
using AceInTheHole.Tables.Poker.Server.Betting;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
namespace AceInTheHole.Tables.Poker.Client
{
    public class PokerPlayerState : NetworkBehaviour
    {
        public NetworkVariable<bool> isLittleBlind = new NetworkVariable<bool>();
        public NetworkVariable<bool> isBigBlind = new NetworkVariable<bool>();
        
        public NetworkVariable<int> balance = new NetworkVariable<int>(1000);
        public NetworkList<Card> Cards;
        public NetworkVariable<int> tablePosition = new NetworkVariable<int>(-1);
        public NetworkVariable<bool> ViewingCards = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<PlayerBetState?> betState 
            = new NetworkVariable<PlayerBetState?>
                (null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        public GameObject playerUiPrefab;
        GameObject playerUiInstance;

        PokerTableState _pokerTableState;

        public TextMeshPro bettingAmount;
        public TextMeshPro roleInfo;
        public GameObject currentPlayerIndicator;

        public bool IsTableHost => _pokerTableState.tableHost.Value == this.tablePosition.Value;
        
        public void Awake()
        {
            Cards = new NetworkList<Card>(null, NetworkVariableReadPermission.Owner);
        }

        public override void OnDestroy()
        {
            Cards.Dispose();
        }

        public void Revalidate()
        {
            if (_pokerTableState.stage.Value == RoundStage.Setup)
            {
                roleInfo.text = "";
                bettingAmount.text = "";
            }
            else
            {

                if (_pokerTableState.WinningPlayersBySeatId.Contains(this.tablePosition.Value))
                {
                    roleInfo.text = _pokerTableState.WinningPlayersBySeatId.Count == 1 ? "Winner" : "Winner (Split pot)";
                } else if (this.isBigBlind.Value)
                {
                    roleInfo.text = "Big blind";
                } else if (isLittleBlind.Value)
                {
                    roleInfo.text = "Little blind";
                }
                else roleInfo.text = "";

                if (betState.Value.HasValue)
                {
                    if (!betState.Value.Value.InRound)
                    {
                        bettingAmount.text = "Folded";
                    }
                    else bettingAmount.text = "$" + betState.Value.Value.Amount;
                }
                else bettingAmount.text = "";
            }
            
            //currentPlayerIndicator.SetActive(_pokerTableState.currentPlayerSeatId.Value == tablePosition.Value);
        }
        
        public void Client_LeaveTable()
        {
            Destroy(playerUiInstance);
            RequestLeaveTableServerRpc();
            var player = gameObject.transform.parent;
            player.GetComponent<ThirdPersonController>().movementEnabled = true;
        }

        [ServerRpc]
        public void RequestLeaveTableServerRpc()
        {
            _pokerTableState.LeaveTable(this);
            transform.parent.GetComponent<NetworkObject>().TrySetParent((GameObject) null);
            NetworkObject.Despawn();
        }

        public void RevalidateAnimationState(RoundStage old, RoundStage current)
        {
            if (_pokerTableState.stage.Value == RoundStage.End)
            {
                var animator = GetComponentInParent<NetworkAnimator>();
                if (_pokerTableState.WinningPlayersBySeatId.Contains(this.tablePosition.Value))
                {
                    animator.SetTrigger("Won round trigger");
                } else if (_pokerTableState.WinningPlayersBySeatId.Count > 0)
                {
                    animator.SetTrigger("Lost round trigger");
                }
            }
        }

        public void OnPotStateChanged(PlayerBetState? old, PlayerBetState? current) => Revalidate();
        public void OnCurrentPlayerChanged(int oldSeat, int newSeat) => Revalidate();
        public void OnWinningPlayersChanged(NetworkListEvent<int> changeData) => Revalidate();
        public void OnBlindsChange(bool oldValue, bool newValue) => Revalidate();

        public void ServerConnectToTable(PokerTableState pts)
        {
            _pokerTableState = pts;
            ClientConnectToTableClientRpc();
            _pokerTableState.stage.OnValueChanged += RevalidateAnimationState;
            name = ToString();
        }
        
        [ClientRpc]
        public void ClientConnectToTableClientRpc()
        {
            _pokerTableState = gameObject.GetComponentInParent<PokerTableState>();
            
            betState.OnValueChanged += OnPotStateChanged;
            _pokerTableState.currentPlayerSeatId.OnValueChanged += OnCurrentPlayerChanged;
            _pokerTableState.WinningPlayersBySeatId.OnListChanged += OnWinningPlayersChanged;
            isBigBlind.OnValueChanged += OnBlindsChange;
            isLittleBlind.OnValueChanged += OnBlindsChange;
            
            var player = gameObject.transform.parent;
            
            if (IsOwner)
            {
                playerUiInstance = Instantiate(playerUiPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                playerUiInstance.GetComponent<PlayerUI>().Configure(this, _pokerTableState);
                
                player.GetComponent<ThirdPersonController>().movementEnabled = false;
                player.localPosition = new Vector3(0, 0, 0);
            }
            

            roleInfo = player.transform.Find("Role").GetComponent<TextMeshPro>();
            bettingAmount = player.transform.Find("BettingAmount").GetComponent<TextMeshPro>();
            name = ToString();
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                betState.OnValueChanged -= OnPotStateChanged;
                _pokerTableState.currentPlayerSeatId.OnValueChanged -= OnCurrentPlayerChanged;
                _pokerTableState.WinningPlayersBySeatId.OnListChanged -= OnWinningPlayersChanged;
                isBigBlind.OnValueChanged -= OnBlindsChange;
                isLittleBlind.OnValueChanged -= OnBlindsChange;
            }
            if (IsServer)
            {
                _pokerTableState.stage.OnValueChanged -= RevalidateAnimationState;
            }
        }

        public override string ToString()
        {
            if (_pokerTableState == null) return $"Client {OwnerClientId}";
            return $"State for {OwnerClientId} (seat {tablePosition.Value}), sitting at {_pokerTableState.gameObject.name}";
        }

        public int CurrentBetAmount => betState.Value?.Amount ?? 0;
        
        public int RequiredIncreaseToCheck => _pokerTableState.currentRequiredBet.Value - CurrentBetAmount;

        public bool HasFolded => !betState.Value?.InRound ?? false;
    }
}