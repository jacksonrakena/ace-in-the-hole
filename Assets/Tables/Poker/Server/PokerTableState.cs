using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AceInTheHole.Engine;
using AceInTheHole.Tables.Poker.Client;
using AceInTheHole.Tables.Poker.Server.Betting;
using AceInTheHole.Tables.Poker.Server.PlayIn;
using JetBrains.Annotations;
using Unity.Netcode;
using Unity.VisualScripting;
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
        readonly Dictionary<int, PokerPlayerState> _playersBySeatPosition = new Dictionary<int, PokerPlayerState>();

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

        public GameObject SeatContainer;

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

        /**
         * Returns the next seat after the specified player, or the first seat at the table if player is null.
         */
        public PokerPlayerState NextOccupiedSeatAfter(PokerPlayerState player = null)
        {
            var nextOrdinalPosition = _playersBySeatPosition
                .Where(e => e.Value != null)
                .OrderBy(e => e.Key)
                .Cast<KeyValuePair<int, PokerPlayerState>?>()
                .FirstOrDefault(e => e != null && (player == null || e.Value.Key > player.tablePosition.Value));

            if (nextOrdinalPosition != null) return nextOrdinalPosition.Value.Value;
            
            return _playersBySeatPosition
                    .OrderBy(e => e.Key)
                    .Where(e => e.Value != null)
                    .FirstOrDefault(e => player == null || e.Key != player.tablePosition.Value).Value;
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
            
            var remainingPlayers = players.Where(k => k.tablePosition.Value > currentPlayerSeatId.Value).ToList();
            if (remainingPlayers.Any())
            {
                currentPlayerSeatId.Value = remainingPlayers.OrderBy(e => e.tablePosition.Value).First().tablePosition.Value;
            }
            else ProcessEndOfRotation();
        }
        
        public void Awake()
        {
            VisibleTableCards = new();
            winningPlayersBySeatId = new();
        }

        public PokerPlayerState CurrentPokerPlayer => _playersBySeatPosition[currentPlayerSeatId.Value];

        public IEnumerable<PokerPlayerState> AllPlayersAtTable => _playersBySeatPosition.Values.NotNull();

        public IEnumerable<PokerPlayerState> AllPlayersRemainingInHand =>
            _playersBySeatPosition
                .Values
                .Where(e => e != null
                            && this.potState.Value.PlayerBetStates != null
                            && this.potState.Value.PlayerBetStates.ContainsKey(e.OwnerClientId)
                            && this.potState.Value.PlayerBetStates[e.OwnerClientId].InRound);
        
        
        public GameObject PlayerStatePrefab;

        public void JoinTable(ulong clientId)
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

            var pokerPlayer = playerStateObject.GetComponent<PokerPlayerState>();
            
            if (_playersBySeatPosition.Any(e => e.Value == null))
            {
                var position = _playersBySeatPosition.First(e => e.Value == null).Key;
                _playersBySeatPosition[position] = pokerPlayer;
                pokerPlayer.tablePosition.Value = position;
                Log($"{pokerPlayer} joined table, assigned seat position {position}");
                if (_tableHost.Value == -1)
                {
                    AssignHost(pokerPlayer, position);
                }
                var targetSeat = SeatContainer.transform.Find("Seat" + position);
                if (!pokerPlayer.transform.parent.GetComponent<NetworkObject>().TrySetParent(targetSeat.transform))
                {
                    Debug.Log($"Failed to move {pokerPlayer} to seat {targetSeat}");
                }
                _nPlayerCount.Value++;
                pokerPlayer.ServerConnectToTable(this);
            }
            else
            {
                Log($"Player {pokerPlayer} can't join the table as there are no position available.");
            }
        }
        public void Log(string message)
        {
            Debug.Log($"{gameObject.name}: {message}");
        }
        public void AssignHost(PokerPlayerState pokerPlayer, int position)
        {
            _tableHost.Value = position;
            Log($"{pokerPlayer} assigned as table host");
        }
        public void LeaveTable(PokerPlayerState pokerPlayer)
        {
            Log($"{pokerPlayer} left table {gameObject.name}");
            
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
        
        PokerPlayerState TryAdvanceLittleBlind()
        {
            if (AllPlayersAtTable.Any(e => e.isLittleBlind.Value))
            {
                var current = AllPlayersAtTable.First(e => e.isLittleBlind.Value);
                var next = NextOccupiedSeatAfter(current);
                current.isLittleBlind.Value = false;
                next.isLittleBlind.Value = true;
                return next;
            }
            
            var player = NextOccupiedSeatAfter();
            player.isLittleBlind.Value = true;
            return player;
        }
        [CanBeNull] PokerPlayerState GetCurrentLittleBlind()
        {
            if (_playersBySeatPosition.Any(e => e.Value.isLittleBlind.Value))
            {
                return _playersBySeatPosition.First(e => e.Value.isLittleBlind.Value).Value;
            }
            return null;
        }
        
        [CanBeNull] PokerPlayerState TryAdvanceBigBlind()
        {
            if (AllPlayersAtTable.Any(e => e.isBigBlind.Value))
            {
                var current = AllPlayersAtTable.First(e => e.isBigBlind.Value);
                var next = NextOccupiedSeatAfter(current);
                current.isBigBlind.Value = false;
                next.isBigBlind.Value = true;
                return next;
            }
            
            var player = NextOccupiedSeatAfter(GetCurrentLittleBlind());
            player.isBigBlind.Value = true;
            return player;
        }

        public void StartPlayInRound(ref PotState tsv)
        {
            tsv.CurrentRequiredBet = 50;
            tsv.PlayerBetStates = new Dictionary<ulong, PlayerBetState>();
            var lb = TryAdvanceLittleBlind();
            var bb = TryAdvanceBigBlind();
            Debug.Log($"Little blind={lb}, Big blind={bb}");

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
        
        public void ProcessEndOfRotation()
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
                if (remainingPlayers.Count() == 1)
                {
                    StartCoroutine(EndGame());
                    potState.Value = new PotState();
                    return;
                }   
            }

            switch (stage.Value)
            {
                case RoundStage.Setup:
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
                    StartCoroutine(EndGame());
                    tsv = new PotState();
                    break;
                case RoundStage.End:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            potState.Value = tsv;
            currentPlayerSeatId.Value = NextOccupiedSeatAfter().tablePosition.Value;
        }

        public List<KeyValuePair<PokerPlayerState, Set>> CurrentLeaders()
        {
            var list = new List<KeyValuePair<PokerPlayerState, Set>>();

            foreach (var player in AllPlayersRemainingInHand)
            {
                foreach (Set set in Set.FindAllPossibleSets(player.Cards, VisibleTableCards))
                {
                    if (list.Count == 0)
                    {
                        list.Add(new KeyValuePair<PokerPlayerState, Set>(player, set));
                    }
                    else
                    {
                        if (set > list.First().Value)
                        {
                            list.Clear();
                            list.Add(new KeyValuePair<PokerPlayerState, Set>(player, set));
                        }
                        else if (list.First().Value.CompareTo(set) == 0)
                        {
                            list.Add(new KeyValuePair<PokerPlayerState, Set>(player, set));
                        }
                    }
                }
            }
            return list;
        }

        IEnumerator EndGame()
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

            yield return new WaitForSeconds(5);

            foreach (var player in AllPlayersAtTable)
            {
                player.Cards.Clear();
            }
            
            stage.Value = RoundStage.Setup;
            currentPlayerSeatId.Value = -1;
            winningPlayersBySeatId.Clear();
            _dealerCards.Clear();
            VisibleTableCards.Clear();
            _deck.Reset();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestGameStartServerRpc(ServerRpcParams prams = default)
        {
            var playerRequesting = _playersBySeatPosition.FirstOrDefault(d => d.Value.OwnerClientId == prams.Receive.SenderClientId);
            if (playerRequesting.Value.IsTableHost != true) return;
            if (stage.Value != RoundStage.Setup) return;
            ProcessEndOfRotation();
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