/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuCrafting.cs"
 * 
 *	This MenuElement stores multiple Inventory Items to be combined.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class MenuCrafting : MenuElement
	{
		
		public TextEffects textEffects;
		public CraftingElementType craftingType = CraftingElementType.Ingredients;
		public int categoryID;
		public List<InvItem> items = new List<InvItem>();

		private Recipe activeRecipe;
		

		public override void Declare ()
		{
			isVisible = true;
			isClickable = true;
			numSlots = 4;
			SetSize (new Vector2 (6f, 10f));
			textEffects = TextEffects.None;
			craftingType = CraftingElementType.Ingredients;
			items = new List<InvItem>();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuCrafting newElement = CreateInstance <MenuCrafting>();
			newElement.Declare ();
			newElement.CopyCrafting (this);
			return newElement;
		}
		
		
		public void CopyCrafting (MenuCrafting _element)
		{
			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			numSlots = _element.numSlots;
			craftingType = _element.craftingType;
			PopulateList ();
			
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			craftingType = (CraftingElementType) EditorGUILayout.EnumPopup ("Crafting element type:", craftingType);

			if (craftingType == CraftingElementType.Ingredients)
			{
				numSlots = EditorGUILayout.IntSlider ("Number of slots:", numSlots, 1, 12);
				slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
				orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
				if (orientation == ElementOrientation.Grid)
				{
					gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
				}
			}
			else
			{
				categoryID = -1;
				numSlots = 1;
			}

			isClickable = true;
			EditorGUILayout.EndVertical ();
			
			PopulateList ();
			base.ShowGUI ();
		}
		
		
		private int GetBinSlot (int _id, List<InvBin> bins)
		{
			int i = 0;
			foreach (InvBin bin in bins)
			{
				if (bin.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			if (craftingType == CraftingElementType.Ingredients)
			{
				// Is slot filled?
				bool isFilled = false;
				foreach (InvItem _item in items)
				{
					if (_item.recipeSlot == _slot)
					{
						isFilled = true;
						break;
					}
				}
				
				GUI.Label (GetSlotRectRelative (_slot), "", _style);

				if (!isFilled)
				{
					return;
				}
				DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), _slot);
				_style.normal.background = null;
				
				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style, Color.black, _style.normal.textColor, 2, textEffects);
				}
				else
				{
					GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style);
				}
			}
			else if (craftingType == CraftingElementType.Output)
			{
				GUI.Label (GetSlotRectRelative (_slot), "", _style);
				if (items.Count > 0)
				{
					DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), _slot);
					_style.normal.background = null;
					
					if (textEffects != TextEffects.None)
					{
						AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style, Color.black, _style.normal.textColor, 2, textEffects);
					}
					else
					{
						GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style);
					}
				}
			}
		}
		

		public void HandleDefaultClick (MouseState _mouseState, int _slot)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
			{
				RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

				if (craftingType == CraftingElementType.Ingredients)
				{
					if (_mouseState == MouseState.SingleClick)
					{
						if (runtimeInventory.selectedItem == null)
						{
							if (GetItem (_slot) != null)
							{
								runtimeInventory.TransferCraftingToLocal (GetItem (_slot).recipeSlot, true);
							}
						}
						else
						{
							if (GetItem (_slot) != null)
							{
								runtimeInventory.TransferCraftingToLocal (GetItem (_slot).recipeSlot, false);
							}

							runtimeInventory.TransferLocalToCrafting (runtimeInventory.selectedItem, _slot);
						}
					}
					else if (_mouseState == MouseState.RightClick)
					{
						if (runtimeInventory.selectedItem != null)
						{
							runtimeInventory.SetNull ();
						}
					}

					PlayerMenus.ResetInventoryBoxes ();
				}
			}
		}
		

		public void ClickOutput (Menu _menu, MouseState _mouseState)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
			{
				RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

				if (items.Count > 0)
				{
					if (_mouseState == MouseState.SingleClick)
					{
						if (runtimeInventory.selectedItem == null)
						{
							// Pick up created item
							if (activeRecipe.onCreateRecipe == OnCreateRecipe.SelectItem)
							{
								runtimeInventory.PerformCrafting (activeRecipe, true);
							}
							else if (activeRecipe.onCreateRecipe == OnCreateRecipe.RunActionList)
							{
								runtimeInventory.PerformCrafting (activeRecipe, false);
								if (activeRecipe.invActionList != null && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>())
								{
									AdvGame.RunActionListAsset (activeRecipe.invActionList);
								}
							}
							else
							{
								runtimeInventory.PerformCrafting (activeRecipe, false);
							}
						}
					}
					PlayerMenus.ResetInventoryBoxes ();
				}
			}
		}


		public override void RecalculateSize ()
		{
			PopulateList ();
			base.RecalculateSize ();
		}
		
		
		private void PopulateList ()
		{
			if (Application.isPlaying)
			{
				if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
				{
					RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

					if (craftingType == CraftingElementType.Ingredients)
					{
						items = new List<InvItem>();
						foreach (InvItem _item in runtimeInventory.craftingItems)
						{
							items.Add (_item);
						}
					}
					else if (craftingType == CraftingElementType.Output)
					{
						SetOutput (true, runtimeInventory);
						return;
					}
				}
			}
			else
			{
				items = new List<InvItem>();
				return;
			}
		}


		public void SetOutput (bool autoCreate, RuntimeInventory runtimeInventory)
		{
			items = new List<InvItem>();
			activeRecipe = runtimeInventory.CalculateRecipe (autoCreate);
			if (activeRecipe != null)
			{
				foreach (InvItem assetItem in AdvGame.GetReferences ().inventoryManager.items)
				{
					if (assetItem.id == activeRecipe.resultID)
					{
						InvItem newItem = new InvItem (assetItem);
						newItem.count = 1;
						items.Add (newItem);
					}
				}
			}

			if (!autoCreate)
			{
				base.RecalculateSize ();
			}
		}

		
		private bool AreAnyItemsInWrongCategory ()
		{
			foreach (InvItem item in items)
			{
				if (item.binID != categoryID)
				{
					return true;
				}
			}
			
			return false;
		}

		
		private void DrawTexture (Rect rect, int i)
		{
			Texture2D tex = null;
			
			if (Application.isPlaying)
			{
				tex = GetItem (i).tex;
			}
			else if (items [i].tex != null)
			{
				tex = items [i].tex;
			}
			
			if (tex != null)
			{
				GUI.DrawTexture (rect, tex, ScaleMode.StretchToFill, true, 0f);
			}
		}
		
		
		public override string GetLabel (int i)
		{
			if (GetItem (i).altLabel != "")
			{
				return GetItem (i).altLabel;
			}
			
			return GetItem (i).label;
		}
		
		
		public InvItem GetItem (int i)
		{
			if (craftingType == CraftingElementType.Output)
			{
				if (items.Count > i)
				{
					return items [i];
				}
			}
			else if (craftingType == CraftingElementType.Ingredients)
			{
				foreach (InvItem _item in items)
				{
					if (_item.recipeSlot == i)
					{
						return _item;
					}
				}
			}
			return null;
		}
		
		
		private string GetCount (int i)
		{
			if (GetItem (i).count < 2)
			{
				return "";
			}
			
			return GetItem (i).count.ToString ();
		}

		
		protected override void AutoSize ()
		{
			if (items.Count > 0)
			{
				AutoSize (new GUIContent (items[0].tex));
			}
			else
			{
				AutoSize (GUIContent.none);
			}
		}
		
	}
	
}