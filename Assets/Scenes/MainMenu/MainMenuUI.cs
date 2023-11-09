using AceInTheHole.Network;
using UnityEngine;
using UnityEngine.UIElements;
namespace AceInTheHole.Scenes.MainMenu
{
    public class MainMenuUI : MonoBehaviour
    {
        public UIDocument m_MainMenuUI = default;
    
        private Button m_HostGameButton;
        private Button m_JoinGameButton;

        Button hostRelay;
        Button joinRelay;
        TextField relayJoinCode;

        public RelayManager _relayManager;

        void Start()
        {
            m_HostGameButton = m_MainMenuUI.rootVisualElement.Query<Button>("host-game-btn").First();
            m_JoinGameButton = m_MainMenuUI.rootVisualElement.Query<Button>("join-game-btn").First();
            hostRelay = m_MainMenuUI.rootVisualElement.Q<Button>("host-relay");
            joinRelay = m_MainMenuUI.rootVisualElement.Q<Button>("join-relay");
            relayJoinCode = m_MainMenuUI.rootVisualElement.Q<TextField>("relay-join-code");
        
            hostRelay.RegisterCallback<ClickEvent>(c =>
            {
                _relayManager.InitialiseHostRelayAsync();
            });
            joinRelay.RegisterCallback<ClickEvent>(c =>
            {
                _relayManager.ConnectToRelayAsync(relayJoinCode.value);
            });
        
            m_HostGameButton.RegisterCallback<ClickEvent>(c =>
            {
                _relayManager.StartHost();
            });
            m_JoinGameButton.RegisterCallback<ClickEvent>(c =>
            {
                _relayManager.ConnectToLocal();
            });
        }
    }
}
