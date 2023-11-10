using AceInTheHole.Tables.Poker.Server;
using AceInTheHole.Tables.Poker.Server.PlayIn;
using UnityEngine.UIElements;
namespace AceInTheHole.Tables.Poker.Client.UI
{
    public class PlayInModule : IUserInterfaceModule
    {
        public PokerPlayerState PokerPlayerState { get; set; }
        public PokerTableState PokerTableState { get; set; }
       
        VisualElement _playUi;
        Button _playHandBtn;
        Button _foldHandBtn;
        
        public void Connect(UIDocument document)
        {
            _playUi = document.rootVisualElement.Q<VisualElement>("play-ui");
            _playHandBtn = _playUi.Q<Button>("play-hand-btn");
            _foldHandBtn = _playUi.Q<Button>("fold-hand-btn");
            
            _playHandBtn.RegisterCallback<ClickEvent>(e => PlayerHandConfirm(PlayInOption.Play));
            _foldHandBtn.RegisterCallback<ClickEvent>(e => PlayerHandConfirm(PlayInOption.Fold));
        }
        
        public void PlayerHandConfirm(PlayInOption opt)
        {
            IUserInterfaceModule.Hide(_playUi);
            PokerTableState.ConfirmPlayInOptionServerRpc(opt);
        }
        
        public void Revalidate()
        {
            if (PokerTableState.currentPlayerSeatId.Value == PokerPlayerState.tablePosition.Value && PokerTableState.stage.Value == RoundStage.PlayIn)
            {
                _playHandBtn.text = $"Play hand (${PokerPlayerState.RequiredIncreaseToCheck})";
                IUserInterfaceModule.Show(_playUi);
            }
            else IUserInterfaceModule.Hide(_playUi);
        }
    }
}