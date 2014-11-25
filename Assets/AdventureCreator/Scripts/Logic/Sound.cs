/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Sound.cs"
 * 
 *	This script allows for easy playback of audio sources from within the ActionList system.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[RequireComponent (typeof (AudioSource))]
	public class Sound : MonoBehaviour
	{

		public SoundType soundType;
		public bool playWhilePaused = false;
		public float relativeVolume = 1f;
		public bool surviveSceneChange = false;
		
		private float maxVolume = 1f;
		private float fadeStartTime;
		private float fadeEndTime;
		private FadeType fadeType;
		[HideInInspector] public bool isFading = false;

		private Options options;
		private AudioSource audioSource;
		
		
		private void Awake ()
		{
			if (soundType == SoundType.Music && surviveSceneChange)
			{
				DontDestroyOnLoad (this);
			}
			
			if (GetComponent <AudioSource>())
			{
				audioSource = GetComponent <AudioSource>();
			}

			audioSource.ignoreListenerPause = playWhilePaused;
		}


		private void Start ()
		{
			StartCoroutine ("_Update");
		}
		
		
		public void AfterLoading ()
		{
			if (audioSource == null && GetComponent <AudioSource>())
			{
				audioSource = GetComponent <AudioSource>();
			}

			if (audioSource)
			{
				audioSource.ignoreListenerPause = playWhilePaused;
				
				if (audioSource.playOnAwake)
				{
					FadeIn (0.5f, audioSource.loop);
				}
				else
				{
					SetMaxVolume ();
				}
			}
			else
			{
				Debug.LogWarning ("Sound object " + this.name + " has no AudioSource component.");
			}
		}
		
		
		private IEnumerator _Update ()
		{
			while (Application.isPlaying)
			{
				UpdateVolume ();
				yield return new WaitForFixedUpdate ();
			}
		}
		
		
		private void UpdateVolume ()
		{
			if (isFading && audioSource.isPlaying)
			{
				float progress = (Time.time - fadeStartTime) / (fadeEndTime - fadeStartTime);
				
				if (fadeType == FadeType.fadeIn)
				{
					if (progress > 1f)
					{
						audioSource.volume = maxVolume;
						isFading = false;
					}
					else
					{
						audioSource.volume = progress * maxVolume;
					}
				}
				else if (fadeType == FadeType.fadeOut)
				{
					if (progress > 1f)
					{
						audioSource.volume = 0f;
						audioSource.Stop ();
						isFading = false;
					}
					else
					{
						audioSource.volume = (1 - progress) * maxVolume;
					}
				}
			}
		}
		
		
		public void Interact ()
		{
			isFading = false;
			SetMaxVolume ();
			Play (audioSource.loop);
		}
		
		
		public void FadeIn (float fadeTime, bool loop)
		{
			audioSource.loop = loop;
			
			fadeStartTime = Time.time;
			fadeEndTime = Time.time + fadeTime;
			fadeType = FadeType.fadeIn;
			
			SetMaxVolume ();
			isFading = true;
			audioSource.volume = 0f;
			audioSource.Play ();
		}
		
		
		public void FadeOut (float fadeTime)
		{
			if (audioSource.isPlaying)
			{
				fadeStartTime = Time.time;
				fadeEndTime = Time.time + fadeTime;
				fadeType = FadeType.fadeOut;
				
				SetMaxVolume ();
				isFading = true;
			}
		}


		public void Play ()
		{
			isFading = false;
			SetMaxVolume ();
			audioSource.Play ();
		}
		
		
		public void Play (bool loop)
		{
			audioSource.loop = loop;
			Play ();
		}


		public void Play (AudioClip clip, bool loop)
		{
			audioSource.clip = clip;
			audioSource.loop = loop;
			Play ();
		}
		
		
		public void SetMaxVolume ()
		{
			maxVolume = relativeVolume;
			
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
			{
				options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
			}
			
			if (options && options.optionsData != null && soundType != SoundType.Other)
			{
				if (soundType == SoundType.Music)
				{
					maxVolume *= options.optionsData.musicVolume;
				}
				else if (soundType == SoundType.SFX)
				{
					maxVolume *= options.optionsData.sfxVolume;
				}
			}

			if (!isFading)
			{
				audioSource.volume = maxVolume;
			}
		}
		
		
		public void Stop ()
		{
			audioSource.Stop ();
		}
		
		
		public void EndOldMusic (Sound newSound)
		{
			if (soundType == SoundType.Music && audioSource.isPlaying && this != newSound)
			{
				if (!isFading || fadeType == FadeType.fadeIn)
				{
					FadeOut (0.1f);
				}
			}
		}


		public bool IsPlaying ()
		{
			return audioSource.isPlaying;
		}


		public bool IsPlaying (AudioClip clip)
		{
			if (audioSource != null && clip != null && audioSource.clip != null && audioSource.clip == clip && audioSource.isPlaying)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsPlayingSameMusic (AudioClip clip)
		{
			if (soundType == SoundType.Music && audioSource != null && clip != null && audioSource.clip != null && audioSource.clip == clip && audioSource.isPlaying)
			{
				return true;
			}
			return false;
		}
		
		
		public bool canDestroy
		{
			get
			{
				if (soundType == SoundType.Music && surviveSceneChange && !audioSource.isPlaying)
				{
					return true;
				}
				
				return false;
			}
		}
		
	}
	
}
