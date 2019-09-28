#if UNITY_ANDROID
using System;
using System.IO;

namespace RyujuEngine.IO
{
	using AAsset = IntPtr;
	using BufferPointer = IntPtr;

	/// <summary>
	/// A stream which reads from xxx.apk/assets/...
	/// </summary>
	public sealed class AndroidAssetsStream : Stream
	{
		/// <summary>
		/// Create a stream.
		/// </summary>
		/// <param name="path">A relative path from assets directory in apk.</param>
		public AndroidAssetsStream(string path)
		{
			_asset = AndroidAssetManager.Open(path);
		}

		/// <inheritdoc />
		public override bool CanRead => true;

		/// <inheritdoc />
		public override bool CanSeek => true;

		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override long Length
			=> AssertTheStreamHasNotBeenClosed(_asset, NativeMethods.GetLength(_asset));

		/// <inheritdoc />
		public override long Position
		{
			get => Seek(0, SeekOrigin.Current);
			set => Seek(value, SeekOrigin.Begin);
		}

		/// <inheritdoc />
		public override void Flush()
		{
			// no-op.
		}

		/// <inheritdoc />
		public override int Read(byte[] buffer, int offset, int count)
		{
			AssertTheStreamHasNotBeenClosed(_asset);
			if (buffer == null)
			{
				throw new ArgumentNullException("The buffer must be non-null.");
			}
			if (offset < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException("Negative offset and/or count.");
			}
			if (buffer.Length < offset + count)
			{
				throw new ArgumentException("Too larger offset and/or count.");
			}

			int readCount;
			unsafe
			{
				fixed (byte* bufferHead = buffer)
				{
					readCount = NativeMethods.Read(_asset, (BufferPointer)(bufferHead + offset), count);
				}
			}
			if (readCount < 0)
			{
				throw new IOException($"Encountered io exception when reading: {readCount}");
			}
			return readCount;
		}

		/// <inheritdoc />
		public override long Seek(long offset, SeekOrigin origin)
		{
			AssertTheStreamHasNotBeenClosed(_asset);
			var position = NativeMethods.Seek(_asset, offset, origin);
			if (position < 0)
			{
				throw new IOException("Encounterd io exception when seeking.");
			}
			return position;
		}

		/// <inheritdoc />
		public override void SetLength(long value)
		{
			throw new NotSupportedException("Asset stream does not support SetLength method.");
		}

		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("Asset stream does not support Write method.");
		}

		protected override void Dispose(bool fromManaged)
		{
			if (fromManaged)
			{
				// no-op.
			}
			NativeMethods.Close(_asset);
			_asset = IntPtr.Zero;
			base.Dispose(fromManaged);
		}

		private static void AssertTheStreamHasNotBeenClosed(AAsset asset)
		{
			if (asset == IntPtr.Zero)
			{
				throw new ObjectDisposedException("The stream is closed.");
			}
		}

		private static T AssertTheStreamHasNotBeenClosed<T>(AAsset asset, in T value)
		{
			AssertTheStreamHasNotBeenClosed(asset);
			return value;
		}

		private AAsset _asset;
	}
}
#endif
