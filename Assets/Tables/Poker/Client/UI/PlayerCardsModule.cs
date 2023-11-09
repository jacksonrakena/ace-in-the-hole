using System.Linq;
using AceInTheHole.Tables.Poker.Server;
using UnityEngine.UIElements;
namespace AceInTheHole.Tables.Poker.Client.UI
{
    public class PlayerCardsModule : IUserInterfaceModule
    {
        public PokerPlayerState PokerPlayerState { get; set; }
        public PokerTableState PokerTableState { get; set; }
        
        
        VisualElement _player;
        Button _seeCardsBtn;
        VisualElement _playerCards;
        public void Connect(UIDocument document)
        {
            _player = document.rootVisualElement.Q<VisualElement>("Player");
            _playerCards = _player.Q<VisualElement>("cardcontainer");
            _seeCardsBtn = _player.Q<Button>("see-cards-btn");
            
            IUserInterfaceModule.Hide(_playerCards);
            
            _seeCardsBtn.RegisterCallback<PointerDownEvent>(pde =>
            {
                IUserInterfaceModule.Show(_playerCards);
                PokerPlayerState.ViewingCards.Value = true;
            }, TrickleDown.TrickleDown);
            
            _seeCardsBtn.RegisterCallback<PointerUpEvent>(_ => 
            { 
                IUserInterfaceModule.Hide(_playerCards);
                PokerPlayerState.ViewingCards.Value = false;
            }, TrickleDown.TrickleDown);
        }
        public void Revalidate()
        {            
            IUserInterfaceModule.SetVisible(_seeCardsBtn, PokerPlayerState.Cards.Count > 0);
            
            foreach (VisualElement card in _playerCards.Children())
            {
                card.style.backgroundImage = null;
            }
            
            for (int i = 0; i < PokerPlayerState.Cards.Count; i++)
            {
                _playerCards.Children().ElementAt(i).style.backgroundImage =
                    PokerPlayerState.Cards[i].Resolve2D();
            }
        }
    }
}