/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuGraphic.cs"
 * 
 *	This MenuElement provides a space for
 *	animated and static textures
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class MenuGraphic : MenuElement
	{

		public CursorIconBase graphic;


		public override void Declare ()
		{
			isVisible = true;
			isClickable = false;
			graphic = new CursorIconBase ();
			numSlots = 1;
			SetSize (new Vector2 (10f, 5f));

			base.Declare ();
		}
		
		
		public override MenuElement DuplicateSelf ()
		{
			MenuGraphic newElement = CreateInstance <MenuGraphic>();
			newElement.Declare ();
			newElement.CopyGraphic (this);
			return newElement;
		}
		
		
		public void CopyGraphic (MenuGraphic _element)
		{
			graphic = _element.graphic;
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			graphic.ShowGUI (false);
			EditorGUILayout.EndVertical ();

			base.ShowGUI ();
		}

		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			graphic.DrawAsInteraction (ZoomRect (relativeRect, zoom), true);
		}
		
		
		public override void RecalculateSize ()
		{
			graphic.Reset ();
			base.RecalculateSize ();
		}
		
		
		protected override void AutoSize ()
		{
			if (graphic.texture != null)
			{
				GUIContent content = new GUIContent (graphic.texture);
				AutoSize (content);
			}
		}
		
	}
	
}