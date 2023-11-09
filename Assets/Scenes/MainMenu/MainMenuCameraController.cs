using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AceInTheHole.Scenes.MainMenu
{
    public class MainMenuCameraController : MonoBehaviour
    {
        class CameraPath
        {
            public Quaternion Rotation;
            public Vector3 Origin;
        
            public Vector3 Destination;
            public float TimeInSeconds;
        }
        static CameraPath Path(Quaternion rotation, Vector3 origin, Vector3 destination, float time)
        {
            return new CameraPath { Rotation = rotation, Origin = origin, Destination = destination, TimeInSeconds = time}; 
        }
    
        List<CameraPath> paths = new List<CameraPath>
        {
            Path(Quaternion.Euler(16.949f, 0f, 0f), new Vector3(0, 2.104f, -6.932f), new Vector3(0, 2.104f, 1.75f), 6),
            Path(Quaternion.Euler(16.949f, 90f, 0), new Vector3(-7.745f, 2.67f, -0.12f), new Vector3(3.48f, 2.67f, -0.12f), 6)
        };

        CameraPath currentPath;
        public float currentTime;

        void Start()
        {   
            DontDestroyOnLoad(gameObject);
            currentPath = paths.First();
            transform.SetPositionAndRotation(currentPath.Origin, currentPath.Rotation);
        }
        void FixedUpdate()
        {     
            if (currentTime >= currentPath.TimeInSeconds)
            {
                currentTime = 0f;
                paths.RemoveAt(0);
                paths.Add(currentPath);
                currentPath = paths.First();
                transform.SetPositionAndRotation(currentPath.Origin, currentPath.Rotation);
            }
            transform.position = Vector3.Lerp(paths.First().Origin, paths.First().Destination, currentTime / currentPath.TimeInSeconds);
            currentTime += Time.fixedDeltaTime;
        }
    }
}
