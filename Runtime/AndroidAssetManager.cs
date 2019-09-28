/*
Copyright (c) 2019 Mezumona Kosaki
Copyright (c) 2019 RyujuOrchestra
This software is released under the MIT License.
*/

#if UNITY_ANDROID
using System;
using System.IO;
using UnityEngine;

namespace RyujuEngine.IO
{
	using JavaActivity = IntPtr;
	using JavaAssetManager = IntPtr;
	using AAssetManager = IntPtr;
	using AAsset = IntPtr;

	/// <summary>
	/// Android AssetManager.
	/// </summary>
	public static class AndroidAssetManager
	{
		/// <summary>
		/// Open an asset stream.
		/// </summary>
		/// <param name="path">A relative path from assets directory in apk.</param>
		/// <returns>A pointer to native AAsset instance.</returns>
		public static AAsset Open(string path)
		{
			if (_assetManager == IntPtr.Zero)
			{
				_assetManager = GetAssetManager();
			}
			var asset = NativeMethods.Open(_assetManager, path, AndroidAssetMode.Buffer);
			if (asset == IntPtr.Zero)
			{
				throw new FileNotFoundException("File not found in assets.");
			}
			return asset;
		}

		private static AAssetManager GetAssetManager()
		{
			var javaAssetManager = GetJavaAssetManager();
			var assetManagerClass = AndroidJNI.GetObjectClass(javaAssetManager);
			var mObjectField = AndroidJNI.GetFieldID(assetManagerClass, "mObject", "J");
			return (AAssetManager)AndroidJNI.GetLongField(javaAssetManager, mObjectField);
		}

		private static JavaAssetManager GetJavaAssetManager()
		{
			var activity = GetJavaActivity();
			var activityClass = AndroidJNI.GetObjectClass(activity);
			var getAssetsMethod = AndroidJNI.GetMethodID(activityClass, "getAssets", "()Landroid/content/res/AssetManager;");
			return AndroidJNI.CallObjectMethod(activity, getAssetsMethod, Array.Empty<jvalue>());
		}

		private static JavaActivity GetJavaActivity()
		{
			var unityPlayerClass = AndroidJNI.FindClass("com/unity3d/player/UnityPlayer");
			var currentActivityField = AndroidJNI.GetStaticFieldID(unityPlayerClass, "currentActivity", "Landroid/app/Activity;");
			return AndroidJNI.GetStaticObjectField(unityPlayerClass, currentActivityField);
		}

		private static AAssetManager _assetManager = IntPtr.Zero;
	}
}
#endif
