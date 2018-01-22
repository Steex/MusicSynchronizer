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
	[System.Runtime.InteropServices.Guid("CA841C2B-90DE-4A50-B316-A98BC68768B1")]
	public partial class MainForm : Form
	{
		private MusicConverter converter;


		public MainForm()
		{
			InitializeComponent();

			// Log into text view.
			Logger.OnWrite += (level, message) => textLog.Invoke((MethodInvoker)delegate { textLog.AppendText(message); });
			Logger.OnClear += () => textLog.Invoke((MethodInvoker)delegate { textLog.Clear(); });
		}


		private void buttonCompare_Click(object sender, EventArgs e)
		{
			converter = new MusicConverter(textMusicSourceRoot.Text, textMusicTargetRoot.Text, textPlaylists.Lines);
			converter.OnCompared += converter_OnCompared;
			converter.OnConverted += converter_OnConverted;

			converter.DoCompare();
		}

		private void buttonConvert_Click(object sender, EventArgs e)
		{
			if (converter != null)
			{
				converter.DoConvert();
			}
		}


		private void converter_OnCompared(object sender, EventArgs e)
		{
		}

		private void converter_OnConverted(object sender, EventArgs e)
		{
			converter = null;
		}

	}
}