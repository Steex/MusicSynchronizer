using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicSynchronizer
{
	public class PlaylistEntry
	{
		public string Title { get; private set; }
		public int Length { get; private set; }
		public string Path { get; private set; }

		public PlaylistEntry(string title, int length, string path)
		{
			Title = title;
			Length = length;
			Path = path;
		}
	}


	public interface IPlaylistReader
	{
		IEnumerable<PlaylistEntry> Songs { get; }
	}
}
