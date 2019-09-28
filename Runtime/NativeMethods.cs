#if UNITY_ANDROID
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RyujuEngine.IO
{
	using AAssetManager = IntPtr;
	using AAsset = IntPtr;

	/// <summary>
	/// Native methods for AndroidAssetsStream.
	/// </summary>
	public static class NativeMethods
	{
		private const string LibraryName = "android";

		/// <summary>
		/// Open the specified file stream.
		/// </summary>
		/// <param name="assetManager">Native AAssetManager pointer. (not java AssetManager)</param>
		/// <param name="fileName">Asset file name.</param>
		/// <param name="mode">Asset file read mode.</param>
		/// <returns></returns>
		[DllImport(LibraryName, EntryPoint = "AAssetManager_open")]
		public static extern AAsset Open(
			AAssetManager assetManager,
			[In, MarshalAs(UnmanagedType.LPStr)] string fileName,
			[MarshalAs(UnmanagedType.I4)] AndroidAssetMode mode
		);

		/// <summary>
		/// Close the stream.
		/// </summary>
		/// <param name="asset">Target stream.</param>
		[DllImport(LibraryName, EntryPoint = "AAsset_close")]
		public static extern void Close(AAsset asset);

		/// <summary>
		/// Get plain length.
		/// </summary>
		/// <param name="asset">Target stream.</param>
		[DllImport(LibraryName, EntryPoint = "AAsset_getLength64")]
		public static extern long GetLength(AAsset asset);

		/// <summary>
		/// Read the stream.
		/// </summary>
		/// <param name="asset">Target stream.</param>
		/// <param name="buffer">Output buffer pointer.</param>
		/// <param name="count">Number of bytes.</param>
		/// <returns>Bytes count if can read, 0 if reached end of file, negative value otherwise.</retruns>
		[DllImport(LibraryName, EntryPoint = "AAsset_read")]
		public static extern int Read(
			AAsset asset,
			IntPtr buffer,
			[MarshalAs(UnmanagedType.SysInt)] long count
		);

		/// <summary>
		/// Seek the stream.
		/// </summary>
		/// <param name="asset">Target stream.</param>
		/// <param name="offset">An offset in byte from whence.</param>
		/// <param name="whence">Seek origin.</param>
		/// <returns>The position after seeking.</returns>
		[DllImport(LibraryName, EntryPoint = "AAsset_seek64")]
		public static extern long Seek(
			AAsset asset,
			long offset,
			[MarshalAs(UnmanagedType.U4)] SeekOrigin whence
		);
	}
}
#endif
