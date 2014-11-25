/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneChanger.cs"
 * 
 *	This script handles the changing of the scene, and stores
 *	which scene was previously loaded, for use by PlayerStart.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class SceneChanger : MonoBehaviour
	{

		public int previousScene = -1;

		private SettingsManager settingsManager;


		private void Awake ()
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
		}
		
		
		public void ChangeScene (int sceneNumber, bool saveRoomData)
		{
			bool useLoadingScreen = false;
			if (settingsManager != null && settingsManager.useLoadingScreen)
			{
				useLoadingScreen = true;
			}

			KickStarter.mainCamera.FadeOut (0f);

			if (KickStarter.player)
			{
				KickStarter.player.Halt ();
			}

			Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			foreach (Sound sound in sounds)
			{
				if (sound.canDestroy)
				{
					Destroy (sound);
				}
			}

			Dialog dialog = GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>();
			dialog.KillDialog (true);

			LevelStorage levelStorage = this.GetComponent <LevelStorage>();
			if (saveRoomData)
			{
				levelStorage.StoreCurrentLevelData ();
				previousScene = Application.loadedLevel;
			}
			
			StateHandler stateHandler = this.GetComponent <StateHandler>();
			stateHandler.gameState = GameState.Normal;
			
			LoadLevel (sceneNumber, useLoadingScreen);
		}


		private void LoadLevel (int sceneNumber)
		{
			if (settingsManager != null && settingsManager.useLoadingScreen)
			{
				LoadLevel (sceneNumber, true);
			}
			else
			{
				LoadLevel (sceneNumber, false);
			}
		}


		private void LoadLevel (int sceneNumber, bool useLoadingScreen)
		{
			if (useLoadingScreen)
			{
				GameObject go = new GameObject ("LevelManager");
				LoadingScreen loadingScreen = go.AddComponent <LoadingScreen>();
				loadingScreen.StartCoroutine (loadingScreen.InnerLoad (sceneNumber, settingsManager.loadingScene));
			}
			else
			{
				Application.LoadLevel (sceneNumber);
			}
		}

	}

}