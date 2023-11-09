using AceInTheHole.Tables.Poker.Server;
using UnityEngine.UIElements;
namespace AceInTheHole.Tables.Poker.Client.UI
{
    public class MoneyModule : IUserInterfaceModule
    {
        public PokerPlayerState PokerPlayerState { get; set; }
        public PokerTableState PokerTableState { get; set; }

        Label _potStatus;
        Label _balanceStatus;
        
        public void Connect(UIDocument document)
        {
            _potStatus = document.rootVisualElement.Q<Label>("pot-status");
            _balanceStatus = document.rootVisualElement.Q<Label>("balance-status");
        }
        
        public void Revalidate()
        {
            _potStatus.text = $"Pot: {PokerTableState.potState.Value.Pot}";
            _balanceStatus.text = $"Balance: {PokerPlayerState.balance.Value}";
        }
    }
}