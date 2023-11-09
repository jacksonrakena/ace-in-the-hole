using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
namespace AceInTheHole.Client.Loading_Screens
{
    public enum LoadingState
    {
        [Description("Initialising")]
        Initialize,
        [Description("Connecting to game services")]
        InitializeNet,
        [Description("Creating lobby")]
        WaitForAllocation,
        [Description("Joining lobby")]
        WaitForCode,
        [Description("Loading world")]
        WaitForSync
    }
    public class LoadingScreen : MonoBehaviour
    {
        public UIDocument _loadingScreen;
    
        void Start()
        {
            _loadingScreen.rootVisualElement.Q<VisualElement>("Container").style.visibility = Visibility.Hidden;
            DontDestroyOnLoad(gameObject);

            loadingLabel = _loadingScreen.rootVisualElement.Q<Label>("loading-label");
            statusLabel = _loadingScreen.rootVisualElement.Q<Label>("state-label");
        }
    
        [Tooltip("The total time to complete the loop of all three dots.")]
        public float loopTime = 0.5f;
    
        float _currentTime = 0f;

        public bool loading = false;

        Label loadingLabel;
        Label statusLabel;
        LoadingState state = LoadingState.Initialize;
    
        public void SetState(LoadingState st)
        {
            this.state = st;
        }

        void FixedUpdate()
        {
            if (_loadingScreen.enabled && loading)
            {
                var nDots = 0;
                _currentTime += Time.fixedDeltaTime;
                if (_currentTime > loopTime) _currentTime = 0f;

                if (_currentTime > 0.66f * loopTime) nDots = 3;
                else if (_currentTime > 0.33f * loopTime) nDots = 2;
                else nDots = 1;
                loadingLabel.text = "Loading" + new string('.', nDots);
                statusLabel.text = typeof(LoadingState).GetMember(state.ToString())[0].GetAttributes<DescriptionAttribute>().First().Description;
            }
        }

        public void StartLoadingScreen()
        {
            loading = true;
            _loadingScreen.rootVisualElement.Q<VisualElement>("Container").style.visibility = Visibility.Visible;
        }

        public void StopLoadingScreen()
        {
            _loadingScreen.rootVisualElement.Q<VisualElement>("Container").style.visibility = Visibility.Hidden;
            _currentTime = 0;
            loading = false;
        }

        public void ConnectionFailed()
        {
            loading = false;
            loadingLabel.text = "Failed to connect";
        }
    }
}