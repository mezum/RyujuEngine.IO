using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using RyujuEngine.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[TestFixture(TestName = nameof(StreamingAssetsStream), TestOf = typeof(StreamingAssetsStream))]
public sealed class StreamingAssetsStreamTest
#if UNITY_EDITOR
: IPrebuildSetup
, IPostBuildCleanup
#endif
{
	// file name
	private const string RyujuEngineIOText = "~~RyujuEngine_IO.txt";
	private const string RyujuEngineIOInvalidText = "~~RyujuEngine_IO_invalid.txt";

	// file content
	private static readonly byte[] RyujuEngineIOTextContent = new byte[]
	{
		(byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e',
		(byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j',
		(byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o',
		(byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t',
		(byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y',
		(byte)'z',
	};

#if UNITY_EDITOR
	// folder name
	private const string Assets = "Assets";
	private const string StreamingAssets = "StreamingAssets";

	// folder path
	private const string Assets_StreamingAssets = Assets + "/" + StreamingAssets;
	private const string Assets_StreamingAssets_TestText = Assets_StreamingAssets + "/" + RyujuEngineIOText;

	// called prebuild
	[PrebuildSetup(typeof(StreamingAssetsStreamTest))]
	void IPrebuildSetup.Setup()
	{
		if (!AssetDatabase.IsValidFolder(Assets_StreamingAssets))
		{
			AssetDatabase.CreateFolder(Assets, StreamingAssets);
		}
		File.WriteAllBytes(Path.Combine(Application.dataPath, StreamingAssets, RyujuEngineIOText), RyujuEngineIOTextContent);
		AssetDatabase.Refresh();
	}

	// called postbuild
	[PostBuildCleanup(typeof(StreamingAssetsStreamTest))]
	void IPostBuildCleanup.Cleanup()
	{
		AssetDatabase.DeleteAsset(Assets_StreamingAssets_TestText);
	}
#endif

	[Test]
	public void It_should_be_able_to_open_a_stream()
	{
		StreamingAssetsStream stream = null;
		Assert.That(() => stream = new StreamingAssetsStream(RyujuEngineIOText), Throws.Nothing, "It must be able to open.");
		Assert.That(stream.CanRead, Is.True, "It must be able to read.");
		Assert.That(stream.CanSeek, Is.True, "It must be able to seek.");
		Assert.That(() => stream.Dispose(), Throws.Nothing, "It must be able to dispose.");
	}

	[Test]
	public void It_should_be_able_to_throw_not_found_exception()
	{
		StreamingAssetsStream stream = null;
		Assert.That(() => stream = new StreamingAssetsStream(RyujuEngineIOInvalidText), Throws.TypeOf<FileNotFoundException>(), "It must be able to detect non-exist file.");
	}

	[Test]
	public void It_should_be_able_to_get_valid_length()
	{
		using (var stream = new StreamingAssetsStream(RyujuEngineIOText))
		{
			Assert.That(stream.Length, Is.EqualTo(RyujuEngineIOTextContent.Length), "It must be able to get valid length.");
		}
	}

	[Test]
	public void It_should_be_able_to_read_all_bytes()
	{
		using (var stream = new StreamingAssetsStream(RyujuEngineIOText))
		{
			var buffer = new byte[stream.Length];
			Assert.That(stream.Read(buffer, 0, buffer.Length), Is.EqualTo(stream.Length), "It must be able to read all bytes.");
			Assert.That(buffer, Is.EqualTo(RyujuEngineIOTextContent), "It must be able to read valid content.");
		}
	}

	[Test]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(3)]
	[TestCase(5)]
	[TestCase(7)]
	public void It_should_be_able_to_read_chunk_bytes(int chunkSize)
	{
		using (var stream = new StreamingAssetsStream(RyujuEngineIOText))
		{
			var length = (int)stream.Length;
			var buffer = new byte[length];

			int restCount = length;
			for (; restCount >= chunkSize; restCount -= chunkSize)
			{
				var readCount = stream.Read(buffer, length - restCount, chunkSize);
				Assert.That(readCount, Is.EqualTo(chunkSize), "It must be read chunk bytes.");
			}
			if (restCount > 0)
			{
				var readCount = stream.Read(buffer, length - restCount, restCount);
				Assert.That(readCount, Is.EqualTo(restCount), "It must be read rest bytes.");
			}
			Assert.That(buffer, Is.EqualTo(RyujuEngineIOTextContent), "It must be able to read valid content.");
		}
	}

	[Test]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(3)]
	[TestCase(5)]
	[TestCase(7)]
	public void It_should_be_able_to_read_bytes_even_if_specify_large_size(int additionalCount)
	{
		using (var stream = new StreamingAssetsStream(RyujuEngineIOText))
		{
			var length = (int)stream.Length;
			var buffer = new byte[length + additionalCount];
			var readCount = stream.Read(buffer, 0, length + additionalCount);
			Assert.That(readCount, Is.EqualTo(length), "It must be able to read only existing bytes.");
			Assert.That(
				new List<byte>(buffer).GetRange(0, length),
				Is.EqualTo(RyujuEngineIOTextContent),
				"It must be able to read valid content."
			);
		}
	}

	[Test]
	public void It_should_be_able_to_read_no_bytes_from_tail()
	{
		using (var stream = new StreamingAssetsStream(RyujuEngineIOText))
		{
			var buffer = new byte[42];
			int count = 0;
			stream.Seek(0, SeekOrigin.End);
			Assert.That(() => count = stream.Read(buffer, 0, buffer.Length), Throws.Nothing, "It must be able to read no bytes from tail.");
			Assert.That(count, Is.EqualTo(0), "It must be able to return zero even if read from tail.");
		}
	}

	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(3)]
	[TestCase(5)]
	[TestCase(7)]
	public void It_should_be_able_to_set_position(int position)
	{
		using (var stream = new StreamingAssetsStream(RyujuEngineIOText))
		{
			Assert.That(() => stream.Position = position, Throws.Nothing, "It must be able to set position");
			Assert.That(stream.Position, Is.EqualTo(position), "It must be able to return valid position.");
		}
	}

	[Test]
	[TestCase(0, 0)]
	[TestCase(0, 1)]
	[TestCase(1, 2)]
	[TestCase(2, 3)]
	[TestCase(3, 5)]
	[TestCase(5, 7)]
	public void It_should_be_able_to_seek_from_begin_with_offset(int position, int offset)
	{
		using (var stream = new StreamingAssetsStream(RyujuEngineIOText))
		{
			stream.Position = position;
			Assert.That(stream.Seek(offset, SeekOrigin.Begin), Is.EqualTo(offset), "It must be able to seek to valid position.");
			Assert.That(stream.Position, Is.EqualTo(offset), "It must be able to return valid position.");
		}
	}

	[Test]
	[TestCase(0, 0)]
	[TestCase(1, 1)]
	[TestCase(2, 2)]
	[TestCase(3, 5)]
	[TestCase(5, 7)]
	public void It_should_be_able_to_seek_from_current_with_offset(int position, int offset)
	{
		using (var stream = new StreamingAssetsStream(RyujuEngineIOText))
		{
			stream.Position = position;
			Assert.That(stream.Seek(offset, SeekOrigin.Current), Is.EqualTo(position + offset), "It must be able to seek to valid position.");
			Assert.That(stream.Position, Is.EqualTo(position + offset), "It must be able to return valid position.");
		}
	}

	[Test]
	[TestCase(0, 0)]
	[TestCase(1, 0)]
	[TestCase(2, -1)]
	[TestCase(3, -2)]
	[TestCase(5, -3)]
	[TestCase(7, -5)]
	public void It_should_be_able_to_seek_from_tail_with_offset(int position, int offset)
	{
		using (var stream = new StreamingAssetsStream(RyujuEngineIOText))
		{
			var length = stream.Length;
			stream.Position = position;
			Assert.That(stream.Seek(offset, SeekOrigin.End), Is.EqualTo(length + offset), "It must be able to seek to valid position.");
			Assert.That(stream.Position, Is.EqualTo(length + offset), "It must be able to return valid position.");
		}
	}
}
