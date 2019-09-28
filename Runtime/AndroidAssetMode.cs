#if UNITY_ANDROID
namespace RyujuEngine.IO
{
	/// <summary>
	/// Asset file reading mode.
	/// </summary>
	public enum AndroidAssetMode
	{
		Unknown = 0,
		Random = 1,
		Streaming = 2,
		Buffer = 3,
	}
}
#endif
