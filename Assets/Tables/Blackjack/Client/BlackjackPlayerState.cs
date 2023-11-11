using AceInTheHole.Engine;
using AceInTheHole.Tables.Base;
using AceInTheHole.Tables.Blackjack.Server;
using Unity.Netcode;
namespace AceInTheHole.Tables.Blackjack.Client
{
    public class BlackjackPlayerState : OrdinalTablePlayerStateBase<BlackjackPlayerState, BlackjackTableState>
    {
        public NetworkList<Card> Cards;

        public void Awake()
        {
            Cards = new NetworkList<Card>(null);
        }
        
        public override void OnJoinTable()
        {
            
        }
        public override void OnLeaveTable()
        {
        }
    }
}