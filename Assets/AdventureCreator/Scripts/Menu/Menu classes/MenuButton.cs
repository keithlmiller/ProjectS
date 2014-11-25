/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuButton.cs"
 * 
 *	This MenuElement can be clicked on to perform a specified function.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	[System.Serializable]
	public class MenuButton : MenuElement
	{
		
		public string label = "Element";

		public string hotspotLabel = "";
		public int hotspotLabelID = -1;

		public TextAnchor anchor;
		public TextEffects textEffects;
		public AC_ButtonClickType buttonClickType;
		public SimulateInputType simulateInput = SimulateInputType.Button;
		public float simulateValue = 1f;
		public bool doFade;
		public string switchMenuTitle;
		public string inventoryBoxTitle;
		public AC_ShiftInventory shiftInventory;
		public int shiftAmount = 1;
		public bool loopJournal = false;
		public ActionListAsset actionList;
		public string inputAxis;
		public Texture2D clickTexture;
		public bool onlyShowWhenEffective;
		public bool allowContinuousClick = false;

		private MenuElement elementToShift;
		private float clickAlpha = 0f;

		
		public override void Declare ()
		{
			label = "Button";
			hotspotLabel = "";
			hotspotLabelID = -1;
			isVisible = true;
			isClickable = true;
			textEffects = TextEffects.None;
			buttonClickType = AC_ButtonClickType.RunActionList;
			simulateInput = SimulateInputType.Button;
			simulateValue = 1f;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			doFade = false;
			switchMenuTitle = "";
			inventoryBoxTitle = "";
			shiftInventory = AC_ShiftInventory.ShiftLeft;
			loopJournal = false;
			actionList = null;
			inputAxis = "";
			clickTexture = null;
			clickAlpha = 0f;
			shiftAmount = 1;
			onlyShowWhenEffective = false;
			allowContinuousClick = false;

			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuButton newElement = CreateInstance <MenuButton>();
			newElement.Declare ();
			newElement.CopyButton (this);
			return newElement;
		}
		
		
		public void CopyButton (MenuButton _element)
		{
			label = _element.label;
			hotspotLabel = _element.hotspotLabel;
			hotspotLabelID = _element.hotspotLabelID;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			buttonClickType = _element.buttonClickType;
			simulateInput = _element.simulateInput;
			simulateValue = _element.simulateValue;
			doFade = _element.doFade;
			switchMenuTitle = _element.switchMenuTitle;
			inventoryBoxTitle = _element.inventoryBoxTitle;
			shiftInventory = _element.shiftInventory;
			loopJournal = _element.loopJournal;
			actionList = _element.actionList;
			inputAxis = _element.inputAxis;
			clickTexture = _element.clickTexture;
			clickAlpha = _element.clickAlpha;
			shiftAmount = _element.shiftAmount;
			onlyShowWhenEffective = _element.onlyShowWhenEffective;
			allowContinuousClick = _element.allowContinuousClick;
					
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				label = EditorGUILayout.TextField ("Button text:", label);
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
				hotspotLabel = EditorGUILayout.TextField ("Hotspot label override:", hotspotLabel);

				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Click texture:", GUILayout.Width (145f));
					clickTexture = (Texture2D) EditorGUILayout.ObjectField (clickTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();

				buttonClickType = (AC_ButtonClickType) EditorGUILayout.EnumPopup ("Click type:", buttonClickType);
			
				if (buttonClickType == AC_ButtonClickType.TurnOffMenu)
				{
					doFade = EditorGUILayout.Toggle ("Do transition?", doFade);
				}
				else if (buttonClickType == AC_ButtonClickType.Crossfade)
				{
					switchMenuTitle = EditorGUILayout.TextField ("Menu to switch to:", switchMenuTitle);
				}
				else if (buttonClickType == AC_ButtonClickType.OffsetInventoryOrDialogue)
				{
					inventoryBoxTitle = EditorGUILayout.TextField ("Element to affect:", inventoryBoxTitle);
					shiftInventory = (AC_ShiftInventory) EditorGUILayout.EnumPopup ("Offset type:", shiftInventory);
					shiftAmount = EditorGUILayout.IntField ("Offset amount:", shiftAmount);
					onlyShowWhenEffective = EditorGUILayout.Toggle ("Only show when effective?", onlyShowWhenEffective);
				}
				else if (buttonClickType == AC_ButtonClickType.OffsetJournal)
				{
					inventoryBoxTitle = EditorGUILayout.TextField ("Journal to affect:", inventoryBoxTitle);
					shiftInventory = (AC_ShiftInventory) EditorGUILayout.EnumPopup ("Offset type:", shiftInventory);
					loopJournal = EditorGUILayout.Toggle ("Cycle pages?", loopJournal);
				}
				else if (buttonClickType == AC_ButtonClickType.RunActionList)
				{
					actionList = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList to run:", actionList, typeof (ActionListAsset), false);
				}
				else if (buttonClickType == AC_ButtonClickType.CustomScript)
				{
					allowContinuousClick = EditorGUILayout.Toggle ("Accept held-down clicks?", allowContinuousClick);
					ShowClipHelp ();
				}
				else if (buttonClickType == AC_ButtonClickType.SimulateInput)
				{
					simulateInput = (SimulateInputType) EditorGUILayout.EnumPopup ("Simulate:", simulateInput);
					inputAxis = EditorGUILayout.TextField ("Input axis:", inputAxis);
					if (simulateInput == SimulateInputType.Axis)
					{
						simulateValue = EditorGUILayout.FloatField ("Input value:", simulateValue);
					}
				}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI ();
		}
		
		#endif


		public void ShowClick ()
		{
			if (isClickable)
			{
				clickAlpha = 1f;
			}
		}

		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (buttonClickType == AC_ButtonClickType.OffsetInventoryOrDialogue && onlyShowWhenEffective && inventoryBoxTitle != "" && Application.isPlaying)
			{
				if (elementToShift == null)
				{
					foreach (Menu _menu in PlayerMenus.GetMenus ())
					{
						if (_menu != null && _menu.elements.Contains (this))
						{
							foreach (MenuElement _element in _menu.elements)
							{
								if (_element != null && _element.title == inventoryBoxTitle)
								{
									elementToShift = _element;
									break;
								}
							}
					    }
					}
				}
				if (elementToShift != null)
				{
					if (!elementToShift.CanBeShifted (shiftInventory))
					{
						return;
					}
				}
			}

			if (clickAlpha > 0f)
			{
				if (clickTexture)
				{
					Color tempColor = GUI.color;
					tempColor.a = clickAlpha;
					GUI.color = tempColor;
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), clickTexture, ScaleMode.StretchToFill, true, 0f);
					tempColor.a = 1f;
					GUI.color = tempColor;
				}
				clickAlpha -= Time.deltaTime;
				if (clickAlpha < 0f)
				{
					clickAlpha = 0f;
				}
			}

			base.Display (_style, _slot, zoom, isActive);

			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style);
			}
		}


		public override string GetLabel (int slot)
		{
			return TranslateLabel (label);
		}

		
		protected override void AutoSize ()
		{
			if (label == "" && backgroundTexture != null)
			{
				GUIContent content = new GUIContent (backgroundTexture);
				AutoSize (content);
			}
			else
			{
				GUIContent content = new GUIContent (TranslateLabel (label));
				AutoSize (content);
			}
		}


		public override void RecalculateSize ()
		{
			clickAlpha = 0f;
			base.RecalculateSize ();
		}


		public string GetHotspotLabel ()
		{
			if (Options.GetLanguage () > 0 && hotspotLabelID > -1)
			{
				return SpeechManager.GetTranslation (hotspotLabelID, Options.GetLanguage ());
			}
			else
			{
				return hotspotLabel;
			}
		}
		
	}

}