using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MusicSynchronizer
{
	public class M3uPlaylistReader: IPlaylistReader
	{
		private static readonly string FileHeader = "#EXTM3U";
		private static readonly string InfoHeader = "#EXTINF:";

		private List<PlaylistEntry> songs = new List<PlaylistEntry>();

		public IEnumerable<PlaylistEntry> Songs
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
			if (lines[0] != FileHeader)
			{
				throw new Exception("Bad format of playlist '" + path + "'.");
			}

			// Iterate through all lines in the playlist and populate the song list.
			IEnumerator<string> itLine = IterateNonEmptyLines(lines.Skip(1));
			while (itLine.MoveNext())
			{
				string songTitle = null;
				int songLength = 0;
				string songPath = null;

				// Read the extrended info if it is available.
				if (itLine.Current.StartsWith(InfoHeader))
				{
					int infoLength = InfoHeader.Length;
					int commaPos = itLine.Current.IndexOf(',', infoLength);
					songLength = int.Parse(itLine.Current.Substring(infoLength, commaPos - infoLength));
					songTitle = itLine.Current.Substring(commaPos + 1);
				}

				itLine.MoveNext();

				// Read the path and make sure it is absolute, including a drive letter.
				songPath = itLine.Current;
				if (songPath.StartsWith("\\"))
				{
					songPath = path.Substring(0, 2) + songPath;
				}

				// Add the playlist entry.
				songs.Add(new PlaylistEntry(songTitle, songLength, songPath));
			}
		}


		private IEnumerator<string> IterateNonEmptyLines(IEnumerable<string> allLines)
		{
			foreach (string line in allLines)
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					yield return line;
				}
			}
		}
	}

}
