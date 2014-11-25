/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInventorySelect.cs"
 * 
 *	This action is used to automatically-select an inventory item.
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
	public class ActionInventorySelect : Action
	{

		public enum InventorySelectType { SelectItem, DeselectActive };
		public InventorySelectType selectType = InventorySelectType.SelectItem;

		public bool giveToPlayer = false;

		public int parameterID = -1;
		public int invID;
		private int invNumber;
		
		private InventoryManager inventoryManager;


		override public void AssignValues (List<ActionParameter> parameters)
		{
			invID = AssignInvItemID (parameters, parameterID, invID);
		}
		
		
		override public float Run ()
		{
			RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
			
			if (runtimeInventory)
			{
				if (selectType == InventorySelectType.DeselectActive)
				{
					runtimeInventory.SetNull ();
				}
				else
				{
					if (giveToPlayer)
					{
						runtimeInventory.Add (invID, 1, false);
					}

					runtimeInventory.SelectItemByID (invID);
				}
			}
			
			return 0f;
		}

		
		#if UNITY_EDITOR

		public ActionInventorySelect ()
		{
			this.isDisplayed = true;
			title = "Inventory: Select";
		}
		
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			selectType = (InventorySelectType) EditorGUILayout.EnumPopup ("Select type:", selectType);
			if (selectType == InventorySelectType.DeselectActive)
			{
				return;
			}

			if (!inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}
			
			if (inventoryManager)
			{
				// Create a string List of the field's names (for the PopUp box)
				List<string> labelList = new List<string>();
				
				int i = 0;
				if (parameterID == -1)
				{
					invNumber = -1;
				}
				
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem _item in inventoryManager.items)
					{
						labelList.Add (_item.label);
						
						// If an item has been removed, make sure selected variable is still valid
						if (_item.id == invID)
						{
							invNumber = i;
						}
						
						i++;
					}
					
					if (invNumber == -1)
					{
						Debug.LogWarning ("Previously chosen item no longer exists!");
						invNumber = 0;
						invID = 0;
					}

					//
					parameterID = Action.ChooseParameterGUI ("Inventory item:", parameters, parameterID, ParameterType.InventoryItem);
					if (parameterID >= 0)
					{
						invNumber = Mathf.Min (invNumber, inventoryManager.items.Count-1);
						invID = -1;
					}
					else
					{
						invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
						invID = inventoryManager.items[invNumber].id;
					}
					//
			
					giveToPlayer = EditorGUILayout.Toggle ("Add if not held?", giveToPlayer);
					
					AfterRunningOption ();
				}
		
				else
				{
					EditorGUILayout.HelpBox ("No inventory items exist!", MessageType.Info);
					invID = -1;
					invNumber = -1;
				}
			}
		}
		
		
		override public string SetLabel ()
		{
			if (selectType == InventorySelectType.DeselectActive)
			{
				return " (Deselect active)";
			}

			string labelAdd = "";
			string labelItem = "";
			
			if (inventoryManager)
			{
				if (inventoryManager.items.Count > 0 && invNumber >= 0 && inventoryManager.items.Count > invNumber)
				{
					labelItem = " " + inventoryManager.items[invNumber].label;
				}
			}
			
			labelAdd = " (" + labelItem + ")";
		
			return labelAdd;
		}

		#endif

	}

}