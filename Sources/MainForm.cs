using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Net;

namespace MusicSynchronizer
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			// Log into text view.
			Logger.OnWrite += (level, message) => textLog.Invoke((MethodInvoker)delegate { textLog.AppendText(message); });
			Logger.OnClear += () => textLog.Invoke((MethodInvoker)delegate { textLog.Clear(); });
		}

		private void buttonCompare_Click(object sender, EventArgs e)
		{
			MusicComparer comparer = new MusicComparer(textMusicSourceRoot.Text, textMusicTargetRoot.Text, textPlaylists.Lines);
			//SortedSet<string> sourceSongs = new SortedSet<string>();
			//SortedSet<string> targetSongs = new SortedSet<string>();

			//foreach (string playlistPath in textPlaylists.Lines)
			//{
			//    IPlaylistReader playlist = PlaylistReaderFactory.CreateReader(playlistPath);
			//    if (playlist != null)
			//    {
			//        foreach (string song in playlist.Songs)
			//        {
			//            sourceSongs.Add(song);
			//        }
			//    }
			//}

			//foreach (string targetSong in Directory.GetFiles(textMusicTargetRoot.Text, "*", SearchOption.AllDirectories))
			//{
			//    targetSongs.Add(Utils.GetRelativePath(targetSong, textMusicTargetRoot.Text));
			//}
		}
	}
}
