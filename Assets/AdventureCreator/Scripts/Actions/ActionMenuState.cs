/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionMenuState.cs"
 * 
 *	This Action alters various variables of menus and menu elements.
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
	public class ActionMenuState : Action
	{
		
		public enum MenuChangeType { TurnOnMenu, TurnOffMenu, HideMenuElement, ShowMenuElement, LockMenu, UnlockMenu, AddJournalPage };
		public MenuChangeType changeType = MenuChangeType.TurnOnMenu;

		public string menuToChange = "";
		public int menuToChangeParameterID = -1;

		public string elementToChange = "";
		public int elementToChangeParameterID = -1;

		public string journalText = "";
		public bool doFade = false;
		public int lineID = -1;
		
		
		public ActionMenuState ()
		{
			this.isDisplayed = true;
			lineID = -1;
			title = "Menu: Change state";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			menuToChange = AssignString (parameters, menuToChangeParameterID, menuToChange);
			elementToChange = AssignString (parameters, elementToChangeParameterID, elementToChange);
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				Menu _menu = PlayerMenus.GetMenuWithName (menuToChange);
			
				if (_menu != null)
				{
					if (changeType == MenuChangeType.TurnOnMenu)
					{
						if (_menu.appearType == AppearType.Manual || _menu.appearType == AppearType.OnInputKey)
						{
							_menu.TurnOn (doFade);
							
							if (doFade && willWait)
							{
								return defaultPauseTime;
							}
						}
						else
						{
							Debug.LogWarning ("Can only turn on menus with an Appear Type of Manual and OnInputKey.");
						}
					}
					else if (changeType == MenuChangeType.TurnOffMenu)
					{
						if (_menu.appearType == AppearType.Manual || _menu.appearType == AppearType.OnInputKey || _menu.appearType == AppearType.OnContainer)
						{
							_menu.TurnOff (doFade);
							
							if (doFade && willWait)
							{
								return defaultPauseTime;
							}
						}
						else
						{
							Debug.LogWarning ("Can only turn off menus with an Appear Type of Manual, OnContainer, and OnInputKey.");
						}
					}
					else if (changeType == MenuChangeType.HideMenuElement || changeType == MenuChangeType.ShowMenuElement)
					{
						MenuElement _element = PlayerMenus.GetElementWithName (menuToChange, elementToChange);
						if (_element != null)
						{
							if (changeType == MenuChangeType.HideMenuElement)
							{
								_element.isVisible = false;
							}
							else
							{
								_element.isVisible = true;
							}
							_menu.ResetVisibleElements ();
							_menu.Recalculate ();
						}
						else
						{
							Debug.LogWarning ("Could not find menu element of name '" + elementToChange + "'");
						}
					}
					else if (changeType == MenuChangeType.LockMenu)
					{
						if (doFade)
						{
							_menu.TurnOff (true);
						}
						else
						{
							_menu.ForceOff ();
						}
						_menu.isLocked = true;

						if (doFade && willWait)
						{
							return defaultPauseTime;
						}
					}
					else if (changeType == MenuChangeType.UnlockMenu)
					{
						_menu.isLocked = false;
					}
					else if (changeType == MenuChangeType.AddJournalPage)
					{
						MenuElement _element = PlayerMenus.GetElementWithName (menuToChange, elementToChange);
						if (_element != null)
						{
							if (journalText != "")
							{
								if (_element is MenuJournal)
								{
									MenuJournal journal = (MenuJournal) _element;
									JournalPage newPage = new JournalPage (lineID, journalText);
									journal.pages.Add (newPage);
								}
								else
								{
									Debug.LogWarning (_element.title + " is not a journal!");
								}
							}
							else
							{
								Debug.LogWarning ("No journal text to add!");
							}
						}
						else
						{
							Debug.LogWarning ("Could not find menu element of name '" + elementToChange + "' inside '" + menuToChange + "'");
						}
					}
				}
				else if (menuToChange != "")
				{
					Debug.LogWarning ("Could not find menu of name '" + menuToChange + "'");
				}
			}
			
			else
			{
				Menu _menu = PlayerMenus.GetMenuWithName (menuToChange);
			
				if (_menu != null)
				{
					if (_menu.IsFading ())
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
			
			return 0f;
		}


		override public void Skip ()
		{
			Menu _menu = PlayerMenus.GetMenuWithName (menuToChange);
			
			if (_menu != null)
			{
				if (changeType == MenuChangeType.TurnOnMenu)
				{
					if (_menu.appearType == AppearType.Manual || _menu.appearType == AppearType.OnInputKey)
					{
						_menu.TurnOn (false);
					}
				}
				else if (changeType == MenuChangeType.TurnOffMenu)
				{
					if (_menu.appearType == AppearType.Manual || _menu.appearType == AppearType.OnInputKey || _menu.appearType == AppearType.OnContainer)
					{
						_menu.ForceOff ();
					}
				}
				else if (changeType == MenuChangeType.HideMenuElement || changeType == MenuChangeType.ShowMenuElement)
				{
					MenuElement _element = PlayerMenus.GetElementWithName (menuToChange, elementToChange);
					if (_element != null)
					{
						if (changeType == MenuChangeType.HideMenuElement)
						{
							_element.isVisible = false;
						}
						else
						{
							_element.isVisible = true;
						}
						_menu.ResetVisibleElements ();
						_menu.Recalculate ();
					}
				}
				else if (changeType == MenuChangeType.LockMenu)
				{
					_menu.isLocked = true;
					_menu.ForceOff ();
				}
				else if (changeType == MenuChangeType.UnlockMenu)
				{
					_menu.isLocked = false;
				}
				else if (changeType == MenuChangeType.AddJournalPage)
				{
					MenuElement _element = PlayerMenus.GetElementWithName (menuToChange, elementToChange);
					if (_element != null)
					{
						if (journalText != "")
						{
							if (_element is MenuJournal)
							{
								MenuJournal journal = (MenuJournal) _element;
								JournalPage newPage = new JournalPage (lineID, journalText);
								journal.pages.Add (newPage);
							}
						}
					}
				}
			}
		}

		
		override public int End (List<AC.Action> actions)
		{
			Menu _menu = PlayerMenus.GetMenuWithName (menuToChange);
			
			if (changeType == MenuChangeType.TurnOnMenu && _menu && _menu.pauseWhenEnabled && (_menu.appearType == AppearType.Manual || _menu.appearType == AppearType.OnInputKey))
			{
				Debug.LogWarning ("Cannot continue ActionList as a pause menu has been turned on");
				return -1;
			}

			return (base.End (actions));
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			changeType = (MenuChangeType) EditorGUILayout.EnumPopup ("Change type:", changeType);
			
			if (changeType == MenuChangeType.TurnOnMenu)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu to turn on:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu to turn on:", menuToChange);
				}
				doFade = EditorGUILayout.Toggle ("Fade?", doFade);
			}

			else if (changeType == MenuChangeType.TurnOffMenu)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu to turn off:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu to turn off:", menuToChange);
				}
				doFade = EditorGUILayout.Toggle ("Fade?", doFade);
			}

			else if (changeType == MenuChangeType.HideMenuElement)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu containing element:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu containing element:", menuToChange);
				}

				elementToChangeParameterID = Action.ChooseParameterGUI ("Element to hide:", parameters, elementToChangeParameterID, ParameterType.String);
				if (elementToChangeParameterID < 0)
				{
					elementToChange = EditorGUILayout.TextField ("Element to hide:", elementToChange);
				}
			}

			else if (changeType == MenuChangeType.ShowMenuElement)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu containing element:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu containing element:", menuToChange);
				}

				elementToChangeParameterID = Action.ChooseParameterGUI ("Element to show:", parameters, elementToChangeParameterID, ParameterType.String);
				if (elementToChangeParameterID < 0)
				{
					elementToChange = EditorGUILayout.TextField ("Element to show:", elementToChange);
				}
			}

			else if (changeType == MenuChangeType.LockMenu)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu to lock:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu to lock:", menuToChange);
				}
				doFade = EditorGUILayout.Toggle ("Fade?", doFade);
			}

			else if (changeType == MenuChangeType.UnlockMenu)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu to unlock:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu to unlock:", menuToChange);
				}
			}

			else if (changeType == MenuChangeType.AddJournalPage)
			{
				if (lineID > -1)
				{
					EditorGUILayout.LabelField ("Speech Manager ID:", lineID.ToString ());
				}

				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu containing element:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu containing element:", menuToChange);
				}

				elementToChangeParameterID = Action.ChooseParameterGUI ("Journal elementw:", parameters, elementToChangeParameterID, ParameterType.String);
				if (elementToChangeParameterID < 0)
				{
					elementToChange = EditorGUILayout.TextField ("Journal element:", elementToChange);
				}

				EditorGUILayout.LabelField ("New page text:");
				journalText = EditorGUILayout.TextArea (journalText);
			}

			if (doFade && (changeType == MenuChangeType.TurnOnMenu || changeType == MenuChangeType.TurnOffMenu || changeType == MenuChangeType.LockMenu))
			{
				willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
			}
			
			AfterRunningOption ();
		}
		

		public override string SetLabel ()
		{
			string labelAdd = " (" + changeType.ToString () + " '" + menuToChange;
			if (changeType == MenuChangeType.HideMenuElement || changeType == MenuChangeType.ShowMenuElement)
			{
				labelAdd += " " + elementToChange;
			}
			labelAdd += "')";
			return labelAdd;
		}

		#endif
		
	}

}