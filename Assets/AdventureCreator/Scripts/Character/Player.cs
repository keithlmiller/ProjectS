/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Player.cs"
 * 
 *	This is attached to the Player GameObject, which must be tagged as Player.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class Player : Char
	{

		// Legacy variables
		public AnimationClip jumpAnim;

		// Mecanim variables
		public string jumpParameter = "Jump";

		public int ID;
		public bool lockedPath;
		public DetectHotspots hotspotDetector;

		private bool isTilting = false;
		private float actualTilt;
		private float targetTilt;
		private float tiltSpeed;
		private float tiltStartTime;

		private Transform fpCam;
		private PlayerInput playerInput;


		public void Awake ()
		{
			DontDestroyOnLoad (this);
			GetReferences ();
			
			if (soundChild && soundChild.gameObject.GetComponent <AudioSource>())
			{
				audioSource = soundChild.gameObject.GetComponent <AudioSource>();
			}

			if (GameObject.FindWithTag (Tags.firstPersonCamera))
			{
				fpCam = GameObject.FindWithTag (Tags.firstPersonCamera).transform;
			}
			
			_Awake ();
		}


		private void GetReferences ()
		{
			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
			{
				playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
			}
		}


		private void OnLevelWasLoaded ()
		{
			GetReferences ();
		}


		private void Update ()
		{
			if (stateHandler && stateHandler.playerIsOff)
			{
				return;
			}

			if (hotspotDetector)
			{
				hotspotDetector._Update ();
			}

			_Update ();
		}
		
		
		private void FixedUpdate ()
		{
			if (settingsManager == null)
			{
				GetSettingsManager ();
			}

			if (stateHandler && stateHandler.playerIsOff)
			{
				return;
			}

			if (activePath && !pausePath)
			{
				if (IsTurningBeforeWalking ())
				{
					charState = CharState.Idle;
				}
				else if ((stateHandler && stateHandler.gameState == GameState.Cutscene) || (settingsManager && settingsManager.movementMethod == MovementMethod.PointAndClick) || (settingsManager && settingsManager.movementMethod == MovementMethod.StraightToCursor && settingsManager.singleTapStraight) || IsMovingToHotspot ())
				{
					charState = CharState.Move;
				}
				
				if (!lockedPath)
				{
					CheckIfStuck ();
				}
			}
			else if (activePath == null && stateHandler.gameState == GameState.Cutscene && charState == CharState.Move)
			{
				charState = CharState.Decelerate;
			}

			if (isJumping)
			{
				if (IsGrounded ())
				{
					isJumping = false;
				}
			}

			if (isTilting)
			{
				actualTilt = Mathf.Lerp (actualTilt, targetTilt, AdvGame.Interpolate (tiltStartTime, tiltSpeed, MoveMethod.Smooth));
				if (Mathf.Abs (targetTilt - actualTilt) < 2f)
				{
					isTilting = false;
				}
			}

			_FixedUpdate ();

			if (IsUFPSPlayer () && isTurning)
			{
				if (isTilting)
				{
					UltimateFPSIntegration.SetRotation (new Vector2 (actualTilt, newRotation.eulerAngles.y));
				}
				else
				{
					UltimateFPSIntegration.SetPitch (newRotation.eulerAngles.y);
				}
			}
			else if (isTilting)
			{
				UpdateTilt ();
			}

			if (IsUFPSPlayer () && activePath != null && charState == CharState.Move)
			{
				UltimateFPSIntegration.Teleport (transform.position);
			}
		}


		private bool IsGrounded ()
		{
			if (_rigidbody != null && Mathf.Abs (_rigidbody.velocity.y) > 0.1f)
			{
				return false;
			}

			if (_collider != null)
			{
				return Physics.CheckCapsule (transform.position + new Vector3 (0f, _collider.bounds.size.y, 0f), transform.position + new Vector3 (0f, _collider.bounds.size.x / 4f, 0f), _collider.bounds.size.x / 2f);
			}

			Debug.Log ("Player has no Collider component");
			return false;
		}


		public void Jump ()
		{
			if (isJumping)
			{
				return;
			}

			if (IsGrounded () && activePath == null)
			{
				if (_rigidbody != null)
				{
					_rigidbody.velocity = new Vector3 (0f, settingsManager.jumpSpeed, 0f);
					isJumping = true;
				}
				else
				{
					Debug.Log ("Player cannot jump as he has no Rigidbody component");
				}
			}
		}


		private bool IsMovingToHotspot ()
		{
			if (playerInput != null && playerInput.hotspotMovingTo != null)
			{
				return true;
			}

			return false;
		}
		
		
		new public void EndPath ()
		{
			lockedPath = false;
			
			base.EndPath ();
		}


		public void SetLockedPath (Paths pathOb)
		{
			// Ignore if using "point and click" or first person methods
			if (settingsManager)
			{
				if (settingsManager.movementMethod == MovementMethod.Direct && settingsManager.inputMethod != InputMethod.TouchScreen)
				{
					lockedPath = true;
					
					if (pathOb.pathSpeed == PathSpeed.Run)
					{
						isRunning = true;
					}
					else
					{
						isRunning = false;
					}
				
					if (pathOb.affectY)
					{
						transform.position = pathOb.transform.position;
					}
					else
					{
						transform.position = new Vector3 (pathOb.transform.position.x, transform.position.y, pathOb.transform.position.z);
					}
			
					activePath = pathOb;
					targetNode = 1;
					charState = CharState.Idle;
				}
				else
				{
					Debug.LogWarning ("Path-constrained player movement is only available with Direct control for Point And Click and Controller input only.");
				}
			}
		}

	
		protected override void Accelerate ()
		{
			if (settingsManager.movementMethod == MovementMethod.UltimateFPS && activePath == null)
			{
				// Fixes "stuttering" effect
				moveSpeed = 0f;
				return;
			}

			base.Accelerate ();
		}


		private void UpdateTilt ()
		{
			if (fpCam && fpCam.GetComponent <FirstPersonCamera>())
			{
				fpCam.GetComponent <FirstPersonCamera>().SetRotationY (actualTilt);
			}
			else if (settingsManager.movementMethod == MovementMethod.UltimateFPS)
			{
				UltimateFPSIntegration.SetTilt (actualTilt);
			}
		}
		
		
		public void SetTilt (Vector3 lookAtPosition, bool isInstant)
		{
			if (fpCam == null)
			{
				return;
			}

			if (isInstant)
			{
				isTilting = false;
				
				transform.LookAt (lookAtPosition);
				float tilt = transform.localEulerAngles.x;
				if (targetTilt > 180)
				{
					targetTilt = targetTilt - 360;
				}

				if (fpCam.GetComponent <FirstPersonCamera>())
				{
					fpCam.GetComponent <FirstPersonCamera>().SetRotationY (tilt);
				}
				else if (settingsManager.movementMethod == MovementMethod.UltimateFPS)
				{
					UltimateFPSIntegration.SetTilt (tilt);
				}
			}
			else
			{
				// Base the speed of tilt change on how much horizontal rotation is needed

				actualTilt = fpCam.eulerAngles.x;
				if (actualTilt > 180)
				{
					actualTilt = actualTilt - 360;
				}

				Quaternion oldRotation = fpCam.rotation;
				fpCam.transform.LookAt (lookAtPosition);
				targetTilt = fpCam.localEulerAngles.x;
				fpCam.rotation = oldRotation;
				if (targetTilt > 180)
				{
					targetTilt = targetTilt - 360;
				}

				Vector3 flatLookVector = lookAtPosition - transform.position;
				flatLookVector.y = 0f;
				
				tiltSpeed = Mathf.Abs (2f / Vector3.Dot (fpCam.forward.normalized, flatLookVector.normalized)) * turnSpeed / 100f;
				tiltSpeed = Mathf.Min (tiltSpeed, 2f);
				tiltStartTime = Time.time;
				isTilting = true;
			}
		}

	}


	[System.Serializable]
	public class PlayerPrefab
	{
		public Player playerOb;
		public int ID;
		public bool isDefault;

		public PlayerPrefab (int[] idArray)
		{
			ID = 0;
			playerOb = null;

			if (idArray.Length > 0)
			{
				isDefault = false;

				foreach (int _id in idArray)
				{
					if (ID == _id)
						ID ++;
				}
			}
			else
			{
				isDefault = true;
			}
		}
	}

}