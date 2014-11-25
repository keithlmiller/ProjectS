/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInventoryCrafting.cs"
 * 
 *	This action is used to perform crafting-related tasks.
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
	public class ActionInventoryCrafting : Action
	{

		public enum ActionCraftingMethod { ClearRecipe, CreateRecipe };
		public ActionCraftingMethod craftingMethod;

		
		override public float Run ()
		{
			if (craftingMethod == ActionCraftingMethod.ClearRecipe)
			{
				GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>().RemoveRecipes ();
			}
			else if (craftingMethod == ActionCraftingMethod.CreateRecipe)
			{
				PlayerMenus.CreateRecipe ();
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		public ActionInventoryCrafting ()
		{
			this.isDisplayed = true;
			title = "Inventory: Crafting";
		}
		
		
		override public void ShowGUI ()
		{
			craftingMethod = (ActionCraftingMethod) EditorGUILayout.EnumPopup ("Method:", craftingMethod);
		}
		
		
		override public string SetLabel ()
		{
			if (craftingMethod == ActionCraftingMethod.CreateRecipe)
			{
				return (" (Create recipe)");
			}
			else if (craftingMethod == ActionCraftingMethod.ClearRecipe)
			{
				return (" (Clear recipe)");
			}
			return "";
		}
		
		#endif
		
	}

}