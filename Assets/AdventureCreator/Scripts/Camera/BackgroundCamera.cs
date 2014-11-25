/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"BackgroundCamera.cs"
 * 
 *	The BackgroundCamera is used to display background images underneath the scene geometry.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class BackgroundCamera : MonoBehaviour
	{


		private SettingsManager settingsManager;

		private void Awake ()
		{
			GetReferences ();
			TurnOn ();
		}


		private void GetReferences ()
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
		}


		public void TurnOn ()
		{
			if (settingsManager == null)
			{
				GetReferences ();
			}

			if (settingsManager != null)
			{
				if (LayerMask.NameToLayer (settingsManager.backgroundImageLayer) == -1)
				{
					Debug.LogWarning ("No '" + settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
				}
				else
				{
					GetComponent <Camera>().cullingMask = (1 << LayerMask.NameToLayer (settingsManager.backgroundImageLayer));
				}
			}
			else
			{
				Debug.LogWarning ("A Settings Manager is required for this camera type");
			}
		}
		
	}

}