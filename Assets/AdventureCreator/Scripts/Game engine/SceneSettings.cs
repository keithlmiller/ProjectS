/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneSettings.cs"
 * 
 *	This script defines which cutscenes play when the scene is loaded,
 *	and where the player should begin from.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class SceneSettings : MonoBehaviour
	{
		
		public Cutscene cutsceneOnStart;
		public Cutscene cutsceneOnLoad;
		public Cutscene cutsceneOnVarChange;
		public PlayerStart defaultPlayerStart;
		public AC_NavigationMethod navigationMethod = AC_NavigationMethod.meshCollider;
		public NavigationMesh navMesh;
		public SortingMap sortingMap;
		public Sound defaultSound;
		
		
		private void Awake ()
		{
			// Turn off all NavMesh objects
			NavigationMesh[] navMeshes = FindObjectsOfType (typeof (NavigationMesh)) as NavigationMesh[];
			foreach (NavigationMesh _navMesh in navMeshes)
			{
				if (navMesh != _navMesh)
				{
					_navMesh.TurnOff ();
				}
			}
			
			// Turn on default NavMesh if using MeshCollider method
			if (navMesh && (navMesh.GetComponent <Collider>() || navMesh.GetComponent <Collider2D>()))
			{
				navMesh.TurnOn ();
			}

			KickStarter.mainCamera.FadeIn (0.5f);
		}
		
		
		private void Start ()
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			if (settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
			{
				SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();
				LevelStorage levelStorage = saveSystem.GetComponent <LevelStorage>();
				
				if (levelStorage)
				{
					levelStorage.ReturnCurrentLevelData ();
				}
				
				if (saveSystem.loadingGame == LoadingGame.No)
				{
					FindPlayerStart ();
				}
				else
				{
					saveSystem.loadingGame = LoadingGame.No;
				}
			}
		}
		
		
		public void ResetPlayerReference ()
		{
			if (sortingMap)
			{
				sortingMap.GetAllFollowers ();
			}
		}
		
		
		private void FindPlayerStart ()
		{
			if (GetPlayerStart () != null)
			{
				GetPlayerStart ().SetPlayerStart ();
			}

			StateHandler stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
			bool playedGlobal = stateHandler.PlayGlobalOnStart ();

			if (cutsceneOnStart != null)
			{
				if (!playedGlobal)
				{
					// Place in a temporary cutscene to set everything up
					stateHandler.gameState = GameState.Cutscene;
				}
				Invoke ("RunCutsceneOnStart", 0.01f);
			}
		}


		private void RunCutsceneOnStart ()
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>().gameState = GameState.Normal;
			cutsceneOnStart.Interact ();
		}
		
		
		public PlayerStart GetPlayerStart ()
		{
			SceneChanger sceneChanger = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>();
			
			PlayerStart[] starters = FindObjectsOfType (typeof (PlayerStart)) as PlayerStart[];
			foreach (PlayerStart starter in starters)
			{
				if (starter.previousScene > -1 && starter.previousScene == sceneChanger.previousScene)
				{
					return starter;
				}
			}
			
			if (defaultPlayerStart)
			{
				return defaultPlayerStart;
			}
			
			return null;
		}
		
		
		public void OnLoad ()
		{
			if (cutsceneOnLoad != null)
			{
				cutsceneOnLoad.Interact ();
			}
		}
		
		
		public void PlayDefaultSound (AudioClip audioClip, bool doLoop)
		{
			if (defaultSound == null)
			{
				Debug.Log ("Cannot play sound since no Default Sound Prefab is defined - please set one in the Scene Manager.");
				return;
			}
			
			if (audioClip && defaultSound.GetComponent <AudioSource>())
			{
				defaultSound.GetComponent <AudioSource>().clip = audioClip;
				defaultSound.Play (doLoop);
			}
		}


		public void PauseGame ()
		{
			// Work out which Sounds will have to be re-played after pausing
			Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			List<Sound> soundsToResume = new List<Sound>();
			foreach (Sound sound in sounds)
			{
				if (sound.playWhilePaused && sound.IsPlaying ())
				{
					soundsToResume.Add (sound);
				}
			}

			Time.timeScale = 0f;
			AudioListener.pause = true;

			foreach (Sound sound in soundsToResume)
			{
				sound.Play ();
			}
		}
		
	}
	
}
