using AceInTheHole.Tables.Poker.Server;
using UnityEngine.UIElements;
namespace AceInTheHole.Tables.Poker.Client.UI
{
    public class HostModule : IUserInterfaceModule
    {
        public PokerPlayerState PokerPlayerState { get; set; }
        public PokerTableState PokerTableState { get; set; }

        VisualElement _hostPanel;
        Button _leaveTableBtn;
        Button _startGameButton;
        Label _joinCode;
        
        public void Connect(UIDocument document)
        {
            _leaveTableBtn = document.rootVisualElement.Q<Button>("leave-table-btn");
            _leaveTableBtn.RegisterCallback<ClickEvent>(s =>
            {
                PokerPlayerState.Client_LeaveTable();
            });
            _hostPanel = document.rootVisualElement.Q<VisualElement>("host-panel");
            _startGameButton = _hostPanel.Q<Button>("start-game");
            _joinCode = _hostPanel.Q<Label>("join-code");
            _startGameButton.RegisterCallback<ClickEvent>((s) =>
            {
                PokerTableState.RequestGameStartServerRpc();
                IUserInterfaceModule.Hide(_startGameButton);
            });
        }
        
        
        public void Revalidate()
        {
            var isTableHost = PokerPlayerState.IsTableHost;
            _hostPanel.visible = isTableHost; 
            IUserInterfaceModule.SetVisible(_startGameButton, 
                isTableHost && PokerTableState.playerCount.Value > 1 && PokerTableState.stage.Value == RoundStage.Setup);
            //_joinCode.text = PokerTableState.JoinCode != null ? $"Join code: {PokerTableState.JoinCode}" : null;
        }
    }
}