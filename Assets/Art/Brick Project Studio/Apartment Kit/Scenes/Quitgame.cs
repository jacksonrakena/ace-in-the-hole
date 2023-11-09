using UnityEngine;
namespace AceInTheHole.Art.Brick_Project_Studio.Apartment_Kit.Scenes
{
    public class Quitgame : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}