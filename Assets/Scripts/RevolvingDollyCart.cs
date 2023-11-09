using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace AceInTheHole
{
    [RequireComponent(typeof(CinemachineDollyCart))]
    public class RevolvingDollyCart : MonoBehaviour
    {
        CinemachineTrackedDolly dolly;
        public List<CinemachinePathBase> paths = new List<CinemachinePathBase>();
        CinemachineDollyCart cart;
        public CinemachineVirtualCamera camera;
        int currentPath = 0;
        // Start is called before the first frame update
        void Start()
        {
            dolly = camera.GetCinemachineComponent<CinemachineTrackedDolly>();
            dolly.m_Path = paths[currentPath];
            cart = GetComponent<CinemachineDollyCart>();
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
            }
        }
    }
}
