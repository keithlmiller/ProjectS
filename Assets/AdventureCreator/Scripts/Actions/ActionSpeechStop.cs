/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSpeechStop.cs"
 * 
 *	This Action forces off all playing speech
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
	public class ActionSpeechStop : Action
	{

		public bool forceMenus;


		public ActionSpeechStop ()
		{
			this.isDisplayed = true;
			title = "Dialogue: Stop speech";
		}
		
		
		override public float Run ()
		{
			Dialog dialog = GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>();
			dialog.KillDialog (forceMenus);

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			forceMenus = EditorGUILayout.Toggle ("Force off subtitles?", forceMenus);

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			return "";
		}
		
		#endif
		
	}

}