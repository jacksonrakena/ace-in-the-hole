using System;
using System.Collections.Generic;
using System.Linq;
using AceInTheHole.Engine;
using AceInTheHole.Tables.Poker.Client;
using AceInTheHole.Tables.Poker.Server.Betting;
using AceInTheHole.Tables.Poker.Server.PlayIn;
using Unity.Netcode;
using UnityEngine;
namespace AceInTheHole.Tables.Poker.Server
{
    public class PokerTableState : NetworkBehaviour
    {
        /*
         * The deck.
         */
        readonly Deck _deck = new Deck();
        
        /*
         * All five dealer cards.
         */
        readonly List<Card> _dealerCards = new List<Card>();
        
        /*
         * Connected players (including those not playing in a round), keyed by their seat position
         */
        readonly Dictionary<int, PokerPlayerState> _playersBySeatPosition = new Dictionary<int, PokerPlayerState>
        {
            {0, null},
            {1, null},
            {2, null},
            {3, null},
            {4, null}
        };

        /**
         * The host (dealer) of the table.
         * At the moment, this variable only changes when the table host disconnects.
         */
        public NetworkVariable<int> _tableHost = new NetworkVariable<int>(-1);

        public NetworkVariable<int> _nPlayerCount = new NetworkVariable<int>(0);
        
        /*
         * The cards that players can see on the table.
         */
        public NetworkList<Card> VisibleTableCards;
        
        /**
         * The state of the game.
         */
        public NetworkVariable<RoundStage> stage = new NetworkVariable<RoundStage>();

        /*
         * The seat position of the current player. (-1 if no player is playing)
         */
        public NetworkVariable<int> currentPlayerSeatId = new NetworkVariable<int>(-1);

        /*
         * The seat position of the winning player. (-1 if no player is playing)
         */
        public NetworkList<int> winningPlayersBySeatId;
        
        /*
         * The state of the pot (including betting, side pots, folds, etc)
         */
        public NetworkVariable<PotState> potState = new NetworkVariable<PotState>(new PotState());

        /*
         * The join code (shown to the player to share to other users)
         */
        public string JoinCode;
        
        public KeyValuePair<int, PokerPlayerState> FirstOccupiedSeat(int excluding = -1)
        {
            return _playersBySeatPosition.OrderBy(e => e.Key).FirstOrDefault(e => e.Key != excluding && e.Value != null);
        }
        
        public KeyValuePair<int, PokerPlayerState> FirstUnoccupiedSeat(int excluding = -1)
        {
            return _playersBySeatPosition.OrderBy(e => e.Key).FirstOrDefault(e => e.Key != excluding && e.Value == null);
        }
        
        public void TryAdvancePlayer()
        {
            var players = AllPlayersRemainingInHand;
            
            // In the play-in round, no player has "folded" yet, so get all the players at the table
            // AllPlayersRemainingInHand filters out players who have not placed a bet
            if (stage.Value == RoundStage.PlayIn)
            {
                players = AllPlayersAtTable;
            }
            
            var remainingPlayers = players.Where(k => k.Key > currentPlayerSeatId.Value).ToList();
            if (remainingPlayers.Any())
            {
                currentPlayerSeatId.Value = remainingPlayers.OrderBy(e => e.Key).First().Key;
            }
            else Turn();
        }
        
        public void Awake()
        {
            VisibleTableCards = new();
            winningPlayersBySeatId = new();
        }

        public PokerPlayerState CurrentPokerPlayer => _playersBySeatPosition[currentPlayerSeatId.Value];

        public Dictionary<int, PokerPlayerState> AllPlayersAtTable => _playersBySeatPosition.Where(e => e.Value != null).ToDictionary(e => e.Key, e => e.Value);

        public Dictionary<int, PokerPlayerState> AllPlayersRemainingInHand =>
            _playersBySeatPosition
                .Where(e => e.Value != null 
                            && this.potState.Value.PlayerBetStates != null
                            && this.potState.Value.PlayerBetStates.ContainsKey(e.Value.OwnerClientId)
                            && this.potState.Value.PlayerBetStates[e.Value.OwnerClientId].InRound)
                .ToDictionary(e => e.Key, e => e.Value);

        PokerPlayerState LittleBlind
        {
            get
            {
                if (AllPlayersAtTable.Values.Any(e => e.isLittleBlind.Value)) return AllPlayersAtTable.FirstOrDefault(e => e.Value.isLittleBlind.Value).Value;
                var player = FirstOccupiedSeat().Value;
                player.isLittleBlind.Value = true;
                return player;
            }
        }

        public PokerPlayerState BigBlind
        {
            get
            {
                if (AllPlayersAtTable.Values.Any(e => e.isBigBlind.Value)) return AllPlayersAtTable.FirstOrDefault(e => e.Value.isBigBlind.Value).Value;
                PokerPlayerState little = LittleBlind;
                PokerPlayerState pokerPlayer = FirstOccupiedSeat(little.tablePosition.Value).Value;
                pokerPlayer.isBigBlind.Value = true;
                return pokerPlayer;
            }
        }


        public void JoinTable(PokerPlayerState pokerPlayer)
        {
            if (_playersBySeatPosition.Any(e => e.Value == null))
            {
                var position = _playersBySeatPosition.First(e => e.Value == null).Key;
                _playersBySeatPosition[position] = pokerPlayer;
                pokerPlayer.tablePosition.Value = position;
                Debug.Log($"SERVER: {pokerPlayer} connected, assigned seat position {position}");
                if (_tableHost.Value == -1)
                {
                    AssignHost(pokerPlayer, position);
                }
                var targetSeat = gameObject.transform.Find("Table/PokerTable1/Seat" + position);
                if (!pokerPlayer.transform.parent.GetComponent<NetworkObject>().TrySetParent(targetSeat.transform))
                {
                    Debug.Log($"Failed to move {pokerPlayer} to seat {targetSeat}");
                }
                _nPlayerCount.Value++;
                pokerPlayer.JoinTableClientRpc();
            }
            else
            {
                Debug.Log($"ERROR: Player {pokerPlayer} can't join the table as there are no position available.");
            }
        }
        public void AssignHost(PokerPlayerState pokerPlayer, int position)
        {
            _tableHost.Value = position;
            Debug.Log($"SERVER: {pokerPlayer} assigned as table host");
        }
        public void LeaveTable(PokerPlayerState pokerPlayer)
        {
            Debug.Log($"SERVER: {pokerPlayer} disconnected");
            
            _nPlayerCount.Value--;
            _playersBySeatPosition[pokerPlayer.tablePosition.Value] = null;
            
            if (_tableHost.Value == pokerPlayer.tablePosition.Value)
            {
                var otherPlayers = _playersBySeatPosition
                    .Where(d => d.Value != null).ToList();
                if (otherPlayers.Any())
                {
                    AssignHost(otherPlayers.First().Value, otherPlayers.First().Key);
                }
            }
            
            if (potState.Value.GetCurrentBetStateFor(pokerPlayer) != null)
            {
                var tsv = potState.Value.Clone();
                tsv.PlayerBetStates.Remove(pokerPlayer.OwnerClientId);
                potState.Value = tsv;
            }

            if (currentPlayerSeatId.Value == pokerPlayer.tablePosition.Value)
            {
                TryAdvancePlayer();
            }
        }

        public void StartPlayInRound(ref PotState tsv)
        {
            tsv.CurrentRequiredBet = 50;
            tsv.PlayerBetStates = new Dictionary<ulong, PlayerBetState>();
            var lb = LittleBlind;
            var bb = BigBlind;
            Debug.Log("Picked blinds");

            var smallBetHalf = (int)(0.5f * tsv.CurrentRequiredBet);

            lb.balance.Value = (int)(lb.balance.Value - smallBetHalf);
            tsv.Pot = (tsv.Pot + smallBetHalf);
            tsv.PlayerBetStates.Add(lb.OwnerClientId, new PlayerBetState { Amount = smallBetHalf, InRound = true});
            lb.Cards.Add(_deck.Draw());
            lb.Cards.Add(_deck.Draw());

            bb.balance.Value = (int)(bb.balance.Value - tsv.CurrentRequiredBet);
            tsv.Pot = (tsv.Pot + tsv.CurrentRequiredBet);
            tsv.PlayerBetStates.Add(bb.OwnerClientId, new PlayerBetState { Amount = tsv.CurrentRequiredBet, InRound = true });
            bb.Cards.Add(_deck.Draw());
            bb.Cards.Add(_deck.Draw());
                    
            _dealerCards.Add(_deck.Draw());
            _dealerCards.Add(_deck.Draw());
            _dealerCards.Add(_deck.Draw());
            _dealerCards.Add(_deck.Draw());
            _dealerCards.Add(_deck.Draw());

            stage.Value = RoundStage.PlayIn;
        }
        
        public void Turn()
        {
            PotState tsv;
            if (potState.Value.PlayerBetStates == null)
            {
                tsv = new PotState
                {
                    PlayerBetStates = new Dictionary<ulong, PlayerBetState>()
                };
            }
            else tsv = potState.Value.Clone();
            
            if (stage.Value != RoundStage.Setup)
            {
                var remainingPlayers = AllPlayersRemainingInHand;
                if (remainingPlayers.Count == 1)
                {
                    potState.Value = EndGame().GetAwaiter().GetResult();
                    return;
                }   
            }

            switch (stage.Value)
            {
                case RoundStage.Setup:
                    Debug.Log("Starting play in round");
                    StartPlayInRound(ref tsv);
                    break;
                case RoundStage.PlayIn:
                    stage.Value = RoundStage.Flop;
                    VisibleTableCards.Add(_dealerCards[0]);
                    VisibleTableCards.Add(_dealerCards[1]);
                    VisibleTableCards.Add(_dealerCards[2]);
                    break;
                case RoundStage.Flop:
                    VisibleTableCards.Add(_dealerCards[3]);
                    stage.Value = RoundStage.Turn;
                    break;
                case RoundStage.Turn:
                    VisibleTableCards.Add(_dealerCards[4]);
                    stage.Value = RoundStage.River;
                    break;
                case RoundStage.River:
                    tsv = EndGame().GetAwaiter().GetResult();
                    break;
                case RoundStage.End:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            potState.Value = tsv;
            currentPlayerSeatId.Value = FirstOccupiedSeat().Key;
            Debug.Log($"Turn complete, going to {stage.Value}");
        }

        public List<KeyValuePair<PokerPlayerState, Set>> CurrentLeaders()
        {
            var list = new List<KeyValuePair<PokerPlayerState, Set>>();

            foreach (var player in AllPlayersRemainingInHand)
            {
                foreach (Set set in Set.FindAllPossibleSets(player.Value.Cards, VisibleTableCards))
                {
                    if (list.Count == 0)
                    {
                        list.Add(new KeyValuePair<PokerPlayerState, Set>(player.Value, set));
                    }
                    else
                    {
                        if (set > list.First().Value)
                        {
                            list.Clear();
                            list.Add(new KeyValuePair<PokerPlayerState, Set>(player.Value, set));
                        }
                        else if (list.First().Value.CompareTo(set) == 0)
                        {
                            list.Add(new KeyValuePair<PokerPlayerState, Set>(player.Value, set));
                        }
                    }
                }
            }
            return list;
        }

        async Awaitable<PotState> EndGame()
        {
            var winners = CurrentLeaders();
            
            foreach (var winner in winners)
            {
                winningPlayersBySeatId.Add(winner.Key.tablePosition.Value);
                winner.Key.balance.Value += potState.Value.Pot / winners.Count;
            }
            
            stage.Value = RoundStage.End;
            currentPlayerSeatId.Value = -1;
                    
            Debug.Log($"Players [{string.Join(", ", winners.Select(e => e.Key.ToString()))}] win.");
            
            await Awaitable.WaitForSecondsAsync(5);

            foreach (var player in AllPlayersAtTable)
            {
                player.Value.Cards.Clear();
            }
            
            stage.Value = RoundStage.Setup;
            currentPlayerSeatId.Value = -1;
            winningPlayersBySeatId.Clear();
            _dealerCards.Clear();
            VisibleTableCards.Clear();
            _deck.Reset();

            return new PotState();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestGameStartServerRpc(ServerRpcParams prams = default)
        {
            var playerRequesting = _playersBySeatPosition.FirstOrDefault(d => d.Value.OwnerClientId == prams.Receive.SenderClientId);
            if (playerRequesting.Value.IsTableHost != true) return;
            if (stage.Value != RoundStage.Setup) return;
            Debug.Log("Requested game start from " + playerRequesting.Key);
            Turn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ConfirmPlayInOptionServerRpc(PlayInOption play, ServerRpcParams prams = default)
        {
            if (CurrentPokerPlayer.OwnerClientId != prams.Receive.SenderClientId) return;

            var tsv = potState.Value.Clone();
            var currentPlayerBet = tsv.GetCurrentBetStateFor(CurrentPokerPlayer) ?? new PlayerBetState { Amount = 0, InRound = true };
            if (play == PlayInOption.Fold)
            {
                currentPlayerBet.InRound = false;
            }
            else
            {
                var val = (potState.Value.CurrentRequiredBet - currentPlayerBet.Amount);
                currentPlayerBet.Amount += val;
                CurrentPokerPlayer.balance.Value -= (int) val;
                tsv.Pot += val;
            }
            tsv.PlayerBetStates[CurrentPokerPlayer.OwnerClientId] = currentPlayerBet;
            potState.Value = tsv;

            TryAdvancePlayer();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ConfirmHandOptionServerRpc(BetAction action, ServerRpcParams prams = default)
        {
            if (CurrentPokerPlayer.OwnerClientId != prams.Receive.SenderClientId) return;

            var tsv = potState.Value.Clone();
            var currentPlayerBet = tsv.PlayerBetStates[CurrentPokerPlayer.OwnerClientId];
            switch (action.Type)
            {
                case BetActionType.Raise:
                    var newMinimum = tsv.CurrentRequiredBet + action.Amount;
                    tsv.CurrentRequiredBet = newMinimum;
                    currentPlayerBet.Amount += action.Amount;
                    CurrentPokerPlayer.balance.Value = (int)(CurrentPokerPlayer.balance.Value - action.Amount);
                    tsv.Pot += action.Amount;
                    break;
                case BetActionType.Check:
                    var difference = tsv.CurrentRequiredBet - currentPlayerBet.Amount;
                    currentPlayerBet.Amount += difference;
                    CurrentPokerPlayer.balance.Value = CurrentPokerPlayer.balance.Value - difference;
                    tsv.Pot += difference;
                    break;
                case BetActionType.Fold:
                    currentPlayerBet.InRound = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            tsv.PlayerBetStates[CurrentPokerPlayer.OwnerClientId] = currentPlayerBet;
            potState.Value = tsv;

            TryAdvancePlayer();
        }
        
        public override void OnDestroy()
        {
            currentPlayerSeatId.Dispose();
            potState.Dispose();
            base.OnDestroy();
        }
    }
}