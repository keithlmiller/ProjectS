/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSpeech.cs"
 * 
 *	This action handles the displaying of messages, and talking of characters.
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
	public class ActionSpeech : Action
	{

		public int constantID = 0;
		public int parameterID = -1;
		
		public bool isPlayer;
		public Char speaker;
		public string messageText;
		public int lineID;
		public bool isBackground = false;
		public AnimationClip headClip;
		public AnimationClip mouthClip;

		public bool play2DHeadAnim = false;
		public string headClip2D = "";
		public int headLayer;
		public bool play2DMouthAnim = false;
		public string mouthClip2D = "";
		public int mouthLayer;

		public float waitTimeOffset = 0f;
		private float endTime = 0f;
		private bool stopAction = false;

		private int splitNumber = 0;
		private bool splitDelay = false;

		private Dialog dialog;
		private StateHandler stateHandler;
		private SpeechManager speechManager;

		
		public ActionSpeech ()
		{
			this.isDisplayed = true;
			title = "Dialogue: Play speech";
			lineID = -1;
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			speaker = AssignFile <Char> (parameters, parameterID, constantID, speaker);

			if (isPlayer)
			{
				speaker = KickStarter.player;
			}
		}


		override public float Run ()
		{
			if (speechManager == null)
			{
				speechManager = AdvGame.GetReferences ().speechManager;
			}
			if (speechManager == null)
			{
				Debug.Log ("No Speech Manager present");
				return 0f;
			}

			dialog = GameObject.FindWithTag(Tags.gameEngine).GetComponent <Dialog>();
			stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();

			if (dialog && stateHandler)
			{
				if (!isRunning)
				{
					stopAction = false;
					isRunning = true;
					splitDelay = false;
					splitNumber = 0;
					
					endTime = Time.time + StartSpeech ();

					if (isBackground)
					{
						isRunning = false;
						return 0f;
					}
					return defaultPauseTime;
				}
				else
				{
					if (stopAction)
					{
						isRunning = false;
						return 0;
					}

					if (!dialog.isMessageAlive)
					{
						if (speechManager.separateLines)
						{
							if (!splitDelay)
							{
								// Begin pause if more lines are present
								splitNumber ++;
								string[] textArray = messageText.Split ('\n');
								if (textArray.Length > splitNumber)
								{
									// Still got more to go, so pause for a moment
									splitDelay = true;
									return speechManager.separateLinePause;
								}
								// else finished
							}
							else
							{
								// Show next line
								splitDelay = false;
								endTime = Time.time + StartSpeech ();
								return defaultPauseTime;
							}
						}

						if (waitTimeOffset <= 0f)
						{
							isRunning = false;
							return 0f;
						}
						else
						{
							stopAction = true;
							return waitTimeOffset;
						}
					}
					else
					{
						if (speechManager.displayForever)
						{
							return defaultPauseTime;
						}

						if (speechManager.separateLines)
						{
							return defaultPauseTime;
						}

						if (Time.time < endTime)
						{
							return defaultPauseTime;
						}
						else
						{
							isRunning = false;
							return 0f;
						}
					}
				}
			}
			
			return 0f;
		}


		override public void Skip ()
		{
			GameObject.FindWithTag(Tags.gameEngine).GetComponent <Dialog>().KillDialog (true);

			if (speaker)
			{
				speaker.isTalking = false;

				if (speaker.animEngine == null)
				{
					speaker.ResetAnimationEngine ();
				}
				
				if (speaker.animEngine != null)
				{
					speaker.animEngine.ActionSpeechSkip (this);
				}
			}
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (lineID > -1)
			{
				EditorGUILayout.LabelField ("Speech Manager ID:", lineID.ToString ());
			}
			
			isPlayer = EditorGUILayout.Toggle ("Player line?",isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					speaker = KickStarter.player;
				}
				else
				{
					speaker = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}
			else
			{
				parameterID = Action.ChooseParameterGUI ("Speaker:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					speaker = null;
				}
				else
				{
					speaker = (Char) EditorGUILayout.ObjectField ("Speaker:", speaker, typeof(Char), true);
					
					constantID = FieldToID <Char> (speaker, constantID);
					speaker = IDToField <Char> (speaker, constantID, false);
				}
			}

			EditorGUILayout.LabelField ("Line text:", GUILayout.Width (145f));
			EditorStyles.textField.wordWrap = true;
			messageText = EditorGUILayout.TextArea (messageText);

			if (speaker)
			{
				if (speaker.animEngine == null)
				{
					speaker.ResetAnimationEngine ();
				}
				if (speaker.animEngine)
				{
					speaker.animEngine.ActionSpeechGUI (this);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("If no Character is set, this line will be considered to be a Narration.", MessageType.Info);
			}
			
			isBackground = EditorGUILayout.Toggle ("Play in background?", isBackground);
			if (!isBackground)
			{
				waitTimeOffset = EditorGUILayout.Slider ("Wait time offset (s):", waitTimeOffset, -1f, 4f);
			}

			AfterRunningOption ();
		}


		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (messageText != "")
			{
				string shortMessage = messageText;
				if (shortMessage != null)
				{
					if (shortMessage.Contains ("\n"))
					{
						shortMessage = shortMessage.Replace ("\n", "");
					}
					if (shortMessage.Length > 30)
					{
						shortMessage = shortMessage.Substring (0, 28) + "..";
					}
				}

				labelAdd = " (" + shortMessage + ")";
			}
			
			return labelAdd;
		}

		#endif


		private float StartSpeech ()
		{
			string _text = messageText;
			string _language = "";
			
			if (Options.GetLanguage () > 0)
			{
				// Not in original language, so pull translation in from Speech Manager
				_text = SpeechManager.GetTranslation (lineID, Options.GetLanguage ());
				_language = speechManager.languages [Options.GetLanguage ()];
			}

			bool isSplittingLines = false;
			bool isLastSplitLine = false;
			if (speechManager.separateLines)
			{
				// Split line into an array, and pull the correct one
				string[] textArray = _text.Split ('\n');
				_text = textArray [splitNumber];

				if (textArray.Length > 1)
				{
					isSplittingLines = true;

					if (textArray.Length == splitNumber + 1)
					{
						isLastSplitLine = true;
					}
				}
			}
			
			if (_text != "")
			{
				dialog.KillDialog (true);

				float displayDuration = dialog.StartDialog (speaker, _text, lineID, _language, isBackground);

				if (speaker)
				{
					if (speaker.animEngine == null)
					{
						speaker.ResetAnimationEngine ();
					}
					
					if (speaker.animEngine != null)
					{
						speaker.animEngine.ActionSpeechRun (this);
					}
				}

				if (isLastSplitLine)
				{
					return (displayDuration + waitTimeOffset);
				}

				if (isSplittingLines)
				{
					return displayDuration;
				}

				if (!isBackground)
				{
					return (displayDuration + waitTimeOffset);
				}
			}
			
			return 0f;
		}

	}

}