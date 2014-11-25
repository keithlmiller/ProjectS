/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSceneCheck.cs"
 * 
 *	This action checks the player's last-visited scene,
 *	useful for running specific "player enters the room" cutscenes.
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
	public class ActionSceneCheck : ActionCheck
	{
		
		public int sceneNumber;
		public enum IntCondition { EqualTo, NotEqualTo };
		public enum SceneToCheck { Current, Previous };
		public SceneToCheck sceneToCheck = SceneToCheck.Current;
		public IntCondition intCondition;
		
		public ActionSceneCheck ()
		{
			this.isDisplayed = true;
			title = "Engine: Check scene";
		}

		
		override public bool CheckCondition ()
		{
			int actualSceneNumber = 0;
			if (sceneToCheck == SceneToCheck.Previous)
			{
				SceneChanger sceneChanger = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>();
				actualSceneNumber = sceneChanger.previousScene;
			}
			else
			{
				actualSceneNumber = Application.loadedLevel;
			}

			if (intCondition == IntCondition.EqualTo)
			{
				if (actualSceneNumber == sceneNumber)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.NotEqualTo)
			{
				if (actualSceneNumber != sceneNumber)
				{
					return true;
				}
			}
			
			return false;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			EditorGUILayout.BeginHorizontal();
				sceneToCheck = (SceneToCheck) EditorGUILayout.EnumPopup (sceneToCheck);
				EditorGUILayout.LabelField ("scene number is:", GUILayout.Width (100f));
				intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
				sceneNumber = EditorGUILayout.IntField (sceneNumber);
			EditorGUILayout.EndHorizontal();
		}

		#endif

	}

}