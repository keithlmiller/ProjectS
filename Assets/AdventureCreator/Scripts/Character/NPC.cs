/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NPC.cs"
 * 
 *	This is attached to all non-Player characters.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class NPC : Char
	{

		public Char followTarget = null;
		public bool followTargetIsPlayer = false;
		public float followFrequency = 0f;
		public float followDistance = 0f;
		public float followDistanceMax = 0f;

		LayerMask LayerOn;
		LayerMask LayerOff;
		
		
		private void Awake ()
		{
			GetSettingsManager ();
			LayerOn = LayerMask.NameToLayer (settingsManager.hotspotLayer);
			LayerOff = LayerMask.NameToLayer (settingsManager.deactivatedLayer);

			_Awake ();
		}

		
		private void FixedUpdate ()
		{

			if (activePath && followTarget)
			{
				FollowCheckDistance ();
				FollowCheckDistanceMax ();
			}

			if (activePath && !pausePath)
			{
				if (IsTurningBeforeWalking ())
				{
					charState = CharState.Idle;
				}
				else 
				{
					charState = CharState.Move;
					CheckIfStuck ();
				}
			}

			_FixedUpdate ();
		}


		private void Update ()
		{
			_Update ();
		}


		public void FollowReset ()
		{
			FollowStop ();

			followTarget = null;
			followTargetIsPlayer = false;
			followFrequency = 0f;
			followDistance = 0f;
		}


		private void FollowUpdate ()
		{
			if (followTarget)
			{
				FollowMove ();
				Invoke ("FollowUpdate", followFrequency);
			}
		}


		private void FollowMove ()
		{
			float dist = FollowCheckDistance ();
			if (dist > followDistance)
			{
				Paths path = GetComponent <Paths>();
				if (path == null)
				{
					Debug.LogWarning ("Cannot move a character with no Paths component");
				}
				else
				{
					path.pathType = AC_PathType.ForwardOnly;
					path.affectY = true;
					
					Vector3[] pointArray;
					Vector3 targetPosition = followTarget.transform.position;
					
					if (settingsManager && settingsManager.ActInScreenSpace ())
					{
						targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
					}
					
					if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <NavigationManager>())
					{
						pointArray = GameObject.FindWithTag (Tags.gameEngine).GetComponent <NavigationManager>().navigationEngine.GetPointsArray (transform.position, targetPosition);
					}
					else
					{
						List<Vector3> pointList = new List<Vector3>();
						pointList.Add (targetPosition);
						pointArray = pointList.ToArray ();
					}

					if (dist > followDistanceMax)
					{
						MoveAlongPoints (pointArray, true);
					}
					else
					{
						MoveAlongPoints (pointArray, false);
					}
				}
			}
		}


		private float FollowCheckDistance ()
		{
			float dist = Vector3.Distance (followTarget.transform.position, transform.position);

			if (dist < followDistance)
			{
				EndPath ();
			}

			return (dist);
		}


		private void FollowCheckDistanceMax ()
		{
			if (followTarget)
			{
				if (FollowCheckDistance () > followDistanceMax)
				{
					if (!isRunning)
					{
						FollowMove ();
					}
				}
				else if (isRunning)
				{
					FollowMove ();
				}
			}
		}


		private void FollowStop ()
		{
			StopCoroutine ("FollowUpdate");

			if (followTarget != null)
			{
				EndPath ();
			}
		}


		public void FollowAssign (Char _followTarget, bool _followTargetIsPlayer, float _followFrequency, float _followDistance, float _followDistanceMax)
		{
			if (_followTargetIsPlayer)
			{
				_followTarget = KickStarter.player;
			}

			if (_followTarget == null || _followFrequency == 0f || _followFrequency < 0f || _followDistance <= 0f || _followDistanceMax <= 0f)
			{
				FollowReset ();
				return;
			}

			followTarget = _followTarget;
			followTargetIsPlayer = _followTargetIsPlayer;
			followFrequency = _followFrequency;
			followDistance = _followDistance;
			followDistanceMax = _followDistanceMax;

			FollowUpdate ();
		}
		
		
		private void TurnOn ()
		{
			gameObject.layer = LayerOn;
		}
		

		private void TurnOff ()
		{
			gameObject.layer = LayerOff;
		}
		
	}

}