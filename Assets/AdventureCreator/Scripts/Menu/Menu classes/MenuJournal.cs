/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuJournal.cs"
 * 
 *	This MenuElement provides an array of labels, used to make a book.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuJournal : MenuElement
	{
		
		public List<JournalPage> pages = new List<JournalPage>();
		public int numPages = 1;
		public int showPage = 1;
		public TextAnchor anchor;
		public TextEffects textEffects;

		
		public override void Declare ()
		{
			pages = new List<JournalPage>();
			pages.Add (new JournalPage ());
			numPages = 1;
			showPage = 1;
			isVisible = true;
			isClickable = false;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			textEffects = TextEffects.None;

			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuJournal newElement = CreateInstance <MenuJournal>();
			newElement.Declare ();
			newElement.CopyJournal (this);
			return newElement;
		}
		
		
		public void CopyJournal (MenuJournal _element)
		{
			pages = new List<JournalPage>();
			foreach (JournalPage page in _element.pages)
			{
				pages.Add (page);
			}

			numPages = _element.numPages;
			showPage = 1;
			anchor = _element.anchor;
			textEffects = _element.textEffects;

			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			numPages = EditorGUILayout.IntField ("Number of starting pages:", numPages);
			if (numPages > 0)
			{
				showPage = EditorGUILayout.IntSlider ("Preview page:", showPage, 1, numPages);

				if (numPages != pages.Count)
				{
					if (numPages > pages.Count)
					{
						while (numPages > pages.Count)
						{
							pages.Add (new JournalPage ());
						}
					}
					else
					{
						pages.RemoveRange (numPages, pages.Count - numPages);
					}
				}

				if (showPage > 0 && pages.Count >= showPage-1)
				{
					EditorGUILayout.LabelField ("Page " + showPage + " text:");
					pages[showPage-1].text = EditorGUILayout.TextArea (pages[showPage-1].text);

					if (pages[showPage-1].text.Contains ("*"))
					{
						EditorGUILayout.HelpBox ("Errors will occur if pages contain '*' characters.", MessageType.Error);
					}
					else if (pages[showPage-1].text.Contains ("|"))
					{
						EditorGUILayout.HelpBox ("Errors will occur if pages contain '|' characters.", MessageType.Error);
					}
				}
			}
			else
			{
				numPages = 1;
			}

			anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
			textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI ();
		}
		
		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.wordWrap = true;
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			if (pages.Count >= showPage)
			{
				string newLabel = TranslatePage (pages[showPage - 1]);

				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), newLabel, _style, Color.black, _style.normal.textColor, 2, textEffects);
				}
				else
				{
					GUI.Label (ZoomRect (relativeRect, zoom), newLabel, _style);
				}
			}
		}


		public override string GetLabel (int slot)
		{
			return TranslatePage (pages[showPage - 1]);
		}


		public void Shift (AC_ShiftInventory shiftType, bool doLoop)
		{
			if (shiftType == AC_ShiftInventory.ShiftRight)
			{
				if (pages.Count > showPage)
				{
					showPage ++;
				}
				else if (doLoop)
				{
					showPage = 1;
				}
			}
			else if (shiftType == AC_ShiftInventory.ShiftLeft)
			{
				if (showPage > 1)
				{
					showPage --;
				}
				else if (doLoop)
				{
					showPage = pages.Count;
				}
			}
		}


		private string TranslatePage (JournalPage page)
		{
			if (Options.GetLanguage () > 0 && page.lineID > -1)
			{
				return (SpeechManager.GetTranslation (page.lineID, Options.GetLanguage ()));
			}
			else
			{
				return (page.text);
			}
		}
		
		
		protected override void AutoSize ()
		{
			if (showPage > 0 && pages.Count >= showPage-1)
			{
				if (pages[showPage-1].text == "" && backgroundTexture != null)
				{
					GUIContent content = new GUIContent (backgroundTexture);
					AutoSize (content);
				}
				else
				{
					GUIContent content = new GUIContent (pages[showPage-1].text);
					AutoSize (content);
				}
			
			}
		}
		
	}


	[System.Serializable]
	public class JournalPage
	{

		public int lineID = -1;
		public string text = "";


		public JournalPage ()
		{ }


		public JournalPage (int _lineID, string _text)
		{
			lineID = _lineID;
			text = _text;
		}

	}

}