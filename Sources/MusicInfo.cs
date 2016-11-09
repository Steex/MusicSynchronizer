using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace MusicSynchronizer
{
	public struct MusicInfo
	{
		public string Path { get; private set; }
		public long Size { get; private set; }
		public DateTime Date { get; private set; }

		public MusicInfo(string path)
			: this()
		{
			FileInfo file = new FileInfo(path);

			Path = path;
			Size = file.Length;
			Date = file.CreationTimeUtc;
		}
	}


	public class MusicComparer
	{
		private HashSet<string> songExtensions = new HashSet<string>{ ".mp3", ".ogg", ".wma", ".aac", ".wav", "*.flac", ".ape", ".wv" };
		private SortedDictionary<string, MusicInfo> sourceSongs = new SortedDictionary<string, MusicInfo>();
		private SortedDictionary<string, MusicInfo> targetSongs = new SortedDictionary<string, MusicInfo>();


		public MusicComparer(string sourceRoot, string targetRoot, IEnumerable<string> playlists)
		{
			// Create list of all playlist files.
			foreach (string playlistPath in playlists)
			{
				IPlaylistReader playlist = PlaylistReaderFactory.CreateReader(playlistPath);
				if (playlist != null)
				{
					AddSongsToList(sourceSongs, playlist.Songs, sourceRoot);
				}
			}

			// Create a list of all files in the target folder.
			IEnumerable<string> targetFiles = Directory.GetFiles(targetRoot, "*", SearchOption.AllDirectories);
			IEnumerable<string> targetSongFiles = targetFiles.Where(path => songExtensions.Contains(Path.GetExtension(path)));

			AddSongsToList(targetSongs, targetSongFiles, targetRoot);

			// 
		}


		private void AddSongsToList(IDictionary<string, MusicInfo> songList, IEnumerable<string> songPaths, string songsRoot)
		{
			// Make sure the root paths end with "\".
			if (!songsRoot.EndsWith("\\"))
			{
				songsRoot += "\\";
			}

			// Add to the list only files that are placed in the source root.
			foreach (string songPath in songPaths)
			{
				if (songPath.StartsWith(songsRoot, StringComparison.OrdinalIgnoreCase))
				{
					string relativePath = songPath.Substring(songsRoot.Length);
					string comparablePath = Path.Combine(Path.GetDirectoryName(relativePath), Path.GetFileNameWithoutExtension(relativePath));

					songList[comparablePath] = new MusicInfo(songPath);
				}
			}
		}

	}

}
