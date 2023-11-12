using System;
using System.Threading.Tasks;
using AceInTheHole.Client.Loading_Screens;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace AceInTheHole.Network
{
    public partial class RelayManager
    {
        // public async Task InitialiseHostRelayAsync_UR()
        // {
        //     var ls = GameObject.Find("Loading Screen").GetComponent<LoadingScreen>();
        //     ls.StartLoadingScreen();
        //     try
        //     {
        //         ls.SetState(LoadingState.InitializeNet);
        //         await InitialiseUnityServicesAsync();
        //         ls.SetState(LoadingState.WaitForAllocation);
        //         var allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(4);
        //         ls.SetState(LoadingState.WaitForCode);
        //         var joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        //
        //         ls.SetState(LoadingState.WaitForSync);
        //         var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //         Debug.Log("Retrieved join code: " + joinCode);
        //         //PokerTableState.Find().JoinCode = joinCode;
        //         transport.SetRelayServerData(allocation.RelayServer.IpV4,
        //             (ushort)allocation.RelayServer.Port,
        //             allocation.AllocationIdBytes,
        //             allocation.Key,
        //             allocation.ConnectionData);
        //     }
        //     catch (Exception e) { Debug.LogException(e); }
        //
        //     StartHost();
        // }
        
        public async Task InitialiseHostRelayAsync()
        {
            var transport = NetworkManager.Singleton.GetComponent<Promul.Transport.PromulTransport>();
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = transport;
            StartHost();
        }
        
        public void StartHost()
        {
            if (NetworkManager.Singleton.StartHost())
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