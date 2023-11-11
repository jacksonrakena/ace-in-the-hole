using System.Collections.Generic;
using AceInTheHole.Engine;
using AceInTheHole.Tables.Base;
using AceInTheHole.Tables.Blackjack.Client;
using Unity.Netcode;

namespace AceInTheHole.Tables.Blackjack.Server
{
    public class BlackjackTableState : OrdinalTableBase<BlackjackPlayerState, BlackjackTableState>
    {
        public override PlayableTableInfo TableInfo => new PlayableTableInfo
        {
            Name = "Blackjack",
            Description = "Traditional blackjack. Play against the house and win big.",
            MaximumPlayers = 8,
            MinimumPlayers = 1
        };

        readonly Deck deck = new Deck();
        readonly List<Card> dealerCards = new List<Card>();


        public override void OnClientJoinTable(BlackjackPlayerState player)
        {
            
        }
        public override void OnClientLeaveTable(BlackjackPlayerState player)
        {
        }
    }
}