/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionRename.cs"
 * 
 *	This action renames Hotspots. A "Remember Name" script needs to be
 *	attached to said hotspot if the renaming is to carry across saved games.
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
	public class ActionRename : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public Hotspot hotspot;

		public string newName;
		
		
		public ActionRename ()
		{
			this.isDisplayed = true;
			title = "Hotspot: Rename";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			hotspot = AssignFile <Hotspot> (parameters, parameterID, constantID, hotspot);
		}
		
		
		override public float Run ()
		{
			if (hotspot && newName != "")
			{
				hotspot.hotspotName = newName;
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Hotspot to rename:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				hotspot = null;
			}
			else
			{
				hotspot = (Hotspot) EditorGUILayout.ObjectField ("Hotspot to rename:", hotspot, typeof (Hotspot), true);
				
				constantID = FieldToID <Hotspot> (hotspot, constantID);
				hotspot = IDToField <Hotspot> (hotspot, constantID, false);
			}
			
			newName = EditorGUILayout.TextField ("New label:", newName);
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (hotspot && newName != "")
			{
				labelAdd = " (" + hotspot.name + " to " + newName + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}

}