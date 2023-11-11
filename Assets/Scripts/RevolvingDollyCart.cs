using System.Collections;
using System.Collections.Generic;
using AceInTheHole.Scenes.MainMenu;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace AceInTheHole
{
    [RequireComponent(typeof(CinemachineDollyCart))]
    public class RevolvingDollyCart : MonoBehaviour
    {
        CinemachineTrackedDolly dolly;
        public List<CinemachinePathBase> paths = new List<CinemachinePathBase>();
        CinemachineDollyCart cart;
        public CinemachineVirtualCamera targetCamera;

        Transform originalTarget;
        
        int currentPath = 0;
        // Start is called before the first frame update
        void Start()
        {
            dolly = targetCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
            dolly.m_Path = paths[currentPath];
            cart = GetComponent<CinemachineDollyCart>();
            originalTarget = targetCamera.LookAt;
        }

        // Update is called once per frame
        void Update()
        {
            var pathProgress = dolly.m_PathPosition / dolly.m_Path.MaxPos;
            if (pathProgress >= 1.0f)
            {
                currentPath++;
                if (currentPath >= paths.Count) currentPath = 0;
                dolly.m_Path = paths[currentPath];
                cart.m_Path = paths[currentPath];
                cart.m_Position = 0;

                if (dolly.m_Path.gameObject.TryGetComponent(typeof(DollyCartLookOverride), out var c))
                {
                    var dco = (DollyCartLookOverride)c;
                    targetCamera.LookAt = dco.LookAt.transform;
                }
                else targetCamera.LookAt = originalTarget;
            }
        }
    }
}
