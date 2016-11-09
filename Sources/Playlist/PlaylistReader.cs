using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicSynchronizer
{
	public interface IPlaylistReader
	{
		IEnumerable<string> Songs { get; }
	}
}
