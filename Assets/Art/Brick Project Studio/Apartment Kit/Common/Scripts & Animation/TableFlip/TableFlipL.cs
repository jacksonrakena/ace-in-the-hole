﻿using System.Collections;
using UnityEngine;
namespace AceInTheHole.Art.Brick_Project_Studio.Apartment_Kit.Common.Scripts___Animation.TableFlip
{
	public class TableFlipL: MonoBehaviour {

		public Animator FlipL;
		public bool open;
		public Transform Player;

		void Start (){
			open = false;
		}

		void OnMouseOver (){
			{
				if (Player) {
					float dist = Vector3.Distance (Player.position, transform.position);
					if (dist < 15) {
						if (open == false) {
							if (Input.GetMouseButtonDown (0)) {
								StartCoroutine (opening ());
							}
						} else {
							if (open == true) {
								if (Input.GetMouseButtonDown (0)) {
									StartCoroutine (closing ());
								}
							}

						}

					}
				}

			}

		}

		IEnumerator opening(){
			print ("you are opening the door");
			FlipL.Play ("Lup");
			open = true;
			yield return new WaitForSeconds (.5f);
		}

		IEnumerator closing(){
			print ("you are closing the door");
			FlipL.Play ("Ldown");
			open = false;
			yield return new WaitForSeconds (.5f);
		}


	}
}

