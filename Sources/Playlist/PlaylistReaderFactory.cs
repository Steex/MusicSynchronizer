using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MusicSynchronizer
{
	public static class PlaylistReaderFactory
	{
		public static IPlaylistReader CreateReader(string playlistPath)
		{
			switch (Path.GetExtension(playlistPath).ToLower())
			{
				case ".m3u":
				case ".m3u8":
					return new M3uPlaylistReader(playlistPath);

				default:
					return null;
			}
		}
	}
}
