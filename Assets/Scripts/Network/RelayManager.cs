using System.Threading.Tasks;
using AceInTheHole.Client.Loading_Screens;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
namespace AceInTheHole.Network
{
    public partial class RelayManager : MonoBehaviour
    {
        public LoadingScreen loadingScreen;

        public bool UseUnityRelay => false;
        
        public async Task InitialiseUnityServicesAsync()
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public void StartServer()
        {
            if (NetworkManager.Singleton.StartServer())
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += (id, mode, s, a) =>
                {
                    loadingScreen.StopLoadingScreen();
                };
                var status = NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            }
        }
    }
}