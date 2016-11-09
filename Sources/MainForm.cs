using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Aliasworlds;

namespace MusicSynchronizer
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
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

		private void textPlaylists_DragEnter(object sender, DragEventArgs e)
		{
			textLog.AppendText(string.Format("Enter\teffect = {0}\r\n", e.Effect.ToString()));
			//e.Effect = DragDropEffects.Link;
		}

		private void textPlaylists_DragLeave(object sender, EventArgs e)
		{
			textLog.AppendText("Leave\r\n");
		}

		private void textPlaylists_DragOver(object sender, DragEventArgs e)
		{
			textLog.AppendText(string.Format("Over\teffect = {0}\r\n", e.Effect.ToString()));

			Point clientMouse = textPlaylists.PointToClient(new Point(e.X, e.Y));

			if (clientMouse.X > 300)
			{
				e.Effect = DragDropEffects.Link;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void textPlaylists_DragDrop(object sender, DragEventArgs e)
		{
			textLog.AppendText(string.Format("Drop\teffect = {0}\r\n", e.Effect.ToString()));
		}

		private void textPlaylists_GiveFeedback(object sender, GiveFeedbackEventArgs e)
		{
			textLog.AppendText(string.Format("GiveFeedback\teffect = {0}\r\n", e.Effect.ToString()));
		}

		private void textPlaylists_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			textLog.AppendText(string.Format("QueryContinueDrag\taction = {0}\r\n", e.Action.ToString()));
		}

		private void textPlaylists_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				DoDragDrop(sender, DragDropEffects.All);
			}
		}

	}

}
