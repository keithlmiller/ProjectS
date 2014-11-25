﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"GameCamera2D.cs"
 * 
 *	This GameCamera allows scrolling horizontally and vertically without altering perspective.
 *	Based on the work by Eric Haines (Eric5h5) at http://wiki.unity3d.com/index.php?title=OffsetVanishingPoint
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class GameCamera2D : _Camera
	{
		
		public bool lockHorizontal = true;
		public bool lockVertical = true;

		public bool limitHorizontal;
		public bool limitVertical;

		public Vector2 constrainHorizontal;
		public Vector2 constrainVertical;
		
		public Vector2 freedom = Vector2.zero;
		public float dampSpeed = 0.9f;

		public Vector2 afterOffset = Vector2.zero;
		
		public Vector2 perspectiveOffset = Vector2.zero;
		private Vector2 originalPosition = Vector2.zero;
		private Vector2 desiredOffset = Vector2.zero;
		private SettingsManager settingsManager;

		
		protected override void Awake ()
		{
			originalPosition = new Vector2 (transform.position.x, transform.position.y);
			base.Awake ();
			settingsManager = AdvGame.GetReferences ().settingsManager;
		}
		
		
		private void Start ()
		{
			ResetTarget ();
			
			if (target)
			{
				MoveCameraInstant ();
			}

			StartCoroutine ("_Update");
		}


		private IEnumerator _Update ()
		{
			while (Application.isPlaying)
			{
				MoveCamera ();
				yield return new WaitForFixedUpdate ();
			}
		}


		public void SwitchTarget (Transform _target)
		{
			target = _target;
		}
				

		private void SetDesired ()
		{
			Vector2 targetOffset = GetOffsetForPosition (target.transform.position);
			
			if (targetOffset.x < (perspectiveOffset.x - freedom.x))
			{
				desiredOffset.x = targetOffset.x + freedom.x;
			}
			else if (targetOffset.x > (perspectiveOffset.x + freedom.x))
			{
				desiredOffset.x = targetOffset.x - freedom.x;
			}

			desiredOffset.x += afterOffset.x;
			if (limitHorizontal)
			{
				desiredOffset.x = ConstrainAxis (desiredOffset.x, constrainHorizontal);
			}
			
			if (targetOffset.y < (perspectiveOffset.y - freedom.y))
			{
				desiredOffset.y = targetOffset.y + freedom.y;
			}
			else if (targetOffset.y > (perspectiveOffset.y + freedom.y))
			{
				desiredOffset.y = targetOffset.y - freedom.y;
			}
			
			desiredOffset.y += afterOffset.y;
			if (limitVertical)
			{
				desiredOffset.y = ConstrainAxis (desiredOffset.y, constrainVertical);
			}
		}	
		
		
		private void MoveCamera ()
		{
			if (targetIsPlayer && KickStarter.player)
			{
				target = KickStarter.player.transform;
			}
			
			if (target && (!lockHorizontal || !lockVertical))
			{
				SetDesired ();
			
				if (!lockHorizontal)
				{
					perspectiveOffset.x = Mathf.Lerp (perspectiveOffset.x, desiredOffset.x, Time.deltaTime * dampSpeed);
				}
				
				if (!lockVertical)
				{
					perspectiveOffset.y = Mathf.Lerp (perspectiveOffset.y, desiredOffset.y, Time.deltaTime * dampSpeed);
				}
			}
			
			SetProjection ();
		}
		
		
		public override void MoveCameraInstant ()
		{
			if (targetIsPlayer && KickStarter.player)
			{
				target = KickStarter.player.transform;
			}
			
			if (target && (!lockHorizontal || !lockVertical))
			{
				SetDesired ();
			
				if (!lockHorizontal)
				{
					perspectiveOffset.x = desiredOffset.x;
				}
				
				if (!lockVertical)
				{
					perspectiveOffset.y = desiredOffset.y;
				}
			}
			
			SetProjection ();
		}


		private void SetProjection ()
		{
			if (!_camera.orthographic)
			{
				_camera.projectionMatrix = AdvGame.SetVanishingPoint (_camera, perspectiveOffset);
			}
			else
			{
				transform.position = new Vector3 (originalPosition.x + perspectiveOffset.x, originalPosition.y + perspectiveOffset.y, transform.position.z);
			}
		}


		public void SnapToOffset ()
		{
			perspectiveOffset = afterOffset;
			SetProjection ();
		}
		
		
		public IEnumerator ResetProjection ()
		{
			transform.position = new Vector3 (originalPosition.x, originalPosition.y, transform.position.z);

			yield return new WaitForFixedUpdate ();
			_camera.ResetProjectionMatrix ();
		}


		private Vector2 GetOffsetForPosition (Vector3 targetPosition)
		{
			Vector2 targetOffset = new Vector2 ();
			float forwardOffsetScale = 93 - (299 * _camera.nearClipPlane);

			if (settingsManager && settingsManager.IsTopDown ())
			{
				if (_camera.orthographic)
				{
					targetOffset.x = transform.position.x;
					targetOffset.y = transform.position.z;
				}
				else
				{
					targetOffset.x = - (targetPosition.x - transform.position.x) / (forwardOffsetScale * (targetPosition.y - transform.position.y));
					targetOffset.y = - (targetPosition.z - transform.position.z) / (forwardOffsetScale * (targetPosition.y - transform.position.y));
				}
			}
			else
			{
				if (_camera.orthographic)
				{
					targetOffset.x = targetPosition.x;
					targetOffset.y = targetPosition.y;
				}
				else
				{
					targetOffset.x = (targetPosition.x - transform.position.x) / (forwardOffsetScale * (targetPosition.z - transform.position.z));
					targetOffset.y = (targetPosition.y - transform.position.y) / (forwardOffsetScale * (targetPosition.z - transform.position.z));
				}
			}

			return targetOffset;
		}


		public void SetCorrectRotation ()
		{
			if (settingsManager == null)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}

			if (_camera == null)
			{
				_camera = GetComponent <Camera>();
			}

			if (settingsManager != null)
			{
				if (settingsManager.IsTopDown ())
				{
					transform.rotation = Quaternion.Euler (90f, 0, 0);
					return;
				}

				if (settingsManager.IsUnity2D ())
				{
					_camera.orthographic = true;
				}
			}

			transform.rotation = Quaternion.Euler (0, 0, 0);
		}


		public bool IsCorrectRotation ()
		{
			if (settingsManager == null)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}

			if (settingsManager != null && settingsManager.IsTopDown ())
			{
				if (transform.rotation == Quaternion.Euler (90f, 0, 0))
				{
					return true;
				}

				return false;
			}

			if (transform.rotation == Quaternion.Euler (0, 0, 0))
			{
				return true;
			}

			return false;
		}


		private void OnDestroy ()
		{
			settingsManager = null;
		}
		
	}

}