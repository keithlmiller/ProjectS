/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionListAsset.cs"
 * 
 *	This script stores a list of Actions in an asset file.
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
	public class ActionListAsset : ScriptableObject
	{
		public bool isSkippable = true;
		public ActionListType actionListType = ActionListType.PauseGameplay;
		public bool useParameters = false;
		public List<ActionParameter> parameters = new List<ActionParameter>();

		public List<AC.Action> actions = new List<AC.Action>();
	}


	public class ActionListAssetMenu
	{

		#if UNITY_EDITOR
		[MenuItem ("Assets/Create/Adventure Creator/ActionList")]
		public static void CreateAsset ()
		{
			ScriptableObject t = CustomAssetUtility.CreateAsset <ActionListAsset> ();
			string assetPath = AssetDatabase.GetAssetPath (t.GetInstanceID ());
			AssetDatabase.RenameAsset (assetPath, "New ActionList");
		}
		#endif

	}

}