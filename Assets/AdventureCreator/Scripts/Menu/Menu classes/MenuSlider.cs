/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuSlider.cs"
 * 
 *	This MenuElement creates a slider for eg. volume control.
 * 
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuSlider : MenuElement
	{
		
		public string label;
		public TextEffects textEffects;
		public float amount;
		public TextAnchor anchor;
		public Texture2D sliderTexture;
		public SliderDisplayType sliderDisplayType = SliderDisplayType.FillBar;
		public AC_SliderType sliderType;
		public Vector2 blockSize = new Vector2 (0.05f, 1f);
		public bool useFullWidth = false;
		public int varID;
		public int numberOfSteps = 0;

		private Rect sliderRect;

		
		public override void Declare ()
		{
			label = "Slider";
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			amount = 1f;
			anchor = TextAnchor.MiddleLeft;
			sliderType = AC_SliderType.CustomScript;
			sliderDisplayType = SliderDisplayType.FillBar;
			blockSize = new Vector2 (0.05f, 1f);
			useFullWidth = false;
			varID = 0;
			textEffects = TextEffects.None;
			numberOfSteps = 0;
			
			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuSlider newElement = CreateInstance <MenuSlider>();
			newElement.Declare ();
			newElement.CopySlider (this);
			return newElement;
		}
		
		
		public void CopySlider (MenuSlider _element)
		{
			label = _element.label;
			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			amount = _element.amount;
			anchor = _element.anchor;
			sliderTexture = _element.sliderTexture;
			sliderType = _element.sliderType;
			sliderDisplayType = _element.sliderDisplayType;
			blockSize = _element.blockSize;
			useFullWidth = _element.useFullWidth;
			varID = _element.varID;
			numberOfSteps = _element.numberOfSteps;

			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				label = EditorGUILayout.TextField ("Label text:", label);
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
				useFullWidth = EditorGUILayout.Toggle ("Use full width?", useFullWidth);
				sliderDisplayType = (SliderDisplayType) EditorGUILayout.EnumPopup ("Display type:", sliderDisplayType);

				EditorGUILayout.BeginHorizontal ();
					if (sliderDisplayType == SliderDisplayType.FillBar)
					{
						EditorGUILayout.LabelField ("Fill-bar texture:", GUILayout.Width (145f));
					}
					else if (sliderDisplayType == SliderDisplayType.MoveableBlock)
					{
						EditorGUILayout.LabelField ("Movable block texture:", GUILayout.Width (145f));
					}
					sliderTexture = (Texture2D) EditorGUILayout.ObjectField (sliderTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();

				if (sliderDisplayType == SliderDisplayType.MoveableBlock)
				{
					blockSize = EditorGUILayout.Vector2Field ("Block size:", blockSize);
				}

				sliderType = (AC_SliderType) EditorGUILayout.EnumPopup ("Slider affects:", sliderType);
				if (sliderType == AC_SliderType.CustomScript)
				{
					amount = EditorGUILayout.Slider ("Default value:", amount, 0f, 1f);
					ShowClipHelp ();
				}
				else if (sliderType == AC_SliderType.FloatVariable)
				{
					varID = EditorGUILayout.IntField ("Global Variable ID:", varID);
				}
				numberOfSteps = EditorGUILayout.IntField ("Number of steps:", numberOfSteps);
				isClickable = EditorGUILayout.Toggle ("User can change value?", isClickable);
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI ();
		}
		
		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			GUI.Label (ZoomRect (relativeRect, zoom), "", _style);

			if (sliderTexture)
			{
				DrawSlider (zoom);
			}
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			_style.normal.background = null;

			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style);
			}
		}


		private void DrawSlider (float zoom)
		{
			sliderRect = relativeRect;
			if (sliderDisplayType == SliderDisplayType.FillBar)
			{
				if (useFullWidth)
				{
					sliderRect.x = relativeRect.x;
					sliderRect.width = slotSize.x / 100 * AdvGame.GetMainGameViewSize ().x * amount;
				}
				else
				{
					sliderRect.x = relativeRect.x + (relativeRect.width / 2);
					sliderRect.width = slotSize.x / 100 * AdvGame.GetMainGameViewSize ().x * amount * 0.5f;
				}
			}
			else if (sliderDisplayType == SliderDisplayType.MoveableBlock)
			{
				sliderRect.width *= blockSize.x;
				sliderRect.height *= blockSize.y;
				sliderRect.y += (relativeRect.height - sliderRect.height) / 2f;
				
				if (useFullWidth)
				{
					sliderRect.x = slotSize.x / 100 * AdvGame.GetMainGameViewSize ().x * amount + relativeRect.x - (sliderRect.width / 2f);
				}
				else
				{
					sliderRect.x = slotSize.x / 100 * AdvGame.GetMainGameViewSize ().x * amount * 0.5f + relativeRect.x + (relativeRect.width / 2) - (sliderRect.width / 2f);
				}
			}
			
			GUI.DrawTexture (ZoomRect (sliderRect, zoom), sliderTexture, ScaleMode.StretchToFill, true, 0f);
		}


		public override string GetLabel (int slot)
		{
			return TranslateLabel (label);
		}
		
		
		public void Change ()
		{
			amount += 0.02f; 
			
			if (amount > 1f)
			{
				amount = 0f;
			}

			UpdateValue ();
		}


		public void Change (float mouseX)
		{
			if (useFullWidth)
			{
				mouseX = mouseX - relativeRect.x;
				amount = mouseX / relativeRect.width;
			}
			else
			{
				mouseX = mouseX - relativeRect.x - (relativeRect.width / 2f);
				amount = mouseX / (relativeRect.width / 2f);
			}

			UpdateValue ();
		}


		private void UpdateValue ()
		{
			if (amount < 0f)
			{
				amount = 0;
			}
			else if (amount > 1f)
			{
				amount = 1f;
			}

			// Limit by steps
			if (numberOfSteps > 0)
			{
				float valueSeparation = 1f / (float) numberOfSteps;
				float nearestValue = 0f;
				while (nearestValue < amount)
				{
					nearestValue += valueSeparation;
				}

				// Now larger than amount, so which is closer?
				float lowerNearest = nearestValue - valueSeparation;
				if (amount - lowerNearest > nearestValue - amount)
				{
					amount = nearestValue;
				}
				else
				{
					amount = lowerNearest;
				}
			}

			if (sliderType == AC_SliderType.Speech || sliderType == AC_SliderType.SFX || sliderType == AC_SliderType.Music)
			{
				if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
				{
					Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
					
					if (sliderType == AC_SliderType.Speech)
					{
						options.optionsData.speechVolume = amount;
					}
					else if (sliderType == AC_SliderType.Music)
					{
						options.optionsData.musicVolume = amount;
						options.SetVolume (SoundType.Music);
					}
					else if (sliderType == AC_SliderType.SFX)
					{
						options.optionsData.sfxVolume = amount;
						options.SetVolume (SoundType.SFX);
					}		
					
					options.SavePrefs ();
				}
				else
				{
					Debug.LogWarning ("Could not find Options data!");
				}
			}
			else if (sliderType == AC_SliderType.FloatVariable)
			{
				if (varID >= 0)
				{
					GVar var = RuntimeVariables.GetVariable (varID);
					if (var.type == VariableType.Float)
					{
						var.floatVal = amount;
						var.Upload ();
					}
				}
			}
		}
		
		
		public override void RecalculateSize ()
		{
			if (Application.isPlaying)
			{
				if (sliderType == AC_SliderType.Speech || sliderType == AC_SliderType.SFX || sliderType == AC_SliderType.Music)
				{
					if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
					{	
						Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
						if (options.optionsData != null)
						{
							if (sliderType == AC_SliderType.Speech)
							{
								amount = options.optionsData.speechVolume;
							}
							else if (sliderType == AC_SliderType.Music)
							{
								amount = options.optionsData.musicVolume;
							}
							else if (sliderType == AC_SliderType.SFX)
							{
								amount = options.optionsData.sfxVolume;
							}
						}
					}
				}
				else if (sliderType == AC_SliderType.FloatVariable)
				{
					if (varID >= 0)
					{
						GVar _variable = RuntimeVariables.GetVariable (varID);
						if (_variable != null)
						{
							if (_variable.type != VariableType.Float)
							{
								Debug.LogWarning ("Cannot link MenuSlider " + title + " to Variable " + varID + " as it is not a Float.");
							}
							else
							{
								amount = Mathf.Clamp (RuntimeVariables.GetFloatValue (varID), 0f, 1f);
							}
						}
						else
						{
							Debug.LogWarning ("Slider " + this.label + " is referencing Gloval Variable " + varID + ", which does not exist.");
						}
					}
				}
			}

			base.RecalculateSize ();
		}
		
		
		protected override void AutoSize ()
		{
			AutoSize (new GUIContent (TranslateLabel (label)));
		}

	}

}