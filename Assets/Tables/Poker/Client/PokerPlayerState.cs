using AceInTheHole.Engine;
using AceInTheHole.Player;
using AceInTheHole.Tables.Base;
using AceInTheHole.Tables.Poker.Client.UI;
using AceInTheHole.Tables.Poker.Server;
using AceInTheHole.Tables.Poker.Server.Betting;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
namespace AceInTheHole.Tables.Poker.Client
{
    public class PokerPlayerState : OrdinalTablePlayerStateBase<PokerPlayerState, PokerTableState>
    {
        public NetworkVariable<bool> isLittleBlind = new NetworkVariable<bool>();
        public NetworkVariable<bool> isBigBlind = new NetworkVariable<bool>();
        
        public NetworkVariable<int> balance = new NetworkVariable<int>(1000);
        public NetworkList<Card> Cards;
        public NetworkVariable<bool> ViewingCards = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<PlayerBetState?> betState 
            = new NetworkVariable<PlayerBetState?>
                (null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        public GameObject playerUiPrefab;
        GameObject playerUiInstance;

        public TextMeshPro bettingAmount;
        public TextMeshPro roleInfo;
        public GameObject currentPlayerIndicator;

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
            if (TableState.stage.Value == RoundStage.Setup)
            {
                roleInfo.text = "";
                bettingAmount.text = "";
            }
            else
            {

                if (TableState.WinningPlayersBySeatId.Contains(this.tablePosition.Value))
                {
                    roleInfo.text = TableState.WinningPlayersBySeatId.Count == 1 ? "Winner" : "Winner (Split pot)";
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

        public void RevalidateAnimationState(RoundStage old, RoundStage current)
        {
            if (TableState.stage.Value == RoundStage.End)
            {
                var animator = GetComponentInParent<NetworkAnimator>();
                if (TableState.WinningPlayersBySeatId.Contains(this.tablePosition.Value))
                {
                    animator.SetTrigger("Won round trigger");
                } else if (TableState.WinningPlayersBySeatId.Count > 0)
                {
                    animator.SetTrigger("Lost round trigger");
                }
            }
        }

        public void OnPotStateChanged(PlayerBetState? old, PlayerBetState? current) => Revalidate();
        public void OnCurrentPlayerChanged(int oldSeat, int newSeat) => Revalidate();
        public void OnWinningPlayersChanged(NetworkListEvent<int> changeData) => Revalidate();
        public void OnBlindsChange(bool oldValue, bool newValue) => Revalidate();

        public override void OnJoinTable()
        {
            if (IsServer)
            {
                TableState.stage.OnValueChanged += RevalidateAnimationState;
            }
            if (IsClient)
            {
                var player = gameObject.transform.parent;
                if (IsOwner)
                {
                    if (playerUiInstance == null)
                    {
                        playerUiInstance = Instantiate(playerUiPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        playerUiInstance.GetComponent<PlayerUI>().Configure(this, TableState);   
                    }
                }
                roleInfo = player.transform.Find("Role").GetComponent<TextMeshPro>();
                bettingAmount = player.transform.Find("BettingAmount").GetComponent<TextMeshPro>();
                name = ToString();
            }
        }

        public override void OnLeaveTable()
        {
            if (IsServer)
            {
                TableState.stage.OnValueChanged -= RevalidateAnimationState;
            }
            if (IsOwner)
            {
                if (IsClient)
                {
                    playerUiInstance.GetComponent<PlayerUI>().enabled = false;
                    playerUiInstance.GetComponent<PlayerUI>().destroyed = true;
                    Destroy(playerUiInstance);
                }
                betState.OnValueChanged -= OnPotStateChanged;
                TableState.currentPlayerSeatId.OnValueChanged -= OnCurrentPlayerChanged;
                TableState.WinningPlayersBySeatId.OnListChanged -= OnWinningPlayersChanged;
                isBigBlind.OnValueChanged -= OnBlindsChange;
                isLittleBlind.OnValueChanged -= OnBlindsChange;
            }
        }
        
        public int CurrentBetAmount => betState.Value?.Amount ?? 0;
        
        public int RequiredIncreaseToCheck => TableState.currentRequiredBet.Value - CurrentBetAmount;

        public bool HasFolded => !betState.Value?.InRound ?? false;
    }
}