using AceInTheHole.Tables.Poker.Server;
using AceInTheHole.Tables.Poker.Server.Betting;
using UnityEngine.UIElements;
namespace AceInTheHole.Tables.Poker.Client.UI
{
    public class ActionModule : IUserInterfaceModule
    {
        public PokerPlayerState PokerPlayerState { get; set; }
        public PokerTableState PokerTableState { get; set; }
        
        VisualElement _actionUi;
        VisualElement _raiseUi;
        VisualElement _actionBtns;
        Button _betBtn;
        Button _checkBtn;
        Button _foldBtn;
        Slider _raiseSlider;
        Button _raiseQuartBtn;
        Button _raiseHalfBtn;
        Button _raise3QuartBtn;
        Button _raiseAllInBtn;
        Button _confirmRaiseBtn;
        
        public void Connect(UIDocument document)
        {
            _actionUi = document.rootVisualElement.Query<VisualElement>("action-ui").First();
            _betBtn = _actionUi.Query<Button>("bet-btn").First();
            _betBtn.RegisterCallback<ClickEvent>(_ =>
            {
                IUserInterfaceModule.Hide(_actionBtns);
                IUserInterfaceModule.Show(_raiseUi);
            });
            _checkBtn = _actionUi.Query<Button>("check-btn").First();
            _foldBtn = _actionUi.Query<Button>("fold-btn").First();
            _checkBtn.RegisterCallback<ClickEvent>((e)=>ConfirmMove(BetActionType.Check, 0));
            _foldBtn.RegisterCallback<ClickEvent>(e=>ConfirmMove(BetActionType.Fold, (int) _raiseSlider.value));
            _raiseSlider = _actionUi.Query<Slider>("raise-slider").First();
            _confirmRaiseBtn = _actionUi.Q<Button>("confirm-raise");
            _confirmRaiseBtn.RegisterCallback<ClickEvent>((e)=>ConfirmMove(BetActionType.Raise, (int) _raiseSlider.value));

            _raiseUi = _actionUi.Q<VisualElement>("raise-ui");
            _actionBtns = _actionUi.Q<VisualElement>("action-buttons");

            _raiseQuartBtn = _actionUi.Q<Button>("raise-quart");
            _raiseQuartBtn.RegisterCallback<ClickEvent>(e => _raiseSlider.value = 0.25f*PokerPlayerState.balance.Value);
            _raiseHalfBtn = _actionUi.Q<Button>("raise-half");
            _raiseHalfBtn.RegisterCallback<ClickEvent>(e => _raiseSlider.value = 0.5f*PokerPlayerState.balance.Value);
            _raise3QuartBtn = _actionUi.Q<Button>("raise-3quart");
            _raise3QuartBtn.RegisterCallback<ClickEvent>(e => _raiseSlider.value = 0.75f*PokerPlayerState.balance.Value);
            _raiseAllInBtn = _actionUi.Q<Button>("raise-allin");
            _raiseAllInBtn.RegisterCallback<ClickEvent>(e => _raiseSlider.value = PokerPlayerState.balance.Value);

            _raiseSlider.RegisterValueChangedCallback((v) => _confirmRaiseBtn.text = $"Raise ${v.newValue}");
        }
        public void ConfirmMove(BetActionType type, int amount)
        {
            IUserInterfaceModule.Hide(_actionUi);
            
            PokerTableState.ConfirmHandOptionServerRpc(new BetAction { Type = type, Amount = amount});
        }
        public void Revalidate()
        {
            if (PokerTableState.currentPlayerSeatId.Value == PokerPlayerState.tablePosition.Value
                && PokerTableState.stage.Value is RoundStage.River or RoundStage.Flop or RoundStage.Turn)
            {
                IUserInterfaceModule.Show(_actionUi);
                IUserInterfaceModule.Show(_actionBtns);

                IUserInterfaceModule.Hide(_raiseUi);
                
                _betBtn.visible = PokerPlayerState.balance.Value > 0;
                _raiseSlider.lowValue = 1;
                _raiseSlider.highValue = PokerPlayerState.balance.Value;
                _raiseSlider.value = 0.25f * PokerPlayerState.balance.Value;
                var requiredRaise = PokerPlayerState.RequiredIncreaseToCheck;
                _checkBtn.text = requiredRaise > 0 ? $"Call (${requiredRaise})" : "Check";
                
            }
            else
            {
                IUserInterfaceModule.Hide(_actionUi);
                IUserInterfaceModule.Hide(_actionBtns);
            }
        }
    }
}