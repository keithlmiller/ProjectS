﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionPlayerSwitch.cs"
 * 
 *	This action causes a different Player prefab
 *	to be controlled.  Note that only one Player prefab
 *  can exist in a scene at any one time - for two player
 *  "characters" to be present, one must be a swapped-out
 * 	NPC instead.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionPlayerSwitch : Action
	{
		
		public int playerID;
		public int playerNumber;

		public NewPlayerPosition newPlayerPosition = NewPlayerPosition.ReplaceNPC;
		public OldPlayer oldPlayer = OldPlayer.RemoveFromScene;

		public bool restorePreviousData = false;

		public int newPlayerScene;

		public int oldPlayerNPC_ID;
		public NPC oldPlayerNPC;

		public int newPlayerNPC_ID;
		public NPC newPlayerNPC;

		public int newPlayerMarker_ID;
		public Marker newPlayerMarker;

		private SettingsManager settingsManager;


		public ActionPlayerSwitch ()
		{
			this.isDisplayed = true;
			title = "Player: Switch";
		}


		override public void AssignValues ()
		{
			oldPlayerNPC = AssignFile <NPC> (oldPlayerNPC_ID, oldPlayerNPC);
			newPlayerNPC = AssignFile <NPC> (newPlayerNPC_ID, newPlayerNPC);
			newPlayerMarker = AssignFile <Marker> (newPlayerMarker_ID, newPlayerMarker);
		}
		
		
		override public float Run ()
		{
			if (!settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}

			if (settingsManager && settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				if (settingsManager.players.Count > 0 && settingsManager.players.Count > playerNumber && playerNumber > -1)
				{
					if (KickStarter.player.ID == playerID)
					{
						Debug.Log ("Cannot switch player - already controlling the desired prefab.");
						return 0f;
					}

					if (settingsManager.players[playerNumber].playerOb != null)
					{
						SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();
						saveSystem.SaveCurrentPlayerData ();

						Vector3 oldPlayerPosition = KickStarter.player.transform.position;
						Quaternion oldPlayerRotation = KickStarter.player.transform.rotation;
						Vector3 oldPlayerScale = KickStarter.player.transform.localScale;

						if (oldPlayer == OldPlayer.ReplaceWithNPC && oldPlayerNPC != null &&
						    (newPlayerPosition == NewPlayerPosition.ReplaceNPC || newPlayerPosition == NewPlayerPosition.AppearAtMarker))
						{
							oldPlayerNPC.transform.position = oldPlayerPosition;
							oldPlayerNPC.transform.rotation = oldPlayerRotation;
							oldPlayerNPC.transform.localScale = oldPlayerScale;
						}

						Quaternion newRotation = Quaternion.identity;
						if (newPlayerPosition == NewPlayerPosition.ReplaceCurrentPlayer)
						{
							newRotation = oldPlayerRotation;
						}
						else if (newPlayerPosition == NewPlayerPosition.ReplaceNPC && newPlayerNPC)
						{
							newRotation = newPlayerNPC.transform.rotation;
						}
						else if (newPlayerPosition == NewPlayerPosition.AppearAtMarker && newPlayerMarker)
						{
							newRotation = newPlayerMarker.transform.rotation;
						}

						KickStarter.ResetPlayer (settingsManager.players[playerNumber].playerOb, playerID, true, newRotation);

						Player newPlayer = KickStarter.player;

						int sceneToLoad = Application.loadedLevel;
						if (restorePreviousData && saveSystem.DoesPlayerDataExist (playerID))
						{
							sceneToLoad = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>().GetPlayerScene (playerID);

							if (sceneToLoad != Application.loadedLevel)
							{
								saveSystem.loadingGame = LoadingGame.JustSwitchingPlayer;
								GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>().ChangeScene (sceneToLoad, true);
							}
						}
						else
						{
							if (newPlayerPosition == NewPlayerPosition.ReplaceCurrentPlayer)
							{
								newPlayer.Teleport (oldPlayerPosition);
								newPlayer.SetRotation (oldPlayerRotation);
								newPlayer.transform.localScale = oldPlayerScale;
							}
							else if (newPlayerPosition == NewPlayerPosition.ReplaceNPC)
							{
								if (newPlayerNPC)
								{
									newPlayer.Teleport (newPlayerNPC.transform.position);
									newPlayer.SetRotation (newPlayerNPC.transform.rotation);
									newPlayer.transform.localScale = newPlayerNPC.transform.localScale;

									newPlayerNPC.transform.position += new Vector3 (100f, -100f, 100f);
								}
							}
							else if (newPlayerPosition == NewPlayerPosition.AppearAtMarker)
							{
								if (newPlayerMarker)
								{
									newPlayer.Teleport (newPlayerMarker.transform.position);
									newPlayer.SetRotation (newPlayerMarker.transform.rotation);
									newPlayer.transform.localScale = newPlayerMarker.transform.localScale;
								}
							}
							else if (newPlayerPosition == NewPlayerPosition.AppearInOtherScene)
							{
								if (newPlayerScene != Application.loadedLevel)
								{
									GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>().ChangeScene (newPlayerScene, true);
								}
							}
						}

						MainCamera mainCamera = KickStarter.mainCamera;
						if (mainCamera.attachedCamera)
						{
							mainCamera.attachedCamera.MoveCameraInstant ();
						}

						AssetLoader.UnloadAssets ();
					}
					else
					{
						Debug.LogWarning ("Cannot switch player - no player prefabs is defined.");
					}
				}
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			if (!settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
			
			if (!settingsManager)
			{
				return;
			}
			
			if (settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
			{
				EditorGUILayout.HelpBox ("This Action requires Player Switching to be allowed, as set in the Settings Manager.", MessageType.Info);
				return;
			}
			
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			playerNumber = -1;
			
			if (settingsManager.players.Count > 0)
			{
				foreach (PlayerPrefab playerPrefab in settingsManager.players)
				{
					if (playerPrefab.playerOb != null)
					{
						labelList.Add (playerPrefab.playerOb.name);
					}
					else
					{
						labelList.Add ("(Undefined prefab)");
					}
					
					// If a player has been removed, make sure selected player is still valid
					if (playerPrefab.ID == playerID)
					{
						playerNumber = i;
					}
					
					i++;
				}
				
				if (playerNumber == -1)
				{
					// Wasn't found (item was possibly deleted), so revert to zero
					Debug.LogWarning ("Previously chosen Player no longer exists!");
					
					playerNumber = 0;
					playerID = 0;
				}
				
				playerNumber = EditorGUILayout.Popup ("New Player:", playerNumber, labelList.ToArray());
				playerID = settingsManager.players[playerNumber].ID;

				restorePreviousData = EditorGUILayout.Toggle ("Restore position?", restorePreviousData);
				if (restorePreviousData)
				{
					EditorGUILayout.LabelField ("If first time in game:", EditorStyles.boldLabel);
				}

				newPlayerPosition = (NewPlayerPosition) EditorGUILayout.EnumPopup ("New Player position:", newPlayerPosition);

				if (newPlayerPosition == NewPlayerPosition.ReplaceNPC)
				{
					newPlayerNPC = (NPC) EditorGUILayout.ObjectField ("NPC to be replaced:", newPlayerNPC, typeof (NPC), true);
					
					newPlayerNPC_ID = FieldToID <NPC> (newPlayerNPC, newPlayerNPC_ID);
					newPlayerNPC = IDToField <NPC> (newPlayerNPC, newPlayerNPC_ID, false);
				}
				else if (newPlayerPosition == NewPlayerPosition.AppearAtMarker)
				{
					newPlayerMarker = (Marker) EditorGUILayout.ObjectField ("Marker to appear at:", newPlayerMarker, typeof (Marker), true);

					newPlayerMarker_ID = FieldToID <Marker> (newPlayerMarker, newPlayerMarker_ID);
					newPlayerMarker = IDToField <Marker> (newPlayerMarker, newPlayerMarker_ID, false);
				}
				else if (newPlayerPosition == NewPlayerPosition.AppearInOtherScene)
				{
					newPlayerScene = EditorGUILayout.IntField ("Scene to appear in:", newPlayerScene);
				}

				if (newPlayerPosition == NewPlayerPosition.ReplaceNPC || newPlayerPosition == NewPlayerPosition.AppearAtMarker)
				{
					EditorGUILayout.Space ();
					oldPlayer = (OldPlayer) EditorGUILayout.EnumPopup ("Old Player", oldPlayer);

					if (oldPlayer == OldPlayer.ReplaceWithNPC)
					{
						oldPlayerNPC = (NPC) EditorGUILayout.ObjectField ("NPC to replace old Player:", oldPlayerNPC, typeof (NPC), true);

						oldPlayerNPC_ID = FieldToID <NPC> (oldPlayerNPC, oldPlayerNPC_ID);
						oldPlayerNPC = IDToField <NPC> (oldPlayerNPC, oldPlayerNPC_ID, false);
					}
				}
			}
			
			else
			{
				EditorGUILayout.LabelField ("No players exist!");
				playerID = -1;
				playerNumber = -1;
			}

			EditorGUILayout.Space ();
			
			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			if (!settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}

			if (settingsManager && settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				if (settingsManager.players.Count > 0 && settingsManager.players.Count > playerNumber && playerNumber > -1)
				{
					if (settingsManager.players[playerNumber].playerOb != null)
					{
						return " (" + settingsManager.players[playerNumber].playerOb.name + ")";
					}
					else
					{
						return (" (Undefined prefab");
					}
				}
			}
			
			return "";
		}
		
		#endif

	}

}