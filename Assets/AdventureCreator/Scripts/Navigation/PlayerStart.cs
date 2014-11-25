/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerStart.cs"
 * 
 *	This script defines a possible starting position for the
 *	player when the scene loads, based on what the previous
 *	scene was.  If no appropriate PlayerStart is found, the
 *	one define in StartSettings is used as the default.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class PlayerStart : Marker
	{
		
		public bool fadeInOnStart;
		public float fadeSpeed = 0.5f;
		public int previousScene;
		public _Camera cameraOnStart;
		
		private GameObject playerOb;


		public void SetPlayerStart ()
		{
			if (KickStarter.mainCamera)
			{
				if (fadeInOnStart)
				{
					KickStarter.mainCamera.FadeIn (fadeSpeed);
				}
				
				if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
				{
					SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

					if (KickStarter.player)
					{
						playerOb = KickStarter.player.gameObject;
						playerOb.GetComponent <Player>().Teleport (this.transform.position);
						playerOb.GetComponent <Player>().SetLookDirection (this.transform.forward, true);

						if (settingsManager.ActInScreenSpace () && !settingsManager.IsUnity2D ())
						{
							playerOb.transform.position = AdvGame.GetScreenNavMesh (playerOb.transform.position);
						}
					}
				
					if (settingsManager.movementMethod == MovementMethod.FirstPerson || settingsManager.movementMethod == MovementMethod.UltimateFPS)
					{
						KickStarter.mainCamera.SetFirstPerson ();
					}
					
					else if (cameraOnStart)
					{
						KickStarter.mainCamera.SetGameCamera (cameraOnStart);
						KickStarter.mainCamera.lastNavCamera = cameraOnStart;
						cameraOnStart.MoveCameraInstant ();
						KickStarter.mainCamera.SetGameCamera (cameraOnStart);
						KickStarter.mainCamera.SnapToAttached ();
					}
					
					else if (cameraOnStart == null)
					{
						Debug.LogWarning (this.name + " has no Camera On Start");
					}
				}
			}
		}
		
	}

}