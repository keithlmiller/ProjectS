/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SpeechLine.cs"
 * 
 *	This script is a data container for speech lines found by Speech Manager.
 *	Such data is used to provide translation support, as well as auto-numbering
 *	of speech lines for sound files.
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
	public class SpeechLine
	{
		
		public int lineID;
		public string scene;
		public string owner;
		public string text;
		public AC_TextType textType;
		
		public List<string> translationText = new List<string>();
		
		private GUIStyle style;
		
		
		public SpeechLine ()
		{
			lineID = 0;
			scene = "";
			owner = "";
			text = "";
			translationText = new List<string> ();
			textType = AC_TextType.Speech;
		}


		public SpeechLine (int _id, string _scene, string _text, int _languagues, AC_TextType _textType)
		{
			lineID = _id;
			scene = _scene;
			owner = "";
			text = _text;
			textType = _textType;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}
		
		
		public SpeechLine (int[] idArray, string _scene, string _text, int _languagues, AC_TextType _textType)
		{
			// Update id based on array
			lineID = 0;

			foreach (int _id in idArray)
			{
				if (lineID == _id)
					lineID ++;
			}

			scene = _scene;
			owner = "";
			text = _text;
			textType = _textType;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}
		
		
		public SpeechLine (int _id, string _scene, string _owner, string _text, int _languagues, AC_TextType _textType)
		{
			lineID = _id;
			scene = _scene;
			owner = _owner;
			text = _text;
			textType = _textType;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}
		
		
		public SpeechLine (int[] idArray, string _scene, string _owner, string _text, int _languagues, AC_TextType _textType)
		{
			// Update id based on array
			lineID = 0;
			foreach (int _id in idArray)
			{
				if (lineID == _id)
					lineID ++;
			}
			
			scene = _scene;
			owner = _owner;
			text = _text;
			textType = _textType;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}

		
		#if UNITY_EDITOR
		
		public void ShowGUI ()
		{
			style = new GUIStyle ();
			style.wordWrap = true;
			style.alignment = TextAnchor.MiddleLeft;
		
			EditorGUILayout.BeginHorizontal ();
				if (owner != "" && textType == AC_TextType.Speech)
				{
					EditorGUILayout.LabelField (owner, style, GUILayout.Width (70));
				}
				EditorGUILayout.LabelField (lineID.ToString (), style, GUILayout.MaxWidth (30));
				EditorGUILayout.LabelField ('"' + text + '"', style, GUILayout.MaxWidth (340));
			EditorGUILayout.EndHorizontal ();

			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		}


		public string GetInfo ()
		{
			string info = textType.ToString ();

			if (owner != "")
			{
				info += " (" + owner + ")";
			}

			return info;
		}
		
		
		public string Print ()
		{
			string result = "Character: " + owner + "\nFilename: " + owner + lineID.ToString () + "\n";
			result += '"';
			result += text;
			result += '"';

			return (result);
		}
		
		#endif
		
	}

}