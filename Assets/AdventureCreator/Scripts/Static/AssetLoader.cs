/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AssetLoader.cs"
 * 
 *	This handles the management and retrieval of "Resources"
 *	assets when loading saved games.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public static class AssetLoader
	{

		private static Object[] textureAssets;
		private static Object[] audioAssets;
		private static Object[] animationAssets;


		public static int GetAssetInstanceID <T> (T originalFile) where T : Object
		{
			if (originalFile != null)
			{
				return originalFile.GetInstanceID ();
			}
			return 0;
		}

		
		public static T RetrieveAsset <T> (T originalFile, int _ID) where T : Object
		{
			if (_ID == 0)
			{
				return originalFile;
			}
		
			Object[] assetFiles = null;

			if (originalFile is Texture2D)
			{
				if (textureAssets == null)
				{
					textureAssets = Resources.LoadAll ("", typeof (T));
				}
				assetFiles = textureAssets;
			}
			else if (originalFile is AudioClip)
			{
				if (audioAssets == null)
				{
					audioAssets = Resources.LoadAll ("", typeof (T));
				}
				assetFiles = audioAssets;
			}
			if (originalFile is AnimationClip)
			{
				if (animationAssets == null)
				{
					animationAssets = Resources.LoadAll ("", typeof (T));
				}
				assetFiles = animationAssets;
			}

			foreach (Object assetFile in assetFiles)
			{
				if (assetFile.GetInstanceID () == _ID)
				{
					return (T) assetFile;
				}
			}
			
			return originalFile;
		}


		public static void UnloadAssets ()
		{
			textureAssets = null;
			audioAssets = null;
			animationAssets = null;
			Resources.UnloadUnusedAssets ();
		}

	}

}