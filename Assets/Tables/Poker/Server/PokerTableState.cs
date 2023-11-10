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
using UnityEngine.Serialization;
namespace AceInTheHole.Tables.Poker.Server
{
    public class PokerTableState : NetworkBehaviour
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
        
        readonly Deck _deck = new Deck();
        readonly List<Card> _dealerCards = new List<Card>();
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
        public NetworkList<int> WinningPlayersBySeatId;
        
        /*
         * The current raise amount, that is, how much each player must bet to continue playing.
         */
        public NetworkVariable<int> currentRequiredBet;
        
        /*
         * The total size of the pot available to the winner of this round.
         */
        public NetworkVariable<int> pot;

        /**
         * Returns the next seat after the specified player, or the first seat at the table if player is null.
         */
        PokerPlayerState NextOccupiedSeatAfter(PokerPlayerState player = null) 
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
        
        void TryAdvancePlayer()
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
            WinningPlayersBySeatId = new();
        }

        [CanBeNull] public PokerPlayerState CurrentPlayer => currentPlayerSeatId.Value == -1 ? null : _playersBySeatPosition[currentPlayerSeatId.Value];

        public IEnumerable<PokerPlayerState> AllPlayersAtTable => _playersBySeatPosition.Values.NotNull();

        public IEnumerable<PokerPlayerState> AllPlayersRemainingInHand =>
            _playersBySeatPosition
                .Values
                .Where(e => e != null
                            && e.betState.Value is { InRound: true });
        
        
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
            tableHost.Value = position;
            Log($"{pokerPlayer} assigned as table host");
        }
        public void LeaveTable(PokerPlayerState pokerPlayer)
        {
            Log($"{pokerPlayer} left table {gameObject.name}");
            
            playerCount.Value--;
            _playersBySeatPosition[pokerPlayer.tablePosition.Value] = null;
            
            if (tableHost.Value == pokerPlayer.tablePosition.Value)
            {
                var otherPlayers = _playersBySeatPosition
                    .Where(d => d.Value != null).ToList();
                if (otherPlayers.Any())
                {
                    AssignHost(otherPlayers.First().Value, otherPlayers.First().Key);
                }
            }
            
            if (pokerPlayer.betState.Value != null)
            {
                pokerPlayer.betState.Value = null;
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
        
        PokerPlayerState TryAdvanceBigBlind()
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

        public void StartPlayInRound()
        {
            currentRequiredBet.Value = 50;
            var lb = TryAdvanceLittleBlind();
            var bb = TryAdvanceBigBlind();
            Debug.Log($"Little blind={lb}, Big blind={bb}");

            var smallBetHalf = (int)(0.5f * currentRequiredBet.Value);

            lb.balance.Value -= smallBetHalf;
            pot.Value += smallBetHalf;
            lb.betState.Value = new PlayerBetState { Amount = smallBetHalf, InRound = true };
            lb.Cards.Add(_deck.Draw());
            lb.Cards.Add(_deck.Draw());

            bb.balance.Value -= currentRequiredBet.Value;
            pot.Value += currentRequiredBet.Value;
            bb.betState.Value = new PlayerBetState { Amount = currentRequiredBet.Value, InRound = true };
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
            if (stage.Value != RoundStage.Setup)
            {
                var remainingPlayers = AllPlayersRemainingInHand;
                if (remainingPlayers.Count() == 1)
                {
                    StartCoroutine(EndGame());
                    return;
                }   
            }

            switch (stage.Value)
            {
                case RoundStage.Setup:
                    StartPlayInRound();
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
                    break;
                case RoundStage.End:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                WinningPlayersBySeatId.Add(winner.Key.tablePosition.Value);
                winner.Key.balance.Value += pot.Value / winners.Count;
            }
            
            stage.Value = RoundStage.End;
            currentPlayerSeatId.Value = -1;
                    
            Debug.Log($"Players [{string.Join(", ", winners.Select(e => e.Key.ToString()))}] win.");

            yield return new WaitForSeconds(5);

            foreach (var player in AllPlayersAtTable)
            {
                player.Cards.Clear();
                player.betState.Value = null;
            }

            pot.Value = 0;
            stage.Value = RoundStage.Setup;
            currentPlayerSeatId.Value = -1;
            WinningPlayersBySeatId.Clear();
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
            if (CurrentPlayer.OwnerClientId != prams.Receive.SenderClientId) return;

            var currentPlayerBet = CurrentPlayer.betState.Value ?? new PlayerBetState() { Amount = 0, InRound = true };
            if (play == PlayInOption.Fold)
            {
                currentPlayerBet.InRound = false;
            }
            else
            {
                var val = (currentRequiredBet.Value - currentPlayerBet.Amount);
                currentPlayerBet.Amount += val;
                CurrentPlayer.balance.Value -= val;
                pot.Value += val;
            }
            CurrentPlayer.betState.Value = currentPlayerBet;

            TryAdvancePlayer();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ConfirmHandOptionServerRpc(BetAction action, ServerRpcParams prams = default)
        {
            if (CurrentPlayer.OwnerClientId != prams.Receive.SenderClientId) return;

            var currentPlayerBet = CurrentPlayer.betState.Value!.Value;
            switch (action.Type)
            {
                case BetActionType.Raise:
                    currentRequiredBet.Value += action.Amount;
                    currentPlayerBet.Amount += action.Amount;
                    CurrentPlayer.balance.Value = (int)(CurrentPlayer.balance.Value - action.Amount);
                    pot.Value += action.Amount;
                    break;
                case BetActionType.Check:
                    var difference = currentRequiredBet.Value - currentPlayerBet.Amount;
                    currentPlayerBet.Amount += difference;
                    CurrentPlayer.balance.Value -= difference;
                    pot.Value += difference;
                    break;
                case BetActionType.Fold:
                    currentPlayerBet.InRound = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            CurrentPlayer.betState.Value = currentPlayerBet;

            TryAdvancePlayer();
        }
        
        public override void OnDestroy()
        {
            currentPlayerSeatId.Dispose();
            base.OnDestroy();
        }
    }
}