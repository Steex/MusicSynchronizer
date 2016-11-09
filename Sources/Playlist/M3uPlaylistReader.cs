using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MusicSynchronizer
{
	public class M3uPlaylistReader: IPlaylistReader
	{
		private List<string> songs = new List<string>();

		public IEnumerable<string> Songs
		{
			get { return songs; }
		}

		public M3uPlaylistReader(string path)
		{
			// Read all lines of playlist.
			string extension = Path.GetExtension(path).ToLower();
			Encoding encoding = (extension == ".m3u8") ? Encoding.UTF8 : Encoding.Default;

			string[] lines = File.ReadAllLines(path, encoding);

			// Check the file is really a M3U playlist.
			if (lines[0] != "#EXTM3U")
			{
				throw new Exception("Bad format of playlist '" + path + "'.");
			}

			// Iterate through all song info in the playlist and populate the song list.
			int entryCount = (lines.Length - 1) / 2;

			for (int i = 0; i < entryCount; ++i)
			{
				// Extract a song path.
				string songPath = lines[2 + i * 2];

				// Make sure the song path is absolute, including a drive letter.
				if (songPath.StartsWith("\\"))
				{
					songPath = path.Substring(0, 2) + songPath;
				}

				songs.Add(songPath);
			}
		}

	}

}
