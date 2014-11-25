/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RuntimeInventory.cs"
 * 
 *	This script creates a local copy of the InventoryManager's items.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class RuntimeInventory : MonoBehaviour
	{
		
		[HideInInspector] public List<InvItem> localItems = new List<InvItem>();
		[HideInInspector] public List<InvItem> craftingItems = new List<InvItem>();
		[HideInInspector] public ActionListAsset unhandledCombine;
		[HideInInspector] public ActionListAsset unhandledHotspot;
		
		[HideInInspector] public InvItem selectedItem = null;
		[HideInInspector] public InvItem hoverItem = null;
		[HideInInspector] public List<int> matchingInvInteractions = new List<int>();
		private GUIStyle countStyle;
		private TextEffects countTextEffects;

		private InventoryManager inventoryManager;
		private SettingsManager settingsManager;
		private PlayerInteraction playerInteraction;
		
		[HideInInspector] public InvItem highlightItem = null;
		private HighlightState highlightState = HighlightState.None;
		private float pulse = 0f;
		private int pulseDirection = 0; // 0 = none, 1 = in, -1 = out
		
		
		public void Awake ()
		{
			selectedItem = null;
			GetReferences ();
			
			craftingItems.Clear ();
			localItems.Clear ();
			GetItemsOnStart ();
			
			if (inventoryManager)
			{
				unhandledCombine = inventoryManager.unhandledCombine;
				unhandledHotspot = inventoryManager.unhandledHotspot;
			}
			else
			{
				Debug.LogError ("An Inventory Manager is required - please use the Adventure Creator window to create one.");
			}
		}
		
		
		private void OnLevelWasLoaded ()
		{
			if (!settingsManager.IsInLoadingScene ())
			{
				GetReferences ();
				SetNull ();
			}
		}
		
		
		private void GetReferences ()
		{
			if (AdvGame.GetReferences ())
			{
				if (AdvGame.GetReferences ().inventoryManager)
				{
					inventoryManager = AdvGame.GetReferences ().inventoryManager;
				}
				if (AdvGame.GetReferences ().settingsManager)
				{
					settingsManager = AdvGame.GetReferences ().settingsManager;
				}
			}

			playerInteraction = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>();
		}
		
		
		public void SetNull ()
		{
			selectedItem = null;
			highlightItem = null;
			PlayerMenus.ResetInventoryBoxes ();
		}
		
		
		public void SelectItemByID (int _id)
		{
			if (_id == -1)
			{
				SetNull ();
				return;
			}

			foreach (InvItem item in localItems)
			{
				if (item != null && item.id == _id)
				{
					selectedItem = item;
					PlayerMenus.ResetInventoryBoxes ();
					return;
				}
			}
			
			SetNull ();
			GetReferences ();
			Debug.LogWarning ("Want to select inventory item " + inventoryManager.GetLabel (_id) + " but player is not carrying it.");
		}
		
		
		public void SelectItem (InvItem item)
		{
			if (selectedItem == item)
			{
				selectedItem = null;
				GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>().ResetSelectedCursor ();
			}
			else
			{
				selectedItem = item;
			}
			
			PlayerMenus.ResetInventoryBoxes ();
		}
		
		
		private void GetItemsOnStart ()
		{
			if (inventoryManager)
			{
				foreach (InvItem item in inventoryManager.items)
				{
					if (item.carryOnStart)
					{
						if (!item.canCarryMultiple)
						{
							item.count = 1;
						}
						
						if (item.count < 1)
						{
							continue;
						}
						
						item.recipeSlot = -1;
						
						if (item.canCarryMultiple && item.useSeparateSlots)
						{
							for (int i=0; i<item.count; i++)
							{
								InvItem newItem = new InvItem (item);
								newItem.count = 1;
								localItems.Add (newItem);
							}
						}
						else
						{
							localItems.Add (new InvItem (item));
						}
					}
				}
			}
			else
			{
				Debug.LogError ("No Inventory Manager found - please use the Adventure Creator window to create one.");
			}
		}
		
		
		public void Add (int _id, int amount, bool selectAfter)
		{
			Add (_id, amount, localItems, selectAfter);
		}
		
		
		public void Remove (int _id, int amount, bool setAmount)
		{
			Remove (_id, amount, setAmount, localItems);
		}
		
		
		private void Add (int _id, int amount, List<InvItem> itemList, bool selectAfter)
		{
			GetReferences ();
			ReorderItems ();
			
			// Raise "count" by 1 for appropriate ID
			foreach (InvItem item in itemList)
			{
				if (item != null && item.id == _id)
				{
					if (item.canCarryMultiple)
					{
						if (item.useSeparateSlots)
						{
							break;
						}
						else
						{
							item.count += amount;
						}
					}
					
					if (selectAfter)
					{
						SelectItem (item);
					}
					return;
				}
			}
			
			// Not already carrying the item
			foreach (InvItem assetItem in inventoryManager.items)
			{
				if (assetItem.id == _id)
				{
					InvItem newItem = new InvItem (assetItem);
					if (!newItem.canCarryMultiple)
					{
						amount = 1;
					}
					newItem.recipeSlot = -1;
					newItem.count = amount;
					
					if (settingsManager.canReorderItems)
					{
						// Insert into first "blank" space
						for (int i=0; i<itemList.Count; i++)
						{
							if (itemList[i] == null)
							{
								itemList[i] = newItem;
								if (selectAfter)
								{
									SelectItem (newItem);
								}
								
								if (newItem.canCarryMultiple && newItem.useSeparateSlots)
								{
									int count = newItem.count-1;
									newItem.count = 1;
									for (int j=0; j<count; j++)
									{
										itemList.Add (newItem);
									}
								}
								return;
							}
						}
					}
					
					if (newItem.canCarryMultiple && newItem.useSeparateSlots)
					{
						int count = newItem.count;
						newItem.count = 1;
						for (int i=0; i<count; i++)
						{
							itemList.Add (newItem);
						}
					}
					else
					{
						itemList.Add (newItem);
					}
					
					if (selectAfter)
					{
						SelectItem (newItem);
					}
					return;
				}
			}
			
			RemoveEmptySlots (itemList);
		}
		
		
		public void Remove (InvItem _item)
		{
			if (_item != null && localItems.Contains (_item))
			{
				if (_item == selectedItem)
				{
					SetNull ();
				}
				
				localItems [localItems.IndexOf (_item)] = null;
				
				ReorderItems ();
				RemoveEmptySlots (localItems);
			}
		}
		
		
		private void Remove (int _id, int amount, bool setAmount, List<InvItem> itemList)
		{
			if (amount <= 0)
			{
				return;
			}
			
			foreach (InvItem item in itemList)
			{
				if (item != null && item.id == _id)
				{
					if (item.canCarryMultiple && item.useSeparateSlots)
					{
						itemList [itemList.IndexOf (item)] = null;
						amount --;
						
						if (amount == 0)
						{
							break;
						}
						
						continue;
					}
					
					if (!item.canCarryMultiple || !setAmount)
					{
						itemList [itemList.IndexOf (item)] = null;
						amount = 0;
					}
					else
					{
						if (item.count > 0)
						{
							int numLeft = item.count - amount;
							item.count -= amount;
							amount = numLeft;
						}
						if (item.count < 1)
						{
							itemList [itemList.IndexOf (item)] = null;
						}
					}
					
					ReorderItems ();
					RemoveEmptySlots (itemList);
					
					if (itemList.Count == 0)
					{
						return;
					}
					
					if (amount <= 0)
					{
						return;
					}
				}
			}
			
			ReorderItems ();
			RemoveEmptySlots (itemList);
		}
		
		
		private void ReorderItems ()
		{
			if (!settingsManager.canReorderItems)
			{
				bool foundNone = false;
				
				while (!foundNone)
				{
					bool foundOne = false;

					for (int i=0; i<localItems.Count; i++)
					{
						if (localItems[i] == null)
						{
							localItems.RemoveAt (i);
							foundOne = true;
						}
					}
					
					if (!foundOne)
					{
						foundNone = true;
					}
				}
			}
		}
		
		
		private void RemoveEmptyCraftingSlots ()
		{
			// Remove empty slots on end
			for (int i=craftingItems.Count-1; i>=0; i--)
			{
				if (localItems[i] == null)
				{
					localItems.RemoveAt (i);
				}
				else
				{
					return;
				}
			}
		}
		
		
		
		private void RemoveEmptySlots (List<InvItem> itemList)
		{
			// Remove empty slots on end
			for (int i=localItems.Count-1; i>=0; i--)
			{
				if (localItems[i] == null)
				{
					localItems.RemoveAt (i);
				}
				else
				{
					return;
				}
			}
		}
		
		
		public string GetLabel (InvItem item)
		{
			if (Options.GetLanguage () > 0)
			{
				return (SpeechManager.GetTranslation (item.lineID, Options.GetLanguage ()));
			}
			else if (item.altLabel != "")
			{
				return (item.altLabel);
			}
			
			return (item.label);
		}
		
		
		public int GetCount (int _id)
		{
			foreach (InvItem item in localItems)
			{
				if (item != null && item.id == _id)
				{
					return (item.count);
				}
			}
			
			return 0;
		}
		
		
		public InvItem GetCraftingItem (int _id)
		{
			foreach (InvItem item in craftingItems)
			{
				if (item.id == _id)
				{
					return item;
				}
			}
			
			return null;
		}
		
		
		public InvItem GetItem (int _id)
		{
			foreach (InvItem item in localItems)
			{
				if (item != null && item.id == _id)
				{
					return item;
				}
			}
			
			return null;
		}
		
		
		public void Look (InvItem item)
		{
			if (item == null || item.recipeSlot > -1) return;
			GetReferences ();
			
			if (item.lookActionList)
			{
				AdvGame.RunActionListAsset (item.lookActionList);
			}
		}
		
		
		public void Use (InvItem item)
		{
			if (item == null || item.recipeSlot > -1) return;
			GetReferences ();
			
			if (item.useActionList)
			{
				selectedItem = null;
				AdvGame.RunActionListAsset (item.useActionList);
			}
			else
			{
				SelectItem (item);
			}
		}
		
		
		public void RunInteraction (InvItem invItem, int iconID)
		{
			if (invItem == null || invItem.recipeSlot > -1) return;
			GetReferences ();
			
			foreach (InvInteraction interaction in invItem.interactions)
			{
				if (interaction.icon.id == iconID)
				{
					if (interaction.actionList)
					{
						AdvGame.RunActionListAsset (interaction.actionList);
					}
					break;
				}
			}
		}
		
		
		public void RunInteraction (bool isHover, int iconID)
		{
			GetReferences ();

			InvItem item = selectedItem;
			if (isHover)
			{
				item = hoverItem;
			}

			foreach (InvInteraction interaction in item.interactions)
			{
				if (interaction.icon.id == iconID)
				{
					if (interaction.actionList)
					{
						AdvGame.RunActionListAsset (interaction.actionList);
					}
					break;
				}
			}
			
			selectedItem = null;
		}
		
		
		public void ShowInteractions (InvItem item)
		{
			selectedItem = item;

			PlayerMenus playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
			playerMenus.SetInteractionMenus (true);
		}


		public void Combine (int ID)
		{
			Combine (GetItem (ID));
		}
		
		
		public void Combine (InvItem item)
		{
			if (item == null || item.recipeSlot > -1) return;
			
			GetReferences ();
			
			if (item == selectedItem)
			{
				selectedItem = null;
				
				if ((settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || settingsManager.inventoryInteractions == InventoryInteractions.Single) && settingsManager.inventoryDragDrop && settingsManager.inventoryDropLook)
				{
					Look (item);
				}
			}
			else
			{
				for (int i=0; i<item.combineID.Count; i++)
				{
					if (item.combineID[i] == selectedItem.id && item.combineActionList[i] != null)
					{
						PlayerMenus.ForceOffAllMenus (true);
						selectedItem = null;
						AdvGame.RunActionListAsset (item.combineActionList [i]);
						return;
					}
				}
				
				if (settingsManager.reverseInventoryCombinations || settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
				{
					// Try opposite: search selected item instead
					for (int i=0; i<selectedItem.combineID.Count; i++)
					{
						if (selectedItem.combineID[i] == item.id && selectedItem.combineActionList[i] != null)
						{
							ActionListAsset assetFile = selectedItem.combineActionList[i];
							PlayerMenus.ForceOffAllMenus (true);
							selectedItem = null;
							AdvGame.RunActionListAsset (assetFile);
							return;
						}
					}
				}
				
				// Found no combine match
				if (selectedItem.unhandledCombineActionList)
				{
					ActionListAsset unhandledActionList = selectedItem.unhandledCombineActionList;
					selectedItem = null;
					AdvGame.RunActionListAsset (unhandledActionList);	
				}
				else if (unhandledCombine)
				{
					selectedItem = null;
					PlayerMenus.ForceOffAllMenus (true);
					AdvGame.RunActionListAsset (unhandledCombine);
				}
				else
				{
					selectedItem = null;
				}
			}
			
			GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>().ResetSelectedCursor ();
		}
		
		
		public List<InvItem> GetSelected ()
		{
			List<InvItem> items = new List<InvItem>();
			
			if (selectedItem != null)
			{
				items.Add (selectedItem);
			}
			
			return items;
		}
		
		
		public bool IsItemCarried (InvItem _item)
		{
			if (_item == null) return false;
			foreach (InvItem item in localItems)
			{
				if (item == _item)
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		public void RemoveRecipes ()
		{
			while (craftingItems.Count > 0)
			{
				for (int i=0; i<craftingItems.Count; i++)
				{
					Add (craftingItems[i].id, craftingItems[i].count, false);
					craftingItems.RemoveAt (i);
				}
			}
			PlayerMenus.ResetInventoryBoxes ();
		}
		
		
		public void TransferCraftingToLocal (int _recipeSlot, bool selectAfter)
		{
			foreach (InvItem item in craftingItems)
			{
				if (item.recipeSlot == _recipeSlot)
				{
					Add (item.id, item.count, selectAfter);
					SelectItemByID (item.id);
					craftingItems.Remove (item);
					return;
				}
			}
		}
		
		
		public void TransferLocalToContainer (Container _container, InvItem _item, int _index)
		{
			if (_item != null && localItems.Contains (_item))
			{
				localItems [localItems.IndexOf (_item)] = null;
				ReorderItems ();
				RemoveEmptySlots (localItems);
				
				SetNull ();
			}
		}
		
		
		public void TransferLocalToCrafting (InvItem _item, int _slot)
		{
			if (_item != null && localItems.Contains (_item))
			{
				_item.recipeSlot = _slot;
				craftingItems.Add (_item);
				
				localItems [localItems.IndexOf (_item)] = null;
				ReorderItems ();
				RemoveEmptySlots (localItems);
				
				SetNull ();
			}
		}
		
		
		public List<InvItem> MatchInteractions ()
		{
			List<InvItem> items = new List<InvItem>();
			matchingInvInteractions = new List<int>();

			if (settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot && hoverItem != null)
			{
				items = MatchInteractionsFromItem (items, hoverItem);
			}

			else if (settingsManager.SelectInteractionMethod () != SelectInteractions.CyclingCursorAndClickingHotspot && selectedItem != null)
			{
				items = MatchInteractionsFromItem (items, selectedItem);
			}

			else if (playerInteraction.GetActiveHotspot ())
			{
				List<Button> invButtons = playerInteraction.GetActiveHotspot ().invButtons;
				foreach (Button button in invButtons)
				{
					foreach (InvItem item in localItems)
					{
						if (item != null && item.id == button.invID && !button.isDisabled)
						{
							matchingInvInteractions.Add (invButtons.IndexOf (button));
							items.Add (item);
							break;
						}
					}
				}
			}

			return items;
		}


		private List<InvItem> MatchInteractionsFromItem (List<InvItem> items, InvItem _item)
		{
			foreach (int combineID in _item.combineID)
			{
				foreach (InvItem item in localItems)
				{
					if (item != null && item.id == combineID)
					{
						matchingInvInteractions.Add (_item.combineID.IndexOf (combineID));
						items.Add (item);
						break;
					}
				}
			}
			return items;
		}
		
		
		public Recipe CalculateRecipe (bool autoCreateMatch)
		{
			if (inventoryManager == null)
			{
				return null;
			}
			
			foreach (Recipe recipe in inventoryManager.recipes)
			{
				if (autoCreateMatch)
				{
					if (!recipe.autoCreate)
					{
						break;
					}
				}
				
				// Are any invalid ingredients present?
				foreach (InvItem item in craftingItems)
				{
					bool found = false;
					foreach (Ingredient ingredient in recipe.ingredients)
					{
						if (ingredient.itemID == item.id)
						{
							found = true;
						}
					}
					if (!found)
					{
						// Not present in recipe
						return null;
					}
				}
				
				bool canCreateRecipe = true;
				while (canCreateRecipe)
				{
					foreach (Ingredient ingredient in recipe.ingredients)
					{
						// Is ingredient present (and optionally, in correct slot)
						InvItem ingredientItem = GetCraftingItem (ingredient.itemID);
						if (ingredientItem == null)
						{
							canCreateRecipe = false;
							break;
						}
						
						if ((recipe.useSpecificSlots && ingredientItem.recipeSlot == (ingredient.slotNumber -1)) || !recipe.useSpecificSlots)
						{
							if ((ingredientItem.canCarryMultiple && ingredientItem.count >= ingredient.amount) || !ingredientItem.canCarryMultiple)
							{
								if (canCreateRecipe && recipe.ingredients.IndexOf (ingredient) == (recipe.ingredients.Count -1))
								{
									return recipe;
								}
							}
							else canCreateRecipe = false;
						}
						else canCreateRecipe = false;
					}
				}
			}
			
			return null;
		}
		
		
		public void PerformCrafting (Recipe recipe, bool selectAfter)
		{
			foreach (Ingredient ingredient in recipe.ingredients)
			{
				for (int i=0; i<craftingItems.Count; i++)
				{
					if (craftingItems [i].id == ingredient.itemID)
					{
						if (ingredient.amount > 1)
						{
							craftingItems [i].count -= ingredient.amount;
							if (craftingItems [i].count < 1)
							{
								craftingItems.RemoveAt (i);
							}
						}
						else
						{
							craftingItems.RemoveAt (i);
						}
					}
				}
			}
			
			RemoveEmptyCraftingSlots ();
			Add (recipe.resultID, 1, selectAfter);
		}
		
		
		public void MoveItemToIndex (InvItem item, List<InvItem> items, int index)
		{
			if (item != null)
			{
				if (settingsManager.canReorderItems)
				{
					// Check nothing in place already
					int oldIndex = items.IndexOf (item);
					while (items.Count <= Mathf.Max (index, oldIndex))
					{
						items.Add (null);
					}
					
					if (items [index] == null)
					{
						items [index] = item;
						items [oldIndex] = null;
						
					}
					
					SetNull ();
					RemoveEmptySlots (items);
				}
				else if (items.IndexOf (item) == index)
				{
					SetNull ();
				}
			}
		}
		
		
		public void SetFont (Font font, int size, Color color, TextEffects textEffects)
		{
			countStyle = new GUIStyle();
			countStyle.font = font;
			countStyle.fontSize = size;
			countStyle.normal.textColor = color;
			countStyle.alignment = TextAnchor.MiddleCenter;
			countTextEffects = textEffects;
		}
		
		
		public void DrawHighlighted (Rect _rect)
		{
			if (highlightItem == null || highlightItem.activeTex == null) return;
			
			if (highlightState == HighlightState.None)
			{
				GUI.DrawTexture (_rect, highlightItem.activeTex, ScaleMode.StretchToFill, true, 0f);
				return;
			}
			
			if (pulseDirection == 0)
			{
				pulse = 0f;
				pulseDirection = 1;
			}
			else if (pulseDirection == 1)
			{
				pulse += settingsManager.inventoryPulseSpeed * Time.deltaTime;
			}
			else if (pulseDirection == -1)
			{
				pulse -= settingsManager.inventoryPulseSpeed * Time.deltaTime;
			}
			
			if (pulse > 1f)
			{
				pulse = 1f;
				
				if (highlightState == HighlightState.Normal)
				{
					highlightState = HighlightState.None;
					GUI.DrawTexture (_rect, highlightItem.activeTex, ScaleMode.StretchToFill, true, 0f);
					return;
				}
				else
				{
					pulseDirection = -1;
				}
			}
			else if (pulse < 0f)
			{
				pulse = 0f;
				
				if (highlightState == HighlightState.Pulse)
				{
					pulseDirection = 1;
				}
				else
				{
					highlightState = HighlightState.None;
					GUI.DrawTexture (_rect, highlightItem.tex, ScaleMode.StretchToFill, true, 0f);
					highlightItem = null;
					return;
				}
			}
			
			
			Color backupColor = GUI.color;
			Color tempColor = GUI.color;
			
			tempColor.a = pulse;
			GUI.color = tempColor;
			GUI.DrawTexture (_rect, highlightItem.activeTex, ScaleMode.StretchToFill, true, 0f);
			GUI.color = backupColor;
			GUI.DrawTexture (_rect, highlightItem.tex, ScaleMode.StretchToFill, true, 0f);
		}
		
		
		public void HighlightItemOnInstant (int _id)
		{
			highlightItem = GetItem (_id);
			highlightState = HighlightState.None;
			pulse = 1f;
		}
		
		
		public void HighlightItemOffInstant ()
		{
			highlightItem = null;
			highlightState = HighlightState.None;
			pulse = 0f;
		}
		
		
		public void HighlightItem (int _id, HighlightType _type)
		{
			highlightItem = GetItem (_id);
			if (highlightItem == null) return;
			
			if (_type == HighlightType.Enable)
			{
				highlightState = HighlightState.Normal;
				pulseDirection = 1;
			}
			else if (_type == HighlightType.Disable)
			{
				highlightState = HighlightState.Normal;
				pulseDirection = -1;
			}
			else if (_type == HighlightType.PulseOnce)
			{
				highlightState = HighlightState.Flash;
				pulse = 0f;
				pulseDirection = 1;
			}
			else if (_type ==  HighlightType.PulseContinually)
			{
				highlightState = HighlightState.Pulse;
				pulse = 0f;
				pulseDirection = 1;
			}
		}
		
		
		public void StopHighlighting ()
		{
			highlightItem = null;
			highlightState = HighlightState.None;
		}


		public void DrawInventoryCount (Vector2 cursorPosition, float cursorSize, int count)
		{
			if (count > 1)
			{
				if (countTextEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (AdvGame.GUIBox (cursorPosition, cursorSize), count.ToString (), countStyle, Color.black, countStyle.normal.textColor, 2, countTextEffects);
				}
				else
				{
					GUI.Label (AdvGame.GUIBox (cursorPosition, cursorSize), count.ToString (), countStyle);
				}
			}
		}

		
		private void OnEnable ()
		{
			inventoryManager = null;
		}
		
		
		private void OnDestroy ()
		{
			inventoryManager = null;
			settingsManager = null;
		}
		
	}
	
}
