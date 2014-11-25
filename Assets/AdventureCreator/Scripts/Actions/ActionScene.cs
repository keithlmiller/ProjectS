/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionScene.cs"
 * 
 *	This action loads a new scene.
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
	public class ActionScene : Action
	{
		
		public int sceneNumber;
		
		
		public ActionScene ()
		{
			this.isDisplayed = true;
			title = "Engine: Change scene";
			numSockets = 0;
		}
		
		
		override public float Run ()
		{
			if (sceneNumber > -1)
			{
				SceneChanger sceneChanger = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>();
				sceneChanger.ChangeScene (sceneNumber, true);
			}
			
			return 0f;
		}


		override public int End (List<AC.Action> actions)
		{
			return -1;
		}
		

		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			sceneNumber = EditorGUILayout.IntField ("Scene number:", sceneNumber);
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = " (" + sceneNumber + ")";

			return labelAdd;
		}

		#endif
		
	}

}