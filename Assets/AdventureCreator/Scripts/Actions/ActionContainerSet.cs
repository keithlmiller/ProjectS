/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionContainerSet.cs"
 * 
 *	This action is used to add or remove items from a container,
 *	with items being defined in the Inventory Manager.
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
	public class ActionContainerSet : Action
	{
		
		public enum ContainerAction {Add, Remove, RemoveAll};
		public ContainerAction containerAction;

		public int invParameterID = -1;
		public int invID;
		private int invNumber;

		public bool useActive = false;
		public int constantID = 0;
		public int parameterID = -1;
		public Container container;

		public bool setAmount = false;
		public int amount = 1;
		public bool transferToPlayer = false;
		
		private InventoryManager inventoryManager;


		override public void AssignValues (List<ActionParameter> parameters)
		{
			container = AssignFile <Container> (parameters, parameterID, constantID, container);
			invID = AssignInvItemID (parameters, invParameterID, invID);

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

			if (!setAmount)
			{
				amount = 1;
			}

			if (containerAction == ContainerAction.Add)
			{
				container.Add (invID, amount);
			}
			else if (containerAction == ContainerAction.Remove)
			{
				if (transferToPlayer)
				{
					RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
					runtimeInventory.Add (invID, amount, false);
				}

				container.Remove (invID, amount);
			}
			else if (containerAction == ContainerAction.RemoveAll)
			{
				if (transferToPlayer)
				{
					RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
					foreach (ContainerItem item in container.items)
					{
						runtimeInventory.Add (item.linkedID, item.count, false);
					}
				}

				container.items.Clear ();
			}

			PlayerMenus.ResetInventoryBoxes ();

			return 0f;
		}

		
		#if UNITY_EDITOR

		public ActionContainerSet ()
		{
			this.isDisplayed = true;
			title = "Container: Add or remove";
		}
		
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (AdvGame.GetReferences ().inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}
			
			if (inventoryManager)
			{
				// Create a string List of the field's names (for the PopUp box)
				List<string> labelList = new List<string>();
				
				int i = 0;
				if (invParameterID == -1)
				{
					invNumber = -1;
				}
				
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem _item in inventoryManager.items)
					{
						labelList.Add (_item.label);
						
						// If a item has been removed, make sure selected variable is still valid
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

					containerAction = (ContainerAction) EditorGUILayout.EnumPopup ("Method:", containerAction);

					if (containerAction == ContainerAction.RemoveAll)
					{
						transferToPlayer = EditorGUILayout.Toggle ("Transfer to Player?", transferToPlayer);
					}
					else
					{
						//
						invParameterID = Action.ChooseParameterGUI ("Inventory item:", parameters, invParameterID, ParameterType.InventoryItem);
						if (invParameterID >= 0)
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

						if (containerAction == ContainerAction.Remove)
						{
							transferToPlayer = EditorGUILayout.Toggle ("Transfer to Player?", transferToPlayer);
						}

						if (inventoryManager.items[invNumber].canCarryMultiple)
						{
							setAmount = EditorGUILayout.Toggle ("Set amount?", setAmount);
						
							if (setAmount)
							{
								if (containerAction == ContainerAction.Add)
								{
									amount = EditorGUILayout.IntField ("Increase count by:", amount);
								}
								else if (containerAction == ContainerAction.Remove)
								{
									amount = EditorGUILayout.IntField ("Reduce count by:", amount);
								}
							}
						}
					}

					AfterRunningOption ();
				}
		
				else
				{
					EditorGUILayout.LabelField ("No inventory items exist!");
					invID = -1;
					invNumber = -1;
				}
			}
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			string labelItem = "";

			if (!inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}

			if (inventoryManager)
			{
				if (inventoryManager.items.Count > 0)
				{
					if (invNumber > -1)
					{
						labelItem = " " + inventoryManager.items[invNumber].label;
					}
				}
			}
			
			if (containerAction == ContainerAction.Add)
			{
				labelAdd = " (Add" + labelItem + ")";
			}
			else if (containerAction == ContainerAction.Remove)
			{
				labelAdd = " (Remove" + labelItem + ")";
			}
			else if (containerAction == ContainerAction.RemoveAll)
			{
				labelAdd = " (Remove all)";
			}
		
			return labelAdd;
		}

		#endif

	}

}