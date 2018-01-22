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
			BackgroundWorker worker = BackgroundOperations.CreateComparer();
			worker.RunWorkerCompleted += CompareWorker_RunWorkerCompleted;
			worker.RunWorkerAsync(new BackgroundOperations.CompareArguments(textMusicSourceRoot.Text, textMusicTargetRoot.Text, textPlaylists.Lines));
		}

		private void CompareWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			BackgroundOperations.CompareResult result = (BackgroundOperations.CompareResult)e.Result;

			BackgroundOperations.LogResult(result.Result, result.ErrorMessage);

			comparer = result.Comparer;
		}


		private void buttonConvert_Click(object sender, EventArgs e)
		{
			ConvertFiles(comparer);
		}


		static readonly string msgContinueAfterProblems = "Some problems were encountered during the operation.\r\nDo you wish to continue?";


		private void ConvertFiles(MusicComparer comparer)
		{
			if (comparer == null)
			{
				Logger.WriteLine("Before a conversion you must perform a comparison.");
				return;
			}

			Logger.WriteLine("Erase obsolete and changed files...");

			// Create a worker and show the conversion progress.
			BackgroundWorker worker = BackgroundOperations.CreateMusicEraser();
			worker.RunWorkerCompleted += OnEraseComplete;
			worker.RunWorkerAsync(new BackgroundOperations.MusicEraseArguments(
				comparer.Songs.Where(s => s.State == SongState.Changed || s.State == SongState.Deleted)));
		}


		private void OnEraseComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			BackgroundOperations.MusicEraseResult result = (BackgroundOperations.MusicEraseResult)e.Result;

			BackgroundOperations.LogResult(result.Result, result.ErrorMessage);

			switch (result.Result)
			{
				case BackgroundOperations.OperationResult.Canceled:
				case BackgroundOperations.OperationResult.Failed:
					return;
				case BackgroundOperations.OperationResult.SucceededWithWarnings:
				case BackgroundOperations.OperationResult.SucceededWithErrors:
					if (Utils.ShowQuestion(this, msgContinueAfterProblems, MessageBoxButtons.YesNo) == DialogResult.No)
					{
						return;
					}
					break;
			}

			// Start to convert.
			Logger.WriteLine("Convert new and changed files...");

			// Create a worker and show the conversion progress.
			BackgroundWorker worker = BackgroundOperations.CreateMusicConverter();
			worker.RunWorkerCompleted += OnConvertComplete;
			worker.RunWorkerAsync(new BackgroundOperations.MusicConvertArguments(comparer));
		}

		private void OnConvertComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			BackgroundOperations.MusicConvertResult result = (BackgroundOperations.MusicConvertResult)e.Result;

			BackgroundOperations.LogResult(result.Result, result.ErrorMessage);

			comparer = null;
		}
	}
}