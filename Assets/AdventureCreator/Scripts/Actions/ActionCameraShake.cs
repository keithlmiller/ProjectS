/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCameraShake.cs"
 * 
 *	This action causes the MainCamera to shake,
 *	and also affects the BackgroundImage if one is active.
 * 
 */

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionCameraShake : Action
	{
		
		public int shakeIntensity;
		
		
		public ActionCameraShake ()
		{
			this.isDisplayed = true;
			title = "Camera: Shake";
		}
		
		
		override public float Run ()
		{
			MainCamera mainCam = KickStarter.mainCamera;
			
			if (mainCam)
			{
				if (!isRunning)
				{
					isRunning = true;
					
					if (mainCam.attachedCamera is GameCamera)
					{
						mainCam.Shake ((float) shakeIntensity / 10000f, true);
					}
					
					else if (mainCam.attachedCamera is GameCamera25D)
					{
						mainCam.Shake ((float) shakeIntensity / 10000f, true);
						
						GameCamera25D gameCamera = (GameCamera25D) mainCam.attachedCamera;
						if (gameCamera.backgroundImage)
						{
							gameCamera.backgroundImage.Shake (shakeIntensity / 100f);
						}
					}
					
					else if (mainCam.attachedCamera is GameCamera2D)
					{
						mainCam.Shake ((float) shakeIntensity / 5000f, false);
					}
					
					else
					{
						mainCam.Shake ((float) shakeIntensity / 10000f, false);
					}
						
					if (willWait)
					{
						return (defaultPauseTime);
					}
				}
				else
				{
					if (!mainCam.IsShaking ())
					{
						isRunning = false;
						return 0f;
					}
					else
					{
						return (defaultPauseTime);
					}
				}
			}
			
			return 0f;
		}


		override public void Skip ()
		{
			return;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			shakeIntensity = EditorGUILayout.IntSlider ("Intensity:", shakeIntensity, 1, 10);
			willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			return "";
		}

		#endif
		
	}

}