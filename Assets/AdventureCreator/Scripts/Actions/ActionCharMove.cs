/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharMove.cs"
 * 
 *	This action moves characters by assinging them a Paths object.
 *	If a player is moved, the game will automatically pause.
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
	public class ActionCharMove : Action
	{

		public int charToMoveParameterID = -1;
		public int movePathParameterID = -1;

		public int charToMoveID = 0;
		public int movePathID = 0;
		
		public Paths movePath;
		public bool isPlayer;
		public Char charToMove;
		public bool doTeleport;
		public bool doStop;
		
		
		public ActionCharMove ()
		{
			this.isDisplayed = true;
			title = "Character: Move along path";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			charToMove = AssignFile <Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);
			movePath = AssignFile <Paths> (parameters, movePathParameterID, movePathID, movePath);

			if (isPlayer)
			{
				charToMove = KickStarter.player;
			}
		}


		override public float Run ()
		{
			if (movePath && movePath.GetComponent <Char>())
			{
				Debug.LogWarning ("Can't follow a Path attached to a Character!");
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				if (charToMove)
				{
					if (charToMove is NPC)
					{
						NPC npcToMove = (NPC) charToMove;
						npcToMove.FollowReset ();
					}

					if (doStop)
					{
						charToMove.EndPath ();
					}
					else if (movePath)
					{
						if (doTeleport)
						{
							charToMove.Teleport (movePath.transform.position);
							
							// Set rotation if there is more than one node
							if (movePath.nodes.Count > 1)
							{
								charToMove.SetLookDirection (movePath.nodes[1] - movePath.nodes[0], true);
							}
						}
						
						if (isPlayer && movePath.pathType != AC_PathType.ForwardOnly)
						{
							Debug.LogWarning ("Cannot move player along a non-forward only path, as this will create an indefinite cutscene.");
						}
						else
						{
							if (willWait && movePath.pathType != AC_PathType.ForwardOnly)
							{
								willWait = false;
								Debug.LogWarning ("Cannot pause while character moves along a non-forward only path, as this will create an indefinite cutscene.");
							}
							
							charToMove.SetPath (movePath);
						
							if (willWait)
							{
								return defaultPauseTime;
							}
						}
					}
				}

				return 0f;
			}
			else
			{
				if (charToMove.GetPath () != movePath)
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


		override public void Skip ()
		{
			if (charToMove)
			{
				if (charToMove is NPC)
				{
					NPC npcToMove = (NPC) charToMove;
					npcToMove.FollowReset ();
				}
				
				if (doStop)
				{
					charToMove.EndPath ();
				}
				else if (movePath)
				{
					if (movePath.pathType == AC_PathType.ForwardOnly)
					{
						// Place at end
						int i = movePath.nodes.Count-1;
						charToMove.transform.position = movePath.nodes [i];
						if (i>0)
						{
							charToMove.SetLookDirection (movePath.nodes[i] - movePath.nodes[i-1], true);
						}
						return;
					}

					if (doTeleport)
					{
						charToMove.transform.position = movePath.transform.position;
						
						// Set rotation if there is more than one node
						if (movePath.nodes.Count > 1)
						{
							charToMove.SetLookDirection (movePath.nodes[1] - movePath.nodes[0], true);
						}
					}

					if (!isPlayer)
					{
						charToMove.SetPath (movePath);
					}
				}
			}
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);

			if (!isPlayer)
			{
				charToMoveParameterID = Action.ChooseParameterGUI ("Character to move:", parameters, charToMoveParameterID, ParameterType.GameObject);
				if (charToMoveParameterID >= 0)
				{
					charToMoveID = 0;
					charToMove = null;
				}
				else
				{
					charToMove = (Char) EditorGUILayout.ObjectField ("Character to move:", charToMove, typeof (Char), true);
					
					charToMoveID = FieldToID <Char> (charToMove, charToMoveID);
					charToMove = IDToField <Char> (charToMove, charToMoveID, false);
				}
			}

			doStop = EditorGUILayout.Toggle ("Stop moving?", doStop);
			if (!doStop)
			{
				movePathParameterID = Action.ChooseParameterGUI ("Path to follow:", parameters, movePathParameterID, ParameterType.GameObject);
				if (movePathParameterID >= 0)
				{
					movePathID = 0;
					movePath = null;
				}
				else
				{
					movePath = (Paths) EditorGUILayout.ObjectField ("Path to follow:", movePath, typeof(Paths), true);
					
					movePathID = FieldToID <Paths> (movePath, movePathID);
					movePath = IDToField <Paths> (movePath, movePathID, false);
				}

				doTeleport = EditorGUILayout.Toggle ("Teleport to start?", doTeleport);
				willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);

				if (movePath != null && movePath.GetComponent <Char>())
				{
					EditorGUILayout.HelpBox ("Can't follow a Path attached to a Character!", MessageType.Warning);
				}
			}
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (charToMove && movePath)
			{
				labelAdd = " (" + charToMove.name + " to " + movePath.name + ")";
			}
			else if (isPlayer && movePath)
			{
				labelAdd = " (Player to " + movePath.name + ")";
			}
			
			return labelAdd;
		}

		#endif
		
	}

}