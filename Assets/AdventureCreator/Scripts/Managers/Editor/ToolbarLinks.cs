using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

public class ToolbarLinks : EditorWindow
{

	[MenuItem ("Adventure Creator/Online resources/Website")]
	static void Website ()
	{
		Application.OpenURL ("http://www.iceboxstudios.co.uk/adventure-creator/");
	}


	[MenuItem ("Adventure Creator/Online resources/Tutorials")]
	static void Tutorials ()
	{
		Application.OpenURL ("http://www.iceboxstudios.co.uk/adventure-creator/tutorials/");
	}


	[MenuItem ("Adventure Creator/Online resources/Forum")]
	static void Forum ()
	{
		Application.OpenURL ("http://www.adventurecreator.org/forum/");
	}


	[MenuItem ("Adventure Creator/Getting started/Load 2D Demo managers")]
	static void Demo2D ()
	{
		ManagerPackage package = AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/2D Demo/ManagerPackage.asset", typeof (ManagerPackage)) as ManagerPackage;
		package.AssignManagers ();
	}


	[MenuItem ("Adventure Creator/Getting started/Load 3D Demo managers")]
	static void Demo3D ()
	{
		ManagerPackage package = AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Demo/ManagerPackage.asset", typeof (ManagerPackage)) as ManagerPackage;
		package.AssignManagers ();
	}

}