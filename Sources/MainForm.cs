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
		private MusicComparer comparer;


		public MainForm()
		{
			InitializeComponent();

			// Log into text view.
			Logger.OnWrite += (level, message) => textLog.Invoke((MethodInvoker)delegate { textLog.AppendText(message); });
			Logger.OnClear += () => textLog.Invoke((MethodInvoker)delegate { textLog.Clear(); });
		}


		private void buttonCompare_Click(object sender, EventArgs e)
		{
			Logger.WriteLine("Compare folders...");

			// Create a worker and show the conversion progress.
			using (BackgroundWorker worker = BackgroundOperations.CreateComparer())
			{
				worker.RunWorkerCompleted += CompareWorker_RunWorkerCompleted;
				worker.RunWorkerAsync(new BackgroundOperations.CompareArguments(textMusicSourceRoot.Text, textMusicTargetRoot.Text, textPlaylists.Lines));
			}
		}

		private void CompareWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			BackgroundOperations.CompareResult result = (BackgroundOperations.CompareResult)e.Result;

			BackgroundOperations.LogResult(result.Result, result.ErrorMessage);

			comparer = result.Comparer;
		}


		private void buttonConvert_Click(object sender, EventArgs e)
		{

		}
	}
}
