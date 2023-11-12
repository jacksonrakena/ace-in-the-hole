using System;
using System.Threading.Tasks;
using AceInTheHole.Client.Loading_Screens;
using AceInTheHole.Network.Promul;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
namespace AceInTheHole.Network
{
    public partial class RelayManager
    {
        public async Task ConnectToRelayAsync(string relayJoinCode)
        {
            // var ls = GameObject.Find("Loading Screen").GetComponent<LoadingScreen>();
            // ls.StartLoadingScreen();
            // try
            // {
            //     ls.SetState(LoadingState.InitializeNet);
            //     await InitialiseUnityServicesAsync();
            //     ls.SetState(LoadingState.WaitForCode);
            //     var allocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            //
            //     ls.SetState(LoadingState.WaitForSync);
            //     var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            //     transport.SetRelayServerData(allocation.RelayServer.IpV4,
            //         (ushort)allocation.RelayServer.Port,
            //         allocation.AllocationIdBytes,
            //         allocation.Key,
            //         allocation.ConnectionData, allocation.HostConnectionData);
            // }
            // catch (Exception e) { Debug.LogException(e); }
            var transport = NetworkManager.Singleton.GetComponent<PromulTransport>();
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = transport;
            StartClient();
        }
        
        public void ConnectToLocal()
        {
            if (NetworkManager.Singleton.StartClient())
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += (r) =>
                {
                    Debug.Log("Transport failure");
                    loadingScreen.GetComponent<LoadingScreen>().ConnectionFailed();
                };
                NetworkManager.Singleton.SceneManager.OnLoadComplete += (id, name, mode) =>
                {
                    GameObject.Find("Loading Screen").GetComponent<LoadingScreen>().StopLoadingScreen();
                };
            }
        }
        
        public void StartClient()
        {
            if (NetworkManager.Singleton.StartClient())
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += (r) =>
                {
                    Debug.Log("Transport failure");
                    loadingScreen.GetComponent<LoadingScreen>().ConnectionFailed();
                };
                NetworkManager.Singleton.SceneManager.OnLoadComplete += (id, name, mode) =>
                {
                    GameObject.Find("Loading Screen").GetComponent<LoadingScreen>().StopLoadingScreen();
                };
            }
        }
    }
}