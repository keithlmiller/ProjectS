/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"UltimateFPSIntegration.cs"
 * 
 *	This script contains a number of static functions for use
 *	in integrating AC with the Ultimate FPS asset
 *
 *	To allow for 2DToolkit integration, the 'UltimateFPSIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'UltimateFPSIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 * 
 */


using UnityEngine;
using System.Collections;


namespace AC
{
	
	public class UltimateFPSIntegration : ScriptableObject
	{
		
		public static bool IsDefinePresent ()
		{
			#if UltimateFPSIsPresent
			return true;
			#else
			return false;
			#endif
		}


		public static void Update (GameState gameState)
		{
			if (gameState == GameState.Normal)
			{
				UltimateFPSIntegration.SetMovementState (true);
				UltimateFPSIntegration.SetCameraState (true);
			}
			else
			{
				UltimateFPSIntegration.SetMovementState (false);
				UltimateFPSIntegration.SetCameraState (false);
			}
		}


		public static void SetMovementState (bool state)
		{
			#if UltimateFPSIsPresent
			vp_FPController Controller = KickStarter.player.GetComponent <vp_FPController>();

			if (state && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>().isUpLocked)
			{
				Controller.SetState ("Freeze", true);
			}
			else
			{
				Controller.SetState ("Freeze", !state);
			}

			if (!state)
			{
				Controller.Stop();
			}
	
			#else
			Debug.Log ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void SetCameraState (bool state)
		{
			#if UltimateFPSIsPresent
			vp_FPCamera _camera = GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <vp_FPCamera>();
			if (state && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>().IsCursorLocked () == false)
			{
				_camera.SetState ("Freeze", true);
			}
			else
			{
				_camera.SetState ("Freeze", !state);
			}
			#else
			Debug.Log ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void Teleport (Vector3 position)
		{
			#if UltimateFPSIsPresent
			KickStarter.player.GetComponent <vp_FPController>().SetPosition (position);
			#endif
		}


		public static void SetRotation (Vector3 rotation)
		{
			#if UltimateFPSIsPresent
			GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <vp_FPCamera>().SetRotation (new Vector2 (rotation.x, rotation.y), true, true);
			#endif
		}


		public static void SetPitch (float pitch)
		{
			#if UltimateFPSIsPresent
			Transform camTransform = GameObject.FindWithTag (Tags.firstPersonCamera).transform;
			camTransform.GetComponent <vp_FPCamera>().Angle = new Vector2 (camTransform.eulerAngles.x, pitch);
			#endif
		}


		public static void SetTilt (float tilt)
		{
			#if UltimateFPSIsPresent
			Transform camTransform = GameObject.FindWithTag (Tags.firstPersonCamera).transform;
			camTransform.GetComponent <vp_FPCamera>().Angle = new Vector2 (tilt, camTransform.eulerAngles.y);
			#endif
		}

	}
	
}