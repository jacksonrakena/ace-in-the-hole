using System.Threading.Tasks;
using AceInTheHole.Client.Loading_Screens;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
namespace AceInTheHole.Network
{
    public partial class RelayManager : MonoBehaviour
    {
        public LoadingScreen loadingScreen;

        public async Task InitialiseUnityServicesAsync()
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}