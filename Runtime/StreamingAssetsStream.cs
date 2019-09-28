/*
Copyright (c) 2019 Mezumona Kosaki
Copyright (c) 2019 RyujuOrchestra
This software is released under the MIT License.
*/

using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace RyujuEngine.IO
{
	/// <summary>
	/// A stream which is readable from StreamAssets file.
	/// </summary>
	public sealed class StreamingAssetsStream : Stream
	{
		/// <summary>
		/// Creates new readable stream from StreamingAssets.
		/// </summary>
		/// <param name="path">A relative file path from StreamingAssets directory.</param>
		public StreamingAssetsStream(string path)
		{
			_impl = OpenStream(path);
		}

		private static Stream OpenStream(string path)
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			const string JarFilePrefix = "jar:file://";
			const string AssetsPrefix = "!/assets/";
			if (path.StartsWith(JarFilePrefix, System.StringComparison.OrdinalIgnoreCase))
			{
				var splitIndex = path.LastIndexOf(AssetsPrefix, System.StringComparison.Ordinal);
				Assert.IsTrue(splitIndex >= 0, "invalid jar path.");
				path = path.Substring(splitIndex + AssetsPrefix.Length);
			}
			return new AndroidAssetsStream(path);
#else
			Assert.IsFalse(Path.IsPathRooted(path), "The path must be relative.");
			path = Path.Combine(Application.streamingAssetsPath, path);
			return new FileStream(path, FileMode.Open, FileAccess.Read);
#endif
		}

		/// <inheritdoc />
		public override bool CanRead => _impl.CanRead;

		/// <inheritdoc />
		public override bool CanSeek => _impl.CanSeek;

		/// <inheritdoc />
		public override bool CanWrite => _impl.CanWrite;

		/// <inheritdoc />
		public override long Length => _impl.Length;

		/// <inheritdoc />
		public override long Position
		{
			get => _impl.Position;
			set => _impl.Position = value;
		}

		/// <inheritdoc />
		public override void Flush()
		{
			_impl.Flush();
		}

		/// <inheritdoc />
		public override int Read(byte[] buffer, int offset, int count)
		{
			return _impl.Read(buffer, offset, count);
		}

		/// <inheritdoc />
		public override long Seek(long offset, SeekOrigin origin)
		{
			return _impl.Seek(offset, origin);
		}

		/// <inheritdoc />
		public override void SetLength(long value)
		{
			_impl.SetLength(value);
		}

		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count)
		{
			_impl.Write(buffer, offset, count);
		}

		/// <inheritdoc />
		protected override void Dispose(bool fromManaged)
		{
			if (fromManaged)
			{
				_impl.Dispose();
			}
			base.Dispose(fromManaged);
		}

		private readonly Stream _impl;
	}
}
