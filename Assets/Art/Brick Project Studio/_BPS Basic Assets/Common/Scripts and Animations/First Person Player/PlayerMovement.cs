using UnityEngine;
namespace AceInTheHole.Art.Brick_Project_Studio._BPS_Basic_Assets.Common.Scripts_and_Animations.First_Person_Player

{
    public class PlayerMovement : MonoBehaviour
    {

        public CharacterController controller;

        public float speed = 5f;
        public float gravity = -15f;

        Vector3 velocity;

        bool isGrounded;

        // Update is called once per frame
        void Update()
        {

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            controller.Move(move * speed * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);

        }
    }
}