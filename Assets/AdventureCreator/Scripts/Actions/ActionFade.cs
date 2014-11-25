/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionFade.cs"
 * 
 *	This action controls the MainCamera's fading.
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
	public class ActionFade : Action
	{
		
		public FadeType fadeType;
		public bool isInstant;
		public float fadeSpeed = 0.5f;
		
		
		public ActionFade ()
		{
			this.isDisplayed = true;
			title = "Camera: Fade";
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				
				MainCamera mainCam = KickStarter.mainCamera;
				RunSelf (mainCam, fadeSpeed);
					
				if (willWait && !isInstant)
				{
					return (fadeSpeed);
				}

				return 0f;
			}

			else
			{
				isRunning = false;
				return 0f;
			}
		}


		override public void Skip ()
		{
			RunSelf (KickStarter.mainCamera, 0f);
		}


		private void RunSelf (MainCamera mainCam, float _time)
		{
			if (mainCam == null)
			{
				return;
			}

			mainCam.StopCrossfade ();

			if (fadeType == FadeType.fadeIn)
			{
				if (isInstant)
				{
					mainCam.FadeIn (0f);
				}
				else
				{
					mainCam.FadeIn (_time);
				}
			}
			else
			{
				if (isInstant)
				{
					mainCam.FadeOut (0f);
				}
				else
				{
					mainCam.FadeOut (_time);
				}
			}
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			fadeType = (FadeType) EditorGUILayout.EnumPopup ("Type:", fadeType);
			
			isInstant = EditorGUILayout.Toggle ("Instant?", isInstant);
			if (!isInstant)
			{
				fadeSpeed = EditorGUILayout.Slider ("Time to fade:", fadeSpeed, 0, 3);
				willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
			}

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (fadeType == FadeType.fadeIn)
			{
				labelAdd = " (In)";
			}
			else
			{
				labelAdd = " (Out)";
			}
			
			return labelAdd;
		}

		#endif

	}

}