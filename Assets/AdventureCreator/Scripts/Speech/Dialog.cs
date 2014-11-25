/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Dialog.cs"
 * 
 *	This script handles the running of dialogue lines, speech or otherwise.
 * 
 */

using UnityEngine;
using System.Collections;
using System.IO;

namespace AC
{

	public class Dialog : MonoBehaviour
	{
		
		public bool isMessageAlive { get; set; }
		public bool foundAudio { get; set; }
		public bool isBackground = false;
		private bool currentLineHasAudio = false;
		
		private AC.Char speakerChar;
		private string speakerName;
		
		private float alpha;
		private bool isSkippable = false;
		private string displayText = "";
		private string fullText = "";
		private float displayDuration;
		private float endTime;
		
		private Options options;
		private AudioSource defaultAudioSource;
		private PlayerInput playerInput;
		private SpeechManager speechManager;
		private StateHandler stateHandler;
		
		
		private void Awake ()
		{
			playerInput = this.GetComponent <PlayerInput>();
			
			if (AdvGame.GetReferences () == null)
			{
				Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
			}
			else
			{
				speechManager = AdvGame.GetReferences ().speechManager;
				
				if (speechManager.textScrollSpeed == 0f)
				{
					Debug.LogError ("Cannot have a Text Scroll Speed of zero - please amend your Speech Manager");
				}
			}
			
			if (this.GetComponent <SceneSettings>() && this.GetComponent <SceneSettings>().defaultSound && this.GetComponent <SceneSettings>().defaultSound.GetComponent <AudioSource>())
			{
				defaultAudioSource = this.GetComponent <SceneSettings>().defaultSound.GetComponent <AudioSource>();
			}
		}
		
		
		private void Start ()
		{
			stateHandler = GameObject.FindWithTag  (Tags.persistentEngine).GetComponent <StateHandler>();
			options = stateHandler.GetComponent <Options>();
		}
		
		
		public void UpdateSkipDialogue ()
		{
			if (isSkippable && isMessageAlive && playerInput != null)
			{
				if (speechManager.displayForever)
				{
					if ((playerInput.mouseState == MouseState.SingleClick || playerInput.mouseState == MouseState.RightClick))
					{
						playerInput.mouseState = MouseState.Normal;
						
						if (stateHandler.gameState == GameState.Cutscene)
						{
							if (speechManager.endScrollBeforeSkip && speechManager.scrollSubtitles && displayText != fullText)
							{
								// Stop scrolling
								StopCoroutine ("StartMessage");
								displayText = fullText;
							}
							else
							{
								// Stop message
								isMessageAlive = false;
								StartCoroutine ("EndMessage");
							}
						}
					}

					else if (Time.time > endTime && isBackground)
					{
						// Stop message due to timeout
						StartCoroutine ("EndMessage");
					}
				}

				else if ((playerInput.mouseState == MouseState.SingleClick || playerInput.mouseState == MouseState.RightClick) && speechManager && speechManager.allowSpeechSkipping)
				{
					playerInput.mouseState = MouseState.Normal;
					
					if (stateHandler.gameState == GameState.Cutscene || (speechManager.allowGameplaySpeechSkipping && stateHandler.gameState == GameState.Normal))
					{
						if (speechManager.endScrollBeforeSkip && speechManager.scrollSubtitles && displayText != fullText)
						{
							// Stop scrolling
							StopCoroutine ("StartMessage");
							displayText = fullText;
						}
						else
						{
							// Stop message
							isMessageAlive = false;
							StartCoroutine ("EndMessage");
						}
					}
				}

				else if (Time.time > endTime)
				{
					// Stop message due to timeout
					StartCoroutine ("EndMessage");
				}
			}
		}
		
		
		public string GetSpeaker ()
		{
			if (speakerChar)
			{
				if (speakerChar.speechLabel != "")
				{
					return speakerChar.speechLabel;
				}
				return speakerChar.name;
			}
			
			return "";
		}
		
		
		public AC.Char GetSpeakingCharacter ()
		{
			return speakerChar;
		}
		
		
		public Texture2D GetPortrait ()
		{
			if (speakerChar && speakerChar.portraitIcon.texture)
			{
				return speakerChar.portraitIcon.texture;
			}
			return null;
		}
		
		
		public bool IsAnimating ()
		{
			if (speakerChar && speakerChar.portraitIcon.isAnimated)
			{
				return true;
			}
			return false;
		}
		
		
		public Rect GetAnimatedRect ()
		{
			if (speakerChar != null && speakerChar.portraitIcon != null)
			{
				return speakerChar.portraitIcon.GetAnimatedRect ();
			}
			return new Rect (0,0,0,0);
		}
		
		
		public Color GetColour ()
		{
			if (speakerChar)
			{
				return speakerChar.speechColor;
			}
			
			return Color.white;
		}
		
		
		public string GetLine ()
		{
			if (speechManager.keepTextInBuffer)
			{
				return displayText;
			}
			if (isMessageAlive && isSkippable)
			{
				return displayText;
			}
			return "";
		}
		
		
		public string GetFullLine ()
		{
			if (isMessageAlive && isSkippable)
			{
				return fullText;
			}
			return "";
		}
		
		
		private IEnumerator EndMessage ()
		{
			StopCoroutine ("StartMessage");
			isSkippable = false;
			
			if (speakerChar)
			{
				speakerChar.isTalking = false;
				
				// Turn off animations on the character's "mouth" layer
				if (speakerChar._animation)
				{
					foreach (AnimationState state in speakerChar._animation)
					{
						if (state.layer == (int) AnimLayer.Mouth)
						{
							state.normalizedTime = 1f;
							state.weight = 0f;
						}
					}
				}
				
				if (speakerChar.GetComponent <AudioSource>())
				{
					speakerChar.GetComponent<AudioSource>().Stop();
				}
			}
			
			// Wait a short moment for fade-out
			yield return new WaitForSeconds (0.1f);
			isMessageAlive = false;
		}
		
		
		private IEnumerator StartMessage (string message)
		{
			isMessageAlive = true;
			isSkippable = true;
			
			displayText = "";
			message = AdvGame.ConvertTokens (message);
			fullText = message;

			endTime = displayDuration + Time.time;
			
			if (speechManager.scrollSubtitles)
			{
				// Start scroll the message
				float amount = 0f;
				while (amount < 1f)
				{
					amount += speechManager.textScrollSpeed / 100f / message.Length;
					if (amount > 1f)
					{
						amount = 1f;
					}

					int numChars = (int) (amount * message.Length);
					string newText = message.Substring (0, numChars);
					if (displayText != newText && speechManager.textScrollCLip && !currentLineHasAudio)
					{
						if (defaultAudioSource)
						{
							if (!defaultAudioSource.isPlaying)
							{
								defaultAudioSource.PlayOneShot (speechManager.textScrollCLip);
							}
						}
						else
						{
							Debug.LogWarning ("Cannot play text scroll audio clip as no 'Default' sound prefab has been defined in the Scene Manager");
						}
					}
					displayText = newText;

					yield return new WaitForFixedUpdate ();
				}
				displayText = message;
			}
			else
			{
				displayText = message;
				yield return new WaitForSeconds (message.Length / speechManager.textScrollSpeed);
			}
			
			if (endTime == Time.time)
			{
				endTime += 2f;
			}
		}
		
		
		public float StartDialog (AC.Char _speakerChar, string message, int lineNumber, string language, bool _isBackground)
		{
			isMessageAlive = false;
			currentLineHasAudio = false;
			isBackground = _isBackground;
			
			if (_speakerChar)
			{
				speakerChar = _speakerChar;
				speakerChar.isTalking = true;
				
				speakerName = _speakerChar.name;
				if (_speakerChar.GetComponent <Player>())
				{
					speakerName = "Player";
				}
				
				if (_speakerChar.portraitIcon != null)
				{
					_speakerChar.portraitIcon.Reset ();
				}
				
				if (_speakerChar.GetComponent <Hotspot>())
				{
					if (_speakerChar.GetComponent <Hotspot>().hotspotName != "")
					{
						speakerName = _speakerChar.GetComponent <Hotspot>().hotspotName;
					}
				}
			}
			else
			{
				if (speakerChar)
				{
					speakerChar.isTalking = false;
				}
				speakerChar = null;			
				speakerName = "Narrator";
			}
			
			// Play sound and time displayDuration to it
			if (lineNumber > -1 && speakerName != "" && speechManager.searchAudioFiles)
			{
				string filename = "Speech/";
				if (language != "" && speechManager.translateAudio)
				{
					// Not in original language
					filename += language + "/";
				}
				filename += speakerName + lineNumber;
				
				foundAudio = false;
				AudioClip clipObj = Resources.Load(filename) as AudioClip;
				if (clipObj)
				{
					AudioSource audioSource = null;
					currentLineHasAudio = true;

					if (_speakerChar != null)
					{
						if (_speakerChar.GetComponent <AudioSource>())
						{
							_speakerChar.GetComponent <AudioSource>().volume = options.optionsData.speechVolume;
							audioSource = _speakerChar.GetComponent <AudioSource>();
						}
						else
						{
							Debug.LogWarning (_speakerChar.name + " has no audio source component!");
						}
					}
					else if (KickStarter.player && KickStarter.player.GetComponent <AudioSource>())
					{
						KickStarter.player.GetComponent <AudioSource>().volume = options.optionsData.speechVolume;
						audioSource = KickStarter.player.GetComponent <AudioSource>();
					}
					else if (defaultAudioSource != null)
					{
						audioSource = defaultAudioSource;
					}
					
					if (audioSource != null)
					{
						audioSource.clip = clipObj;
						audioSource.loop = false;
						audioSource.Play();
						
						foundAudio = true;
					}
					
					displayDuration = clipObj.length;
				}
				else
				{
					displayDuration = speechManager.screenTimeFactor * (float) message.Length;
					if (displayDuration < 0.5f)
					{
						displayDuration = 0.5f;
					}
					
					Debug.Log ("Cannot find audio file: " + filename);
				}
			}
			else
			{
				displayDuration = speechManager.screenTimeFactor * (float) message.Length;
				if (displayDuration < 0.5f)
				{
					displayDuration = 0.5f;
				}
			}
			
			StopCoroutine ("StartMessage");
			StartCoroutine ("StartMessage", message);
			
			return displayDuration;
		}
		
		
		public void KillDialog (bool forceMenusOff)
		{
			isSkippable = false;
			isMessageAlive = false;
			
			StopCoroutine ("StartMessage");
			StopCoroutine ("EndMessage");
			
			if (speakerChar && speakerChar.GetComponent <AudioSource>())
			{
				speakerChar.GetComponent <AudioSource>().Stop();
			}

			if (forceMenusOff)
			{
				GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>().ForceOffSubtitles ();
			}
		}
		
		
		private void OnDestroy ()
		{
			playerInput = null;
			speakerChar = null;
			speechManager = null;
			defaultAudioSource = null;
		}
		
	}

}