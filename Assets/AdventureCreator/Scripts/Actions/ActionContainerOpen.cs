/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionContainerOpen.cs"
 * 
 *	This action makes a Container active for display in an
 *	InventoryBox. To de-activate it, close a Menu with AppearType
 *	set to OnContainer.
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
	public class ActionContainerOpen : Action
	{

		public bool useActive = false;
		public int parameterID = -1;
		public int constantID = 0;
		public Container container;
		
		
		public ActionContainerOpen ()
		{
			this.isDisplayed = true;
			title = "Container: Open";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			container = AssignFile <Container> (parameters, parameterID, constantID, container);

			if (useActive)
			{
				container = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>().activeContainer;
			}
		}

		
		override public float Run ()
		{
			if (container == null)
			{
				return 0f;
			}

			container.Interact ();

			return 0f;
		}
		

		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			useActive = EditorGUILayout.Toggle ("Affect active container?", useActive);
			if (!useActive)
			{
				parameterID = Action.ChooseParameterGUI ("Container:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					container = null;
				}
				else
				{
					container = (Container) EditorGUILayout.ObjectField ("Container:", container, typeof (Container), true);
					
					constantID = FieldToID <Container> (container, constantID);
					container = IDToField <Container> (container, constantID, false);
				}
			}
		}
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (container)
			{
				labelAdd = " (" + container + ")";
			}
			
			return labelAdd;
		}

		#endif
		
	}

}