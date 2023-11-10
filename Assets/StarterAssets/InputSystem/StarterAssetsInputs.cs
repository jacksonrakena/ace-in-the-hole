using UnityEngine;
using UnityEngine.InputSystem;

namespace AceInTheHole.StarterAssets.InputSystem
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;
		

		public void OnMove(InputValue value)
		{
			move = value.Get<Vector2>();
		}

		public void OnLook(InputValue value)
		{
			look = value.Get<Vector2>();
			
		}

		public void OnJump(InputValue value)
		{
			jump = value.isPressed;
		}

		public void OnSprint(InputValue value)
		{
			sprint = value.isPressed;
		}
	}
}