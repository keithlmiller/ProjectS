/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"CursorIcon.cs"
 * 
 *	This script is a data class for cursor icons.
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class CursorIcon : CursorIconBase
	{

		public string label;
		public int lineID = -1;
		public int id;

		
		public CursorIcon ()
		{
			texture = null;
			id = 0;
			lineID = -1;
			isAnimated = false;
			numFrames = 1;
			size = 0.04f;

			label = "Icon " + (id + 1).ToString ();
		}

		
		public CursorIcon (int[] idArray)
		{
			texture = null;
			id = 0;
			lineID = -1;
			isAnimated = false;
			numFrames = 1;

			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
				{
					id ++;
				}
			}
			
			label = "Icon " + (id + 1).ToString ();
		}


		public void Copy (CursorIcon _cursorIcon)
		{
			label = _cursorIcon.label;
			lineID = _cursorIcon.lineID;
			id = _cursorIcon.id;

			base.Copy (_cursorIcon);
		}

	}


	[System.Serializable]
	public class CursorIconBase
	{
		
		public Texture2D texture;
		public bool isAnimated = false;
		public int numFrames = 1;
		public float size = 0.015f;
		public float animSpeed = 4f;
		public bool endAnimOnLastFrame = false;
		public Vector2 clickOffset;

		private float frameIndex = 0f;
		private float frameWidth = -1f;


		public CursorIconBase ()
		{
			texture = null;
			isAnimated = false;
			numFrames = 1;
			size = 0.015f;
			frameIndex = 0f;
			frameWidth = -1f;
			animSpeed = 4;
			endAnimOnLastFrame = false;
			clickOffset = Vector2.zero;
		}
		

		public void Copy (CursorIconBase _icon)
		{
			texture = _icon.texture;
			isAnimated = _icon.isAnimated;
			numFrames = _icon.numFrames;
			animSpeed = _icon.animSpeed;
			endAnimOnLastFrame = _icon.endAnimOnLastFrame;
			clickOffset = _icon.clickOffset;
		}


		public void DrawAsInteraction (Rect _rect, bool isActive)
		{
			if (texture == null)
			{
				return;
			}

			if (isAnimated && numFrames > 0 && Application.isPlaying)
			{
				if (isActive)
				{
					GUI.DrawTextureWithTexCoords (_rect, texture, GetAnimatedRect ());
				}
				else
				{
					GUI.DrawTextureWithTexCoords (_rect, texture, new Rect (0f, 0f, frameWidth, 1f));
					frameIndex = 0f;
				}
			}
			else
			{
				GUI.DrawTexture (_rect, texture, ScaleMode.StretchToFill, true, 0f);
			}
		}


		public void Draw (Vector2 centre)
		{
			if (texture == null)
			{
				return;
			}
			
			Rect _rect = AdvGame.GUIBox (centre, size);
			_rect.x -= clickOffset.x * _rect.width;
			_rect.y -= clickOffset.y * _rect.height;

			if (isAnimated && numFrames > 0 && Application.isPlaying)
			{
				GUI.DrawTextureWithTexCoords (_rect, texture, GetAnimatedRect ());
			}
			else
			{
				GUI.DrawTexture (_rect, texture, ScaleMode.ScaleToFit, true, 0f);
			}
		}


		public Rect GetAnimatedRect ()
		{
			if (frameIndex < 0f)
			{
				frameIndex = 0f;
			}
			else if (frameIndex < numFrames)
			{
				if (Time.deltaTime == 0f)
				{
					frameIndex += 0.02f * animSpeed;
				}
				else
				{
					frameIndex += Time.deltaTime * animSpeed;
				}
			}

			if (frameIndex >= numFrames)
			{
				if (!endAnimOnLastFrame)
				{
					frameIndex = 0f;
				}
				else
				{
					frameIndex = numFrames;
					return new Rect (frameWidth * Mathf.Floor (numFrames - 0.1f), 0f, frameWidth, 1f);
				}
			}
			return new Rect (frameWidth * Mathf.Floor (frameIndex), 0f, frameWidth, 1f);
		}


		public void Reset ()
		{
			if (isAnimated)
			{
				if (numFrames > 0)
				{
					frameWidth = 1f / numFrames;
					frameIndex = 0f;
				}
				else
				{
					Debug.LogWarning ("Cannot have an animated cursor with less than one frame!");
				}

				if (animSpeed < 0)
				{
					animSpeed = 0;
				}
			}
		}


		#if UNITY_EDITOR

		public void ShowGUI (bool includeSize)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Texture:", GUILayout.Width (145));
			texture = (Texture2D) EditorGUILayout.ObjectField (texture, typeof (Texture2D), false, GUILayout.Width (70), GUILayout.Height (70));
			EditorGUILayout.EndHorizontal ();

			if (includeSize)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Size:", GUILayout.Width (50f));
				size = EditorGUILayout.FloatField (size, GUILayout.Width (70f));
				EditorGUILayout.LabelField ("Click offset:", GUILayout.Width (110f));
				clickOffset = EditorGUILayout.Vector2Field ("", clickOffset, GUILayout.Width (120f));
				EditorGUILayout.EndHorizontal ();
			}
			
			isAnimated = EditorGUILayout.ToggleLeft ("Animate?", isAnimated);
			if (isAnimated)
			{
				numFrames = EditorGUILayout.IntField ("Number of frames:", numFrames);
				animSpeed = EditorGUILayout.FloatField ("Animation speed:", animSpeed);
				endAnimOnLastFrame = EditorGUILayout.Toggle ("End on last frame?", endAnimOnLastFrame);
			}
		}

		#endif

	}


	[System.Serializable]
	public class HotspotPrefix
	{

		public string label;
		public int lineID;


		public HotspotPrefix (string text)
		{
			label = text;
			lineID = -1;
		}

	}

}