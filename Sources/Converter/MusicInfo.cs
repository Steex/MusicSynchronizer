using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace MusicSynchronizer
{
	public enum SongState
	{
		Actual,
		New,
		Changed,
		Deleted,
	}


	public struct SongInfo
	{
		public string Name { get; private set; }
		public string SourcePath { get; private set; }
		public string TargetPath { get; private set; }
		public SongState State { get; private set; }

		public SongInfo(string name, string sourcePath, string targetPath)
			: this()
		{
			Name = name;
			SourcePath = sourcePath;
			TargetPath = targetPath;

			if (!File.Exists(TargetPath))
			{
				State = SongState.New;
			}
			else if (!File.Exists(SourcePath))
			{
				State = SongState.Deleted;
			}
			else if (File.GetLastWriteTime(SourcePath) > File.GetLastWriteTime(TargetPath))
			{
				State = SongState.Changed;
			}
			else
			{
				State = SongState.Actual;
			}
		}
	}


	public class MusicComparer
	{
		private static readonly HashSet<string> songExtensions = new HashSet<string>{ ".mp3", ".ogg", ".wma", ".aac", ".wav", "*.flac", ".ape", ".wv" };

		private List<SongInfo> combinedSongs = new List<SongInfo>();


		public IEnumerable<SongInfo> Songs
		{
			get
			{
				return combinedSongs;
			}
		}


		public MusicComparer(string sourceRoot, string targetRoot, IEnumerable<string> playlists)
		{
			// Make sure the root paths end with "\".
			if (!sourceRoot.EndsWith("\\"))
			{
				sourceRoot += "\\";
			}

			if (!targetRoot.EndsWith("\\"))
			{
				targetRoot += "\\";
			}

			// Create list of all playlist files.
			SortedDictionary<string, string> sourceSongs = new SortedDictionary<string, string>();

			foreach (string playlistPath in playlists)
			{
				IPlaylistReader playlist = PlaylistReaderFactory.CreateReader(playlistPath);
				if (playlist != null)
				{
					foreach (string songPath in playlist.Songs.Where(s => Utils.IsPathRelative(s, sourceRoot)))
					{
						AddSongToList(songPath, sourceRoot, sourceSongs);
					}
				}
			}

			// Create a list of all files in the target folder.
			SortedDictionary<string, string> targetSongs = new SortedDictionary<string, string>();

			IEnumerable<string> targetFiles = Directory.GetFiles(targetRoot, "*", SearchOption.AllDirectories);
			IEnumerable<string> targetSongFiles = targetFiles.Where(path => songExtensions.Contains(Path.GetExtension(path)));

			foreach (string songPath in targetSongFiles)
			{
				AddSongToList(songPath, targetRoot, targetSongs);
			}

			// Compare source and target songs.
			var itSource = sourceSongs.GetEnumerator();
			var itTarget = targetSongs.GetEnumerator();

			bool hasSource = itSource.MoveNext();
			bool hasTarget = itTarget.MoveNext();

			while (hasSource || hasTarget)
			{
				if (!hasSource)
				{
					combinedSongs.Add(new SongInfo(itTarget.Current.Key, "", itTarget.Current.Value));
					hasTarget = itTarget.MoveNext();
				}
				else if (!hasTarget)
				{
					combinedSongs.Add(new SongInfo(itSource.Current.Key, itSource.Current.Value, Path.Combine(targetRoot, itSource.Current.Key) + ".mp3"));
					hasSource = itSource.MoveNext();
				}
				else
				{
					int compareResult = string.Compare(itSource.Current.Key, itTarget.Current.Key, true);
					if (compareResult < 0)
					{
						combinedSongs.Add(new SongInfo(itSource.Current.Key, itSource.Current.Value, Path.Combine(targetRoot, itSource.Current.Key) + ".mp3"));
						hasSource = itSource.MoveNext();
					}
					else if (compareResult > 0)
					{
						combinedSongs.Add(new SongInfo(itTarget.Current.Key, "", itTarget.Current.Value));
						hasTarget = itTarget.MoveNext();
					}
					else
					{
						combinedSongs.Add(new SongInfo(itSource.Current.Key, itSource.Current.Value, itTarget.Current.Value));
						hasSource = itSource.MoveNext();
						hasTarget = itTarget.MoveNext();
					}
				}
			}

			// Write test log.
			foreach (SongInfo song in combinedSongs)
			{
				Logger.WriteLine(song.State.ToString().Substring(0, 3) + "   " + song.Name);
			}

		}

		private void AddSongToList(string songPath, string songsRoot, IDictionary<string, string> list)
		{
			string relativePath = Utils.GetRelativePath(songPath, songsRoot);
			string comparablePath = Path.Combine(Path.GetDirectoryName(relativePath), Path.GetFileNameWithoutExtension(relativePath));

			list[comparablePath] = songPath;
		}

	}

}
