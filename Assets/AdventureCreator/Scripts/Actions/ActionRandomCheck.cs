/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionRandomCheck.cs"
 * 
 *	This action checks the value of a random number
 *	and performs different follow-up Actions accordingly.
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
	public class ActionRandomCheck : ActionCheckMultiple
	{
		
		public ActionRandomCheck ()
		{
			this.isDisplayed = true;
			title = "Variable: Check random number";
		}
		
		
		override public int End (List<Action> actions)
		{
			if (numSockets <= 0)
			{
				Debug.LogWarning ("Could not compute Random check because no values were possible!");
				return -1;
			}

			int randomResult = Random.Range (0, numSockets);

			return ProcessResult (randomResult, actions);
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			numSockets = EditorGUILayout.IntField ("# of possible values:", numSockets);
			numSockets = Mathf.Max (0, numSockets);
		}
		
		#endif
		
	}

}