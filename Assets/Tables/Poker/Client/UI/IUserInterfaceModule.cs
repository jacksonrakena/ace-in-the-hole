using AceInTheHole.Tables.Poker.Server;
using UnityEngine.UIElements;
namespace AceInTheHole.Tables.Poker.Client.UI
{
    public interface IUserInterfaceModule
    {
        public PokerPlayerState PokerPlayerState { get; set; }
        public PokerTableState PokerTableState { get; set; }
        public void Connect(UIDocument document);

        public void Revalidate();

        public static void Hide(VisualElement ve)
        {
            ve.style.display = DisplayStyle.None;
        }
        public static void Show(VisualElement ve)
        {
            ve.style.display = DisplayStyle.Flex;
        }
        public static void SetVisible(VisualElement ve, bool visible)
        {
            if (visible) { Show(ve); }
            else { Hide(ve); }
        }
    }
}