using System.Globalization;
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
            var culture = CultureInfo.CurrentCulture;
            var nfi = (NumberFormatInfo) culture.NumberFormat.Clone();
            nfi.CurrencyDecimalDigits = 0;
            _potStatus.text = $"Pot: {PokerTableState.pot.Value.ToString("C", nfi)}";
            _balanceStatus.text = $"Balance: {PokerPlayerState.balance.Value.ToString("C", nfi)}";
        }
    }
}