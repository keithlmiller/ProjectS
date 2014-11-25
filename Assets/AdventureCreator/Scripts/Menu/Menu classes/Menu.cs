/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Menu.cs"
 * 
 *	This script is a container of MenuElement subclasses, which together make up a menu.
 *	When menu elements are added, this script updates the size, positioning etc automatically.
 *	The handling of menu visibility, element clicking, etc is all handled in MenuSystem,
 *	rather than the Menu class itself.
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
	public class Menu : ScriptableObject
	{
		public bool isEditing = false;
		public bool isLocked = false;
		public int id;
		public string title;
		public Vector2 manualSize = Vector2.zero;
		public AC_PositionType positionType = AC_PositionType.Centred;
		public Vector2 manualPosition = Vector2.zero;
		public TextAnchor alignment = TextAnchor.MiddleCenter;
		public string toggleKey = "";
		public bool ignoreMouseClicks = false;
		public bool pauseWhenEnabled = false;
		public bool enabledOnStart = false;
		public ActionListAsset actionListOnTurnOn = null;
		
		public Texture2D backgroundTexture;
		public Texture2D sliderTexture;
		
		public List<MenuElement> visibleElements = new List<MenuElement>();
		public float transitionProgress = 0f;
		public AppearType appearType;
		public SpeechMenuType speechMenuType = SpeechMenuType.All;
		
		public MenuElement selected_element;
		public int selected_slot = 0;
		
		public List<MenuElement> elements = new List<MenuElement>();
		
		[SerializeField] private Vector2 biggestElementSize;
		
		public float spacing;
		private bool isEnabled;
		public AC_SizeType sizeType;
		
		public MenuOrientation orientation;
		[SerializeField] private Rect rect = new Rect ();
		
		public MenuTransition transitionType = MenuTransition.Fade;
		public PanDirection panDirection = PanDirection.Up;
		public PanMovement panMovement = PanMovement.Linear;
		public float panDistance = 0.5f;
		public float fadeSpeed = 0.5f;
		public TextAnchor zoomAnchor = TextAnchor.MiddleCenter;
		private bool isFading = false;
		private FadeType fadeType = FadeType.fadeIn;
		private Vector2 panOffset = Vector2.zero;
		private Vector2 dragOffset = Vector2.zero;
		private float zoomAmount = 1f;
		public bool zoomElements = false;
		private Rect aspectCorrectedRect = new Rect ();

		private PlayerMenus playerMenus;
		
		
		public void Declare (int[] idArray)
		{
			spacing = 0.5f;
			orientation = MenuOrientation.Vertical;
			appearType = AppearType.Manual;
			
			elements = new List<MenuElement>();
			visibleElements = new List<MenuElement>();
			enabledOnStart = false;
			isEnabled = false;
			sizeType = AC_SizeType.Automatic;
			speechMenuType = SpeechMenuType.All;
			actionListOnTurnOn = null;
			
			fadeSpeed = 0.5f;
			transitionType = MenuTransition.Fade;
			panDirection = PanDirection.Up;
			panMovement = PanMovement.Linear;
			panDistance = 0.5f;
			zoomAnchor = TextAnchor.MiddleCenter;
			zoomElements = false;
			ignoreMouseClicks = false;
			
			pauseWhenEnabled = false;
			id = 0;
			isLocked = false;
			
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
				{
					id ++;
				}
			}
			
			title = "Menu " + (id + 1).ToString ();
		}
		
		
		public void Copy (Menu _menu)
		{
			isEditing = false;
			id = _menu.id;
			isLocked = _menu.isLocked;
			title = _menu.title;
			manualSize = _menu.manualSize;
			positionType = _menu.positionType;
			manualPosition = _menu.manualPosition;
			alignment = _menu.alignment;
			toggleKey = _menu.toggleKey;
			backgroundTexture = _menu.backgroundTexture;
			sliderTexture = _menu.sliderTexture;
			visibleElements = new List<MenuElement>();
			transitionProgress = 0f;
			appearType = _menu.appearType;
			selected_element = null;
			selected_slot = 0;
			spacing = _menu.spacing;
			sizeType = _menu.sizeType;
			orientation = _menu.orientation;
			fadeSpeed = _menu.fadeSpeed;
			transitionType = _menu.transitionType;
			panDirection = _menu.panDirection;
			panMovement = _menu.panMovement;
			panDistance = _menu.panDistance;
			zoomAnchor = _menu.zoomAnchor;
			zoomElements = _menu.zoomElements;
			pauseWhenEnabled = _menu.pauseWhenEnabled;
			speechMenuType = _menu.speechMenuType;
			enabledOnStart = _menu.enabledOnStart;
			actionListOnTurnOn = _menu.actionListOnTurnOn;
			ignoreMouseClicks = _menu.ignoreMouseClicks;
			
			elements = new List<MenuElement>();
			foreach (MenuElement _element in _menu.elements)
			{
				elements.Add (_element.DuplicateSelf ());
			}
			
			Recalculate ();
			
			if (appearType == AppearType.Manual && _menu.enabledOnStart)
			{
				transitionProgress = 1f;
				TurnOn (false);
			}
			else
			{
				transitionProgress = 0f;
				TurnOff (false);
			}
			if (transitionType == MenuTransition.Zoom)
			{
				zoomAmount = 0f;
			}
			
			UpdateTransition ();
		}		
		
		
		#if UNITY_EDITOR
		
		public void ShowGUI ()
		{
			title = EditorGUILayout.TextField ("Menu name:", title);
			isLocked = EditorGUILayout.Toggle ("Start game locked off?", isLocked);
			ignoreMouseClicks = EditorGUILayout.Toggle ("Ignore Cursor clicks?", ignoreMouseClicks);
			actionListOnTurnOn = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList when turn on:", actionListOnTurnOn, typeof (ActionListAsset), false);
			
			appearType = (AppearType) EditorGUILayout.EnumPopup ("Appear type:", appearType);
			if (appearType == AppearType.OnInputKey)
			{
				toggleKey = EditorGUILayout.TextField ("Toggle key:", toggleKey);
			}
			if (appearType == AppearType.Manual || appearType == AppearType.OnInputKey)
			{
				if (appearType == AppearType.Manual)
				{
					enabledOnStart = EditorGUILayout.Toggle ("Enabled on start?", enabledOnStart);
				}
				pauseWhenEnabled = EditorGUILayout.Toggle ("Pause game when enabled?", pauseWhenEnabled);
			}
			else if (appearType == AppearType.OnInteraction || appearType == AppearType.OnContainer)
			{
				pauseWhenEnabled = EditorGUILayout.Toggle ("Pause game when enabled?", pauseWhenEnabled);
			}
			else if (appearType == AppearType.WhenSpeechPlays)
			{
				speechMenuType = (SpeechMenuType) EditorGUILayout.EnumPopup ("Display speech of type:", speechMenuType);
			}
			
			spacing = EditorGUILayout.Slider ("Spacing (%):", spacing, 0f, 10f);
			orientation = (MenuOrientation) EditorGUILayout.EnumPopup ("Element orientation:", orientation);
			
			positionType = (AC_PositionType) EditorGUILayout.EnumPopup ("Position:", positionType);
			if (positionType == AC_PositionType.Aligned)
			{
				alignment = (TextAnchor) EditorGUILayout.EnumPopup ("Alignment:", alignment);
			}
			else if (positionType == AC_PositionType.Manual || positionType == AC_PositionType.FollowCursor || positionType == AC_PositionType.AppearAtCursorAndFreeze || positionType == AC_PositionType.OnHotspot || positionType == AC_PositionType.AboveSpeakingCharacter || positionType == AC_PositionType.AbovePlayer)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("X:", GUILayout.Width (20f));
				manualPosition.x = EditorGUILayout.Slider (manualPosition.x, 0f, 100f);
				EditorGUILayout.LabelField ("Y:", GUILayout.Width (20f));
				manualPosition.y = EditorGUILayout.Slider (manualPosition.y, 0f, 100f);
				EditorGUILayout.EndHorizontal ();
			}
			
			sizeType = (AC_SizeType) EditorGUILayout.EnumPopup ("Size:", sizeType);
			if (sizeType == AC_SizeType.Manual)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("W:", GUILayout.Width (15f));
				manualSize.x = EditorGUILayout.Slider (manualSize.x, 0f, 100f);
				EditorGUILayout.LabelField ("H:", GUILayout.Width (15f));
				manualSize.y = EditorGUILayout.Slider (manualSize.y, 0f, 100f);
				EditorGUILayout.EndHorizontal ();
			}
			else if (sizeType == AC_SizeType.Absolute)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Width:", GUILayout.Width (50f));
				manualSize.x = EditorGUILayout.FloatField (manualSize.x);
				EditorGUILayout.LabelField ("Height:", GUILayout.Width (50f));
				manualSize.y = EditorGUILayout.FloatField (manualSize.y);
				EditorGUILayout.EndHorizontal ();
			}
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Background texture:", GUILayout.Width (145f));
			backgroundTexture = (Texture2D) EditorGUILayout.ObjectField (backgroundTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
			EditorGUILayout.EndHorizontal ();
			
			transitionType = (MenuTransition) EditorGUILayout.EnumPopup ("Transition type:", transitionType);
			if (transitionType == MenuTransition.Pan || transitionType == MenuTransition.FadeAndPan)
			{
				panDirection = (PanDirection) EditorGUILayout.EnumPopup ("Pan from:", panDirection);
				panMovement= (PanMovement) EditorGUILayout.EnumPopup ("Pan movement:", panMovement);
				panDistance = EditorGUILayout.Slider ("Pan distance:", panDistance, 0f, 1f);
			}
			else if (transitionType == MenuTransition.Zoom)
			{
				zoomAnchor = (TextAnchor) EditorGUILayout.EnumPopup ("Zoom from:", zoomAnchor);
				zoomElements = EditorGUILayout.Toggle ("Adjust elements?", zoomElements);
			}
			if (transitionType != MenuTransition.None)
			{
				fadeSpeed = EditorGUILayout.Slider ("Transition speed:", fadeSpeed, 0f, 1f);
			}
		}
		
		#endif
		
		
		public void DrawOutline (MenuElement _selectedElement)
		{
			DrawStraightLine.DrawBox (rect, Color.yellow, 1f, false);
			
			foreach (MenuElement element in visibleElements)
			{
				if (element == _selectedElement)
				{
					element.DrawOutline (true, this);
				}
				{
					element.DrawOutline (false, this);
				}
			}
		}
		
		
		public void StartDisplay ()
		{
			if (isFading)
			{
				GUI.BeginGroup (new Rect (dragOffset.x + panOffset.x + GetRect ().x, dragOffset.y + panOffset.y + GetRect ().y, GetRect ().width * zoomAmount, GetRect ().height * zoomAmount));
			}
			else
			{
				GUI.BeginGroup (new Rect (dragOffset.x + GetRect ().x, dragOffset.y + GetRect ().y, GetRect ().width * zoomAmount, GetRect ().height * zoomAmount));
			}
			
			if (backgroundTexture)
			{
				Rect texRect = new Rect (0f, 0f, rect.width, rect.height);
				GUI.DrawTexture (texRect, backgroundTexture, ScaleMode.StretchToFill, true, 0f);
			}
		}
		
		
		public void EndDisplay ()
		{
			GUI.EndGroup ();
		}
		
		
		public void SetPosition (Vector2 _position)
		{
			rect.x = _position.x * AdvGame.GetMainGameViewSize ().x;
			rect.y = _position.y * AdvGame.GetMainGameViewSize ().y;
			
			FitMenuInsideScreen ();
			UpdateAspectRect ();
		}
		
		
		public void SetCentre (Vector2 _position)
		{
			Vector2 centre = new Vector2 (_position.x * AdvGame.GetMainGameViewSize ().x, _position.y * AdvGame.GetMainGameViewSize ().y);
			
			rect.x = centre.x - (rect.width / 2);
			rect.y = centre.y - (rect.height / 2);
			
			FitMenuInsideScreen ();
			UpdateAspectRect ();
		}
		
		
		private Vector2 GetCentre ()
		{
			Vector2 centre = Vector2.zero;
			
			centre.x = (rect.x + (rect.width / 2)) / AdvGame.GetMainGameViewSize ().x * 100f;
			centre.y = (rect.y + (rect.height / 2)) / AdvGame.GetMainGameViewSize ().y * 100f;
			
			return centre;
		}
		
		
		private void FitMenuInsideScreen ()
		{
			if (rect.x < 0f)
			{
				rect.x = 0f;
			}
			
			if (rect.y < 0f)
			{
				rect.y = 0f;
			}
			
			if ((rect.x + rect.width) > AdvGame.GetMainGameViewSize ().x)
			{
				rect.x = AdvGame.GetMainGameViewSize ().x - rect.width;
			}
			
			if ((rect.y + rect.height) > AdvGame.GetMainGameViewSize ().y)
			{
				rect.y = AdvGame.GetMainGameViewSize ().y - rect.height;
			}
		}
		
		
		public void Align (TextAnchor _anchor)
		{
			// X
			if (_anchor == TextAnchor.LowerLeft || _anchor == TextAnchor.MiddleLeft || _anchor == TextAnchor.UpperLeft)
			{
				rect.x = 0;
			}
			else if (_anchor == TextAnchor.LowerCenter || _anchor == TextAnchor.MiddleCenter || _anchor == TextAnchor.UpperCenter)
			{
				rect.x = (AdvGame.GetMainGameViewSize ().x - rect.width) / 2;
			}
			else
			{
				rect.x = AdvGame.GetMainGameViewSize ().x - rect.width;
			}
			
			// Y
			if (_anchor == TextAnchor.LowerLeft || _anchor == TextAnchor.LowerCenter || _anchor == TextAnchor.LowerRight)
			{
				rect.y = AdvGame.GetMainGameViewSize ().y - rect.height;
			}
			else if (_anchor == TextAnchor.MiddleLeft || _anchor == TextAnchor.MiddleCenter || _anchor == TextAnchor.MiddleRight)
			{
				rect.y = (AdvGame.GetMainGameViewSize ().y - rect.height) / 2;
			}
			else
			{
				rect.y = 0;
			}
		}
		
		
		private void SetManualSize (Vector2 _size)
		{
			rect.width = _size.x * AdvGame.GetMainGameViewSize ().x;
			rect.height = _size.y * AdvGame.GetMainGameViewSize ().y;
		}
		
		
		public Rect GetRect ()
		{
			if (!Application.isPlaying)
			{
				return MainCamera._LimitMenuToAspect (rect);
			}
			
			if (aspectCorrectedRect == new Rect ())
			{
				UpdateAspectRect ();
			}
			
			return aspectCorrectedRect;
		}
		
		
		private void UpdateAspectRect ()
		{
			// This used to be called every GetRect (), but is now only done when the menu changes position
			aspectCorrectedRect = MainCamera._LimitMenuToAspect (rect);
		}
		
		
		public bool IsPointerOverSlot (MenuElement _element, int slot, Vector2 _pointer) 
		{
			Rect rectRelative = _element.GetSlotRectRelative (slot);
			Rect rectAbsolute = GetRectAbsolute (rectRelative);
			return (rectAbsolute.Contains (_pointer));
		}
		
		
		public Rect GetRectAbsolute (Rect _rectRelative)
		{
			Rect RectAbsolute = new Rect (_rectRelative.x + dragOffset.x + GetRect ().x, _rectRelative.y + dragOffset.y + GetRect ().y, _rectRelative.width, _rectRelative.height);
			
			return (RectAbsolute);
		}
		
		
		public void ResetVisibleElements ()
		{
			visibleElements.Clear ();
			foreach (MenuElement element in elements)
			{
				if (element.isVisible)
				{
					visibleElements.Add (element);
				}
			}
		}
		
		
		public void Recalculate ()
		{
			ResetVisibleElements ();
			PositionElements ();
			
			if (sizeType == AC_SizeType.Automatic)
			{
				AutoResize ();
			}
			else if (sizeType == AC_SizeType.Manual)
			{
				SetManualSize (new Vector2 (manualSize.x / 100f, manualSize.y / 100f));
			}
			else if (sizeType == AC_SizeType.Absolute)
			{
				rect.width = manualSize.x;
				rect.height = manualSize.y;
			}
			
			if (positionType == AC_PositionType.Centred)
			{
				Centre ();
				manualPosition = GetCentre ();
			}
			else if (positionType == AC_PositionType.Aligned)
			{
				Align (alignment);
				manualPosition = GetCentre ();
			}
			else if (positionType == AC_PositionType.Manual || !Application.isPlaying)
			{
				SetCentre (new Vector2 (manualPosition.x / 100f, manualPosition.y / 100f));
			}
		}
		
		
		public void AutoResize ()
		{
			visibleElements.Clear ();
			biggestElementSize = new Vector2 ();
			
			foreach (MenuElement element in elements)
			{
				if (element != null)
				{
					element.RecalculateSize ();
					
					if (element.isVisible)
					{
						visibleElements.Add (element);
						
						if (element.GetSizeFromCorner ().x > biggestElementSize.x)
						{
							biggestElementSize.x = element.GetSizeFromCorner ().x;
						}
						
						if (element.GetSizeFromCorner ().y > biggestElementSize.y)
						{
							biggestElementSize.y = element.GetSizeFromCorner ().y;
						}
					}
				}
			}
			
			rect.width = (spacing / 100 * AdvGame.GetMainGameViewSize ().x) + biggestElementSize.x;
			rect.height = (spacing / 100 * AdvGame.GetMainGameViewSize ().x) + biggestElementSize.y;
			
			manualSize = new Vector2 (rect.width * 100f / AdvGame.GetMainGameViewSize ().x, rect.height * 100f / AdvGame.GetMainGameViewSize ().y);
		}
		
		
		private void PositionElements ()
		{
			float totalLength = 0f;
			
			foreach (MenuElement element in visibleElements)
			{
				if (element == null)
				{
					Debug.Log ("Null element found");
					break;
				}
				
				if (element.positionType == AC_PositionType2.RelativeToMenuSize && sizeType == AC_SizeType.Automatic)
				{
					Debug.LogError ("Menu " + title + " cannot display because it's size is Automatic, while it's Element " + element.title + "'s Position is set to Relative");
					return;
				}
				
				element.RecalculateSize ();
				
				if (element.positionType == AC_PositionType2.RelativeToMenuSize)
				{
					element.SetRelativePosition (new Vector2 (rect.width / 100f, rect.height / 100f));
				}
				else if (orientation == MenuOrientation.Horizontal)
				{
					if (element.positionType == AC_PositionType2.Aligned)
					{
						element.SetPosition (new Vector2 ((spacing / 100 * AdvGame.GetMainGameViewSize ().x) + totalLength, (spacing / 100 * AdvGame.GetMainGameViewSize ().x)));
					}
					
					totalLength += element.GetSize().x + (spacing / 100 * AdvGame.GetMainGameViewSize ().x);
				}
				else
				{
					if (element.positionType == AC_PositionType2.Aligned)
					{
						element.SetPosition (new Vector2 ((spacing / 100 * AdvGame.GetMainGameViewSize ().x), (spacing / 100 * AdvGame.GetMainGameViewSize ().x) + totalLength));
					}
					
					totalLength += element.GetSize().y + (spacing / 100 * AdvGame.GetMainGameViewSize ().x);
				}
			}
		}
		
		
		public void Centre ()
		{
			SetCentre (new Vector2 (0.5f, 0.5f));
		}
		
		
		public bool IsEnabled ()
		{
			if (isLocked)
			{
				if (isFading && fadeType == FadeType.fadeOut)
				{
					return isEnabled;
				}
				
				return false;
			}
			
			return (isEnabled);
		}
		
		
		public bool IsVisible ()
		{
			if (transitionProgress == 1f && isEnabled)
			{
				return true;
			}
			
			return false;
		}
		
		
		public void HandleTransition ()
		{
			if (isFading && isEnabled)
			{
				if (fadeType == FadeType.fadeIn)
				{
					transitionProgress += (0.2f * fadeSpeed);
					
					if (transitionProgress > 1f)
					{
						transitionProgress = 1f;
						UpdateTransition ();
						EndTransitionOn ();
						return;
					}
					else
					{
						UpdateTransition ();
					}
				}
				else
				{
					transitionProgress -= (0.2f * fadeSpeed);
					
					if (transitionProgress < 0f)
					{
						transitionProgress = 0f;
						UpdateTransition ();
						EndTransitionOff ();
						return;
					}
					else
					{
						UpdateTransition ();
					}
				}
			}
		}
		
		
		private void EndTransitionOn ()
		{
			transitionProgress = 1f;
			isEnabled = true;
			isFading = false;
		}
		
		
		private void EndTransitionOff ()
		{
			transitionProgress = 0f;
			isFading = false;
			isEnabled = false;
			ReturnGameState ();
			
			PlayerMenus playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
			playerMenus.CheckCrossfade (this);
		}
		
		
		public bool IsOn ()
		{
			if (!isLocked && isEnabled && !isFading)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsOff ()
		{
			if (isLocked)
			{
				return true;
			}
			if (!isEnabled)
			{
				return true;
			}
			return false;
		}
		
		
		public void TurnOn (bool doFade)
		{
			if (IsOn ())
			{
				return;
			}

			if (actionListOnTurnOn != null)
			{
				AdvGame.RunActionListAsset (actionListOnTurnOn);
			}
			
			if (transitionType == MenuTransition.None)
			{
				doFade = false;
			}

			if (playerMenus == null)
			{
				playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
			}
			playerMenus.UpdateMenuPosition (this, Vector2.zero);
			
			// Setting selected_slot to -2 will cause PlayerInput's selected_option to reset
			if (isLocked)
			{
				Debug.Log ("Cannot turn on menu " + title + " as it is locked.");
			}
			else if (!isEnabled || (isFading && fadeType == FadeType.fadeOut && appearType == AppearType.OnHotspot))
			{
				if (positionType == AC_PositionType.AppearAtCursorAndFreeze)
				{
					if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
					{
						PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
						SetCentre (new Vector2 ((playerInput.GetInvertedMouse ().x / Screen.width) + ((manualPosition.x - 50f) / 100f),
						                        (playerInput.GetInvertedMouse ().y / Screen.height) + ((manualPosition.y - 50f) / 100f)));
					}
				}
				
				selected_slot = -2;
				
				MenuSystem.OnMenuEnable (this);
				ChangeGameState ();
				Recalculate ();
				
				dragOffset = Vector2.zero;
				isEnabled = true;
				isFading = doFade;
				
				if (doFade && fadeSpeed > 0f)
				{
					fadeType = FadeType.fadeIn;
				}
				else
				{
					transitionProgress = 1f;
					isEnabled = true;
					isFading = false;
				}
			}
		}
		
		
		public bool IsFading ()
		{
			return isFading;
		}
		
		
		public void TurnOff (bool doFade)
		{
			if (IsOff ())
			{
				return;
			}

			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
			{
				PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
				if (appearType == AppearType.OnContainer)
				{
					playerInput.activeContainer = null;
				}
			}
			
			if (transitionType == MenuTransition.None)
			{
				doFade = false;
			}
			if (isEnabled && (!isFading || (isFading && fadeType == FadeType.fadeIn && appearType == AppearType.OnHotspot)))
			{
				isFading = doFade;
				
				if (doFade && fadeSpeed > 0f)
				{
					fadeType = FadeType.fadeOut;
				}
				else
				{
					UpdateTransition ();
					isFading = false;
					isEnabled = false;
					ReturnGameState ();
				}
			}
		}
		
		
		public void ForceOff ()
		{
			if (isEnabled || isFading)
			{
				transitionProgress = 0f;
				UpdateTransition ();
				isFading = false;
				isEnabled = false;
			}
		}
		
		
		public void UpdateTransition ()
		{
			if (transitionType == MenuTransition.Fade)
			{
				return;
			}
			
			if (transitionType == MenuTransition.FadeAndPan || transitionType == MenuTransition.Pan)
			{
				float amount = 0f;
				
				if (panMovement == PanMovement.Linear)
				{
					amount = (1f - transitionProgress) * panDistance;
				}
				if (panMovement == PanMovement.Smooth)
				{
					amount = ((transitionProgress * transitionProgress) - (2 * transitionProgress) + 1) * panDistance;
				}
				else if (panMovement == PanMovement.Overshoot)
				{
					amount = ((4f / 3f * transitionProgress * transitionProgress) - (7f / 3f * transitionProgress) + 1) * panDistance;
				}
				
				if (panDirection == PanDirection.Down)
				{
					panOffset = new Vector2 (0f, amount);
				}
				else if (panDirection == PanDirection.Left)
				{
					panOffset = new Vector2 (-amount, 0f);
				}
				else if (panDirection == PanDirection.Up)
				{
					panOffset = new Vector2 (0f, -amount);
				}
				else if (panDirection == PanDirection.Right)
				{
					panOffset = new Vector2 (amount, 0f);
				}
				
				panOffset = new Vector2 (panOffset.x * AdvGame.GetMainGameViewSize ().x, panOffset.y * AdvGame.GetMainGameViewSize ().y);
			}
			
			else if (transitionType == MenuTransition.Zoom)
			{
				zoomAmount = transitionProgress;
				
				if (zoomAnchor == TextAnchor.UpperLeft)
				{
					panOffset = Vector2.zero;
				}
				else if (zoomAnchor == TextAnchor.UpperCenter)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width / 2f, 0f);
				}
				else if (zoomAnchor == TextAnchor.UpperRight)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width, 0f);
				}
				else if (zoomAnchor == TextAnchor.MiddleLeft)
				{
					panOffset = new Vector2 (0f, (1f - zoomAmount) * rect.height / 2f);
				}
				else if (zoomAnchor == TextAnchor.MiddleCenter)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width / 2f, (1f - zoomAmount) * rect.height / 2f);
				}
				else if (zoomAnchor == TextAnchor.MiddleRight)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width, (1f - zoomAmount) * rect.height / 2f);
				}
				else if (zoomAnchor == TextAnchor.LowerLeft)
				{
					panOffset = new Vector2 (0, (1f - zoomAmount) * rect.height);
				}
				else if (zoomAnchor == TextAnchor.LowerCenter)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width / 2f, (1f - zoomAmount) * rect.height);
				}
				else if (zoomAnchor == TextAnchor.LowerRight)
				{
					panOffset = new Vector2 ((1f - zoomAmount) * rect.width, (1f - zoomAmount) * rect.height);
				}
			}
		}
		
		
		private void ChangeGameState ()
		{
			if (IsBlocking () && Application.isPlaying)
			{
				if (appearType != AppearType.OnInteraction)
				{
					GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>().DisableHotspot (true);
				}
				KickStarter.mainCamera.FadeIn (0);
				GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>().gameState = GameState.Paused;
			}
		}
		
		
		private void ReturnGameState ()
		{
			if (IsBlocking () && Application.isPlaying)
			{
				GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>().RestoreLastNonPausedState ();
			}
		}
		
		
		public bool IsBlocking ()
		{
			if (pauseWhenEnabled && (appearType == AppearType.Manual || appearType == AppearType.OnInputKey || appearType == AppearType.OnContainer))
			{
				return true;
			}
			return false;
		}
		
		
		public void MatchInteractions (List<Button> buttons)
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					MenuInteraction interaction = (MenuInteraction) element;
					interaction.MatchInteractions (buttons);
				}
				else if (element is MenuInventoryBox)
				{
					Recalculate ();
					element.AutoSetVisibility ();
				}
			}
			
			Recalculate ();
			Recalculate ();
		}
		
		
		public void MatchInteractions (InvItem item)
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					MenuInteraction interaction = (MenuInteraction) element;
					interaction.MatchInteractions (item);
				}
				else if (element is MenuInventoryBox)
				{
					Recalculate ();
					element.AutoSetVisibility ();
				}
			}
			
			Recalculate ();
			Recalculate ();
		}
		
		
		public void HideInteractions ()
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					element.isVisible = false;
					element.isClickable = false; // This function is only called for Context-Sensitive anyway
				}
			}
		}
		
		
		public void MatchLookInteraction (Button button)
		{
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					MenuInteraction interaction = (MenuInteraction) element;
					interaction.MatchLookInteraction (cursorManager.lookCursor_ID);
				}
			}
		}
		
		
		public void MatchUseInteraction (Button button)
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					MenuInteraction interaction = (MenuInteraction) element;
					interaction.MatchUseInteraction (button);
				}
			}
		}
		
		
		public void SetDragOffset (Vector2 pos, Rect dragRect)
		{
			if (pos.x < dragRect.x)
			{
				pos.x = dragRect.x;
			}
			else if (pos.x > (dragRect.x + dragRect.width - GetRect ().width))
			{
				pos.x = dragRect.x + dragRect.width - GetRect ().width;
			}
			
			if (pos.y < dragRect.y)
			{
				pos.y = dragRect.y;
			}
			else if (pos.y > (dragRect.y + dragRect.height - GetRect ().height))
			{
				pos.y = dragRect.y + dragRect.height - GetRect ().height;
			}
			
			dragOffset = pos;
			
			UpdateAspectRect ();
		}
		
		
		public Vector2 GetDragStart ()
		{
			return dragOffset;
		}
		
		
		public float GetZoom ()
		{
			if (transitionType == MenuTransition.Zoom && zoomElements)
			{
				return zoomAmount;
			}
			
			else return 1f;
		}
		
		
		public int ControlSelected (int selected_option)
		{
			if (selected_slot == -2)
			{
				selected_option = 0;
			}
			
			if (selected_option < 0)
			{
				selected_option = 0;
				selected_element = visibleElements[0];
				selected_slot = 0;
			}
			else
			{
				int sel = 0;
				selected_slot = -1;
				int element = 0;
				int slot = 0;
				
				for (element=0; element<visibleElements.Count; element++)
				{
					if (visibleElements[element].isClickable)
					{
						for (slot=0; slot<visibleElements[element].GetNumSlots (); slot++)
						{
							if (selected_option == sel)
							{
								selected_slot = slot;
								selected_element = visibleElements[element];
								break;
							}
							sel++;
						}
					}
					
					if (selected_slot != -1)
					{
						break;
					}
				}
				
				if (selected_slot == -1)
				{
					// Couldn't find match, must've maxed out
					selected_slot = slot - 1;
					selected_element = visibleElements[element-1];
					selected_option = sel - 1;
				}
			}
			
			return selected_option;
		}
		
		
		public MenuElement GetElementWithName (string menuElementName)
		{
			foreach (MenuElement menuElement in elements)
			{
				if (menuElement.title == menuElementName)
				{
					return menuElement;
				}
			}
			
			return null;
		}
		
		
		public Vector2 GetSlotCentre (MenuElement _element, int slot)
		{
			foreach (MenuElement menuElement in elements)
			{
				if (menuElement == _element)
				{
					Rect slotRect = _element.GetSlotRectRelative (slot);
					return new Vector2 (GetRect ().x + slotRect.x + (slotRect.width / 2f), GetRect ().y + slotRect.y + (slotRect.height / 2f));
				}
			}
			
			return Vector2.zero;
		}
		
	}
	
}