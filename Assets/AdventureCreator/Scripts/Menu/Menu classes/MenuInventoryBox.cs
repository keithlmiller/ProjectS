/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuInventoryBox.cs"
 * 
 *	This MenuElement lists all inventory items held by the player.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuInventoryBox : MenuElement
	{
		
		public TextEffects textEffects;
		public AC_InventoryBoxType inventoryBoxType;
		public bool limitSlots;
		public int maxSlots;
		public bool limitToCategory;
		public int categoryID;
		public List<InvItem> items = new List<InvItem>();
		public bool selectItemsAfterTaking = true;
		public ConversationDisplayType displayType = ConversationDisplayType.IconOnly;

		private RuntimeInventory runtimeInventory;


		public override void Declare ()
		{
			isVisible = true;
			isClickable = true;
			inventoryBoxType = AC_InventoryBoxType.Default;
			numSlots = 0;
			SetSize (new Vector2 (6f, 10f));
			limitSlots = false;
			maxSlots = 10;
			limitToCategory = false;
			selectItemsAfterTaking = true;
			categoryID = -1;
			displayType = ConversationDisplayType.IconOnly;
			textEffects = TextEffects.None;
			items = new List<InvItem>();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuInventoryBox newElement = CreateInstance <MenuInventoryBox>();
			newElement.Declare ();
			newElement.CopyInventoryBox (this);
			return newElement;
		}
		
		
		public void CopyInventoryBox (MenuInventoryBox _element)
		{
			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			inventoryBoxType = _element.inventoryBoxType;
			numSlots = _element.numSlots;
			limitSlots = _element.limitSlots;
			maxSlots = _element.maxSlots;
			limitToCategory = _element.limitToCategory;
			categoryID = _element.categoryID;
			selectItemsAfterTaking = _element.selectItemsAfterTaking;
			displayType = _element.displayType;
			PopulateList ();

			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
				displayType = (ConversationDisplayType) EditorGUILayout.EnumPopup ("Display:", displayType);
				inventoryBoxType = (AC_InventoryBoxType) EditorGUILayout.EnumPopup ("Inventory box type:", inventoryBoxType);
				if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					limitToCategory = EditorGUILayout.Toggle ("Limit to category?", limitToCategory);
					if (limitToCategory)
					{
						if (AdvGame.GetReferences ().inventoryManager)
						{
							List<string> binList = new List<string>();
							List<InvBin> bins = AdvGame.GetReferences ().inventoryManager.bins;
							foreach (InvBin bin in bins)
							{
								binList.Add (bin.label);
							}

							EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Category:", GUILayout.Width (146f));
								if (binList.Count > 0)
								{
									int binNumber = GetBinSlot (categoryID, bins);
									binNumber = EditorGUILayout.Popup (binNumber, binList.ToArray());
									categoryID = bins[binNumber].id;
								}
								else
								{
									categoryID = -1;
									EditorGUILayout.LabelField ("No categories defined!", EditorStyles.miniLabel, GUILayout.Width (146f));
								}
							EditorGUILayout.EndHorizontal ();
						}
						else
						{
							EditorGUILayout.HelpBox ("No Inventory Manager defined!", MessageType.Warning);
							categoryID = -1;
						}
					}
					else
					{
						categoryID = -1;
					}

					limitSlots = EditorGUILayout.Toggle ("Fixed number of slots?", limitSlots);
					if (limitSlots)
					{
						maxSlots = EditorGUILayout.IntSlider ("Number slots:", maxSlots, 1, 30);
					}

					isClickable = true;
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplaySelected)
				{
					isClickable = false;
					limitSlots = true;
					maxSlots = 1;
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
				{
					isClickable = true;
					limitSlots = true;
					maxSlots = 1;
				}
				else if (inventoryBoxType == AC_InventoryBoxType.Container)
				{
					isClickable = true;
					limitSlots = EditorGUILayout.Toggle ("Fixed number of slots?", limitSlots);
					if (limitSlots)
					{
						maxSlots = EditorGUILayout.IntSlider ("Number of slots:", maxSlots, 1, 30);
					}
					selectItemsAfterTaking = EditorGUILayout.Toggle ("Select item after taking?", selectItemsAfterTaking);
				}
				else
				{
					isClickable = true;
					numSlots = EditorGUILayout.IntField ("Test slots:", numSlots);
					limitSlots = false;
					maxSlots = 10;
				}

				if (inventoryBoxType != AC_InventoryBoxType.DisplaySelected && inventoryBoxType != AC_InventoryBoxType.DisplayLastSelected)
				{
					slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
					orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
					if (orientation == ElementOrientation.Grid)
					{
						gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
					}
				}
				
				if (inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					ShowClipHelp ();
				}
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

			if (items.Count > 0 && items.Count > (_slot+offset) && items [_slot+offset] != null)
			{
				if (displayType == ConversationDisplayType.IconOnly)
				{
					GUI.Label (GetSlotRectRelative (_slot), "", _style);
					DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), items [_slot+offset], isActive);
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
				else if (displayType == ConversationDisplayType.TextOnly)
				{
					string labelText = items [_slot+offset].label;
					if (runtimeInventory != null)
					{
						labelText = runtimeInventory.GetLabel (items [_slot+offset]);
					}
					string countText = GetCount (_slot);
					if (countText != "")
					{
						labelText += " (" + countText + ")";
					}

					if (textEffects != TextEffects.None)
					{
						AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), labelText, _style, Color.black, _style.normal.textColor, 2, textEffects);
					}
					else
					{
						GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), labelText, _style);
					}
				}
			}
		}
		
		
		public void HandleDefaultClick (MouseState _mouseState, int _slot, AC_InteractionMethod interactionMethod)
		{
			if (runtimeInventory != null)
			{
				runtimeInventory.StopHighlighting ();
				runtimeInventory.SetFont (font, GetFontSize (), fontColor, textEffects);

				if (items.Count <= (_slot + offset) || items[_slot+offset] == null)
				{
					// Blank space
					runtimeInventory.MoveItemToIndex (runtimeInventory.selectedItem, runtimeInventory.localItems, _slot + offset);
					return;
				}

				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					runtimeInventory.ShowInteractions (items [_slot + offset]);
				}
				else if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					if (_mouseState == MouseState.SingleClick)
					{
						int cursorID = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>().GetSelectedCursorID ();
						int cursor = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>().GetSelectedCursor ();

						if (cursor == -2)
						{
							if (runtimeInventory.selectedItem != null && items [_slot + offset] == runtimeInventory.selectedItem)
							{
								runtimeInventory.SelectItem (items [_slot + offset]);
							}
							else
							{
								runtimeInventory.Combine (items [_slot + offset]);
							}
						}
						else if (cursor == -1)
						{
							runtimeInventory.SelectItem (items [_slot + offset]);
						}
						else if (cursorID > -1)
						{
							runtimeInventory.RunInteraction (items [_slot + offset], cursorID);
						}
					}
				}
				else
				{
					if (_mouseState == MouseState.SingleClick)
					{
						if (runtimeInventory.selectedItem == null)
						{
							runtimeInventory.Use (items [_slot + offset]);
						}
						else
						{
							runtimeInventory.Combine (items [_slot + offset]);
						}
					}
					else if (_mouseState == MouseState.RightClick)
					{
						if (runtimeInventory.selectedItem == null)
						{
							runtimeInventory.Look (items [_slot + offset]);
						}
						else
						{
							runtimeInventory.SetNull ();
						}
					}
				}
			}
		}
		
		
		public override void RecalculateSize ()
		{
			PopulateList ();

			if (inventoryBoxType == AC_InventoryBoxType.HostpotBased)
			{
				if (!Application.isPlaying)
				{
					if (numSlots < 0)
					{
						numSlots = 0;
					}
					if (numSlots > items.Count)
					{
						numSlots = items.Count;
					}
				}
				else
				{
					numSlots = items.Count;
				}
			}
			else
			{
				numSlots = items.Count;

				if (limitSlots)
				{
					numSlots = maxSlots;
				}

				LimitOffset (items.Count);
			}

			base.RecalculateSize ();
		}
		
		
		private void PopulateList ()
		{
			if (Application.isPlaying)
			{
				if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
				{
					runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

					if (inventoryBoxType == AC_InventoryBoxType.HostpotBased)
					{
						items = runtimeInventory.MatchInteractions ();
					}
					else if (inventoryBoxType == AC_InventoryBoxType.DisplaySelected)
					{
						items = runtimeInventory.GetSelected ();
					}
					else if (inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
					{
						if (runtimeInventory.selectedItem != null)
						{
							items = new List<InvItem>();
							items = runtimeInventory.GetSelected ();
						}
						else if (items.Count == 1 && !runtimeInventory.IsItemCarried (items[0]))
						{
							items.Clear ();
						}
					}
					else if (inventoryBoxType == AC_InventoryBoxType.Container)
					{
						PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
						if (playerInput.activeContainer)
						{
							items.Clear ();
							foreach (ContainerItem containerItem in playerInput.activeContainer.items)
							{
								InvItem referencedItem = new InvItem (AdvGame.GetReferences ().inventoryManager.GetItem (containerItem.linkedID));
								referencedItem.count = containerItem.count;
								items.Add (referencedItem);
							}
						}
					}
					else
					{
						items = new List<InvItem>();
						foreach (InvItem _item in runtimeInventory.localItems)
						{
							if (AdvGame.GetReferences ().settingsManager.hideSelectedFromMenu && runtimeInventory.selectedItem == _item)
							{
								items.Add (null);
							}
							else
							{
								items.Add (_item);
							}
						}
					}
				}
			}
			else
			{
				items = new List<InvItem>();
				if (AdvGame.GetReferences ().inventoryManager)
				{
					foreach (InvItem _item in AdvGame.GetReferences ().inventoryManager.items)
					{
						items.Add (_item);
						if (_item != null)
						{
							_item.recipeSlot = -1;
						}
					}
				}
			}

			if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript)
			{
				if (limitToCategory && categoryID > -1)
				{
					while (AreAnyItemsInWrongCategory ())
					{
						foreach (InvItem _item in items)
						{
							if (_item != null && _item.binID != categoryID)
							{
								items.Remove (_item);
								break;
							}
						}
					}
				}

				while (AreAnyItemsInRecipe ())
				{
					foreach (InvItem _item in items)
					{
						if (_item != null && _item.recipeSlot > -1)
						{
							if (AdvGame.GetReferences ().settingsManager.canReorderItems)
								items [items.IndexOf (_item)] = null;
							else
								items.Remove (_item);
							break;
						}
					}
				}
			}
		}


		public override bool CanBeShifted (AC_ShiftInventory shiftType)
		{
			if (shiftType == AC_ShiftInventory.ShiftLeft)
			{
				if (offset == 0)
				{
					return false;
				}
			}
			else
			{
				if ((maxSlots + offset) >= items.Count)
				{
					return false;
				}
			}
			return true;
		}


		private bool AreAnyItemsInRecipe ()
		{
			foreach (InvItem item in items)
			{
				if (item != null && item.recipeSlot >= 0)
				{
					return true;
				}
			}
			return false;
		}


		private bool AreAnyItemsInWrongCategory ()
		{
			foreach (InvItem item in items)
			{
				if (item != null && item.binID != categoryID)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public override void Shift (AC_ShiftInventory shiftType, int amount)
		{
			if (limitSlots)
			{
				if (numSlots >= maxSlots)
				{
					Shift (shiftType, maxSlots, items.Count, amount);
				}
			}
			else
			{
				Debug.Log ("Cannot offset " + title + " as it does not limit the number of available slots.");
			}
		}


		private void DrawTexture (Rect rect, InvItem _item, bool isActive)
		{
			if (_item == null) return;

			Texture2D tex = null;
			if (Application.isPlaying && runtimeInventory != null && inventoryBoxType != AC_InventoryBoxType.DisplaySelected)
			{
				if (_item == runtimeInventory.highlightItem && _item.activeTex != null)
				{
					runtimeInventory.DrawHighlighted (rect);
					return;
				}

				if (_item.activeTex != null && ((isActive && AdvGame.GetReferences ().settingsManager.activeWhenHover) || _item == runtimeInventory.selectedItem))
				{
					tex = _item.activeTex;
				}
				else if (_item.tex != null)
				{
					tex = _item.tex;
				}
			}
			else if (_item.tex != null)
			{
				tex = _item.tex;
			}

			if (tex != null)
			{
				GUI.DrawTexture (rect, tex, ScaleMode.StretchToFill, true, 0f);
			}
		}


		public override string GetLabel (int i)
		{
			if (items.Count <= (i+offset) || items [i+offset] == null)
			{
				return null;
			}

			return items [i+offset].GetLabel ();
		}


		public InvItem GetItem (int i)
		{
			if (items.Count <= (i+offset) || items [i+offset] == null)
			{
				return null;
			}

			return items [i+offset];
		}


		private string GetCount (int i)
		{
			if (items.Count <= (i+offset) || items [i+offset] == null)
			{
				return "";
			}

			if (items [i+offset].count < 2)
			{
				return "";
			}
			return items [i + offset].count.ToString ();
		}


		public void ResetOffset ()
		{
			offset = 0;
		}
		
		
		protected override void AutoSize ()
		{
			if (items.Count > 0)
			{
				if (displayType == ConversationDisplayType.IconOnly)
				{
					AutoSize (new GUIContent (items[0].tex));
				}
				else if (displayType == ConversationDisplayType.TextOnly)
				{
					AutoSize (new GUIContent (items[0].label));
				}
			}
			else
			{
				AutoSize (GUIContent.none);
			}
		}


		public void ClickContainer (MouseState _mouseState, int _slot, Container container)
		{
			if (container == null || runtimeInventory == null) return;

			runtimeInventory.SetFont (font, GetFontSize (), fontColor, textEffects);

			if (_mouseState == MouseState.SingleClick)
			{
				if (runtimeInventory.selectedItem == null)
				{
					if (container.items [_slot+offset] != null)
					{
						ContainerItem containerItem = container.items [_slot + offset];
						runtimeInventory.Add (containerItem.linkedID, containerItem.count, selectItemsAfterTaking);
						container.items.Remove (containerItem);
					}
				}
				else
				{
					// Placing an item inside the container
					container.InsertAt (runtimeInventory.selectedItem, _slot+offset);
					runtimeInventory.Remove (runtimeInventory.selectedItem);
				}
			}

			else if (_mouseState == MouseState.RightClick)
			{
				if (runtimeInventory.selectedItem != null)
				{
					runtimeInventory.SetNull ();
				}
			}
		}

	}

}