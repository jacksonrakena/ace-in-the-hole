using AceInTheHole.Engine;
using AceInTheHole.Player;
using AceInTheHole.Tables.Poker.Client.UI;
using AceInTheHole.Tables.Poker.Server;
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
        
        public GameObject playerUiPrefab;

        PokerTableState _pokerTableState;

        public TextMeshPro bettingAmount;
        public TextMeshPro roleInfo;
        public GameObject currentPlayerIndicator;

        public bool IsTableHost => _pokerTableState._tableHost.Value == this.tablePosition.Value;
        
        public void Awake()
        {
            Cards = new NetworkList<Card>(null, NetworkVariableReadPermission.Owner);
        }
        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                _pokerTableState.LeaveTable(this);
            }   
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

                if (_pokerTableState.winningPlayersBySeatId.Contains(this.tablePosition.Value))
                {
                    roleInfo.text = _pokerTableState.winningPlayersBySeatId.Count == 1 ? "Winner" : "Winner (Split pot)";
                } else if (this.isBigBlind.Value)
                {
                    roleInfo.text = "Big blind";
                } else if (isLittleBlind.Value)
                {
                    roleInfo.text = "Little blind";
                }
                else roleInfo.text = "";   
                
                if (_pokerTableState.potState.Value.HasFolded(this))
                {
                    bettingAmount.text = "Folded";
                }
                else bettingAmount.text = "$" + _pokerTableState.potState.Value.GetCurrentBetAmountFor(this);
            }
            
            //currentPlayerIndicator.SetActive(_pokerTableState.currentPlayerSeatId.Value == tablePosition.Value);
        }
        
        [ClientRpc]
        public void JoinTableClientRpc()
        {
            var player = gameObject.transform.parent;
            player.GetComponent<ThirdPersonController>().movementEnabled = false;
            player.localPosition = new Vector3(0, 0, 0);

            roleInfo = player.transform.Find("Role").GetComponent<TextMeshPro>();
            bettingAmount = player.transform.Find("BettingAmount").GetComponent<TextMeshPro>();
            name = ToString();
            //currentPlayerIndicator = player.transform.Find("Current Player Indicator").gameObject;
        }

        [ClientRpc]
        public void LeaveTableClientRpc()
        {
            var player = gameObject.transform.parent;
            player.GetComponent<ThirdPersonController>().movementEnabled = true;
        }
    
        public override void OnNetworkSpawn()
        {
            name = ToString();
            _pokerTableState = GameObject.Find("Poker Table").GetComponent<PokerTableState>();

            if (IsServer)
            {
                _pokerTableState.stage.OnValueChanged += (_, _) =>
                {
                    if (_pokerTableState.stage.Value == RoundStage.End)
                    {
                        var animator = GetComponentInChildren<NetworkAnimator>();
                        if (_pokerTableState.winningPlayersBySeatId.Contains(this.tablePosition.Value))
                        {
                            animator.SetTrigger("Won round trigger");
                        } else if (_pokerTableState.winningPlayersBySeatId.Count > 0)
                        {
                            animator.SetTrigger("Lost round trigger");
                        }
                    }
                    // Revalidate();
                };
            }
            if (IsClient)
            {
                _pokerTableState.potState.OnValueChanged += (_, _) => Revalidate();
                _pokerTableState.currentPlayerSeatId.OnValueChanged += (_, _) => Revalidate();
                _pokerTableState.winningPlayersBySeatId.OnListChanged += (_) => Revalidate();
                isBigBlind.OnValueChanged += (_, _) => Revalidate();
                isLittleBlind.OnValueChanged += (_, _) => Revalidate();
                
                // Revalidate();
            }
        
            if (IsOwner)
            {
                var uiObject = Instantiate(playerUiPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                uiObject.GetComponent<PlayerUI>().Configure(this, _pokerTableState);
            }
        }

        public override string ToString()
        {
            return $"Poker state for {OwnerClientId} (seat {tablePosition.Value})";
        }
    }
}