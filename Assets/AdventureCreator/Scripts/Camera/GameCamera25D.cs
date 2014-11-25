/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"GameCamera25D.cs"
 * 
 *	This GameCamera is fixed, but allows for a background image to be displayed.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class GameCamera25D : _Camera
	{
		
		public BackgroundImage backgroundImage;
		
		
		public void SetActiveBackground ()
		{
			if (backgroundImage)
			{
				// Move background images onto correct layer
				BackgroundImage[] backgroundImages = FindObjectsOfType (typeof (BackgroundImage)) as BackgroundImage[];
				foreach (BackgroundImage image in backgroundImages)
				{
					if (image == backgroundImage)
					{
						image.TurnOn ();
					}
					else
					{
						image.TurnOff ();
					}
				}
				
				// Turn BackgroundCamera on
				if (GameObject.FindWithTag (Tags.backgroundCamera) && GameObject.FindWithTag (Tags.backgroundCamera).GetComponent <BackgroundCamera>())
				{
					BackgroundCamera backgroundCamera = GameObject.FindWithTag (Tags.backgroundCamera).GetComponent <BackgroundCamera>();
					backgroundCamera.TurnOn ();
				}
				else
				{
					Debug.LogWarning ("No BackgroundCamera found - please use the Scene Manager to Organise Room Objects with 2.5D Camera Projection.");
				}
				
				// Set MainCamera's Clear Flags
				KickStarter.mainCamera.PrepareForBackground ();
			}
		}


		new public void ResetTarget ()
		{}


		public void SnapCameraInEditor ()
		{
			if (KickStarter.mainCamera)
			{
				MainCamera mainCamera = KickStarter.mainCamera;

				mainCamera.transform.parent = this.transform;
				mainCamera.transform.localPosition = Vector3.zero;
				mainCamera.transform.localEulerAngles = Vector3.zero;
				mainCamera.GetComponent <Camera>().isOrthoGraphic = this.GetComponent <Camera>().isOrthoGraphic;
				mainCamera.GetComponent <Camera>().fieldOfView = this.GetComponent <Camera>().fieldOfView;
				mainCamera.GetComponent <Camera>().farClipPlane = this.GetComponent <Camera>().farClipPlane;
				mainCamera.GetComponent <Camera>().nearClipPlane = this.GetComponent <Camera>().nearClipPlane;
				mainCamera.GetComponent <Camera>().orthographicSize = this.GetComponent <Camera>().orthographicSize;
			}
		}
		
	}
		
}