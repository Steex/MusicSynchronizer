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
	public class MusicConverter
	{
		public event EventHandler OnCompared;
		public event EventHandler OnConverted;


		private MusicComparer comparer;

		private string sourceFolder;
		private string targetFolder;
		private IEnumerable<string> playlists;

		private static readonly string msgContinueAfterProblems = "Some problems were encountered during the operation.\r\nDo you wish to continue?";


		public MusicConverter(string sourceFolder, string targetFolder, IEnumerable<string> playlists)
		{
			this.sourceFolder = sourceFolder;
			this.targetFolder = targetFolder;
			this.playlists = playlists;
		}


		public void DoCompare()
		{
			Logger.WriteLine("Compare folders...");

			// Create a worker and show the conversion progress.
			BackgroundWorker worker = BackgroundOperations.CreateComparer();
			worker.RunWorkerCompleted += OnCompareComplete;
			worker.RunWorkerAsync(new BackgroundOperations.CompareArguments(sourceFolder, targetFolder, playlists));
		}

		private void OnCompareComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			BackgroundOperations.CompareResult result = (BackgroundOperations.CompareResult)e.Result;

			BackgroundOperations.LogResult(result.Result, result.ErrorMessage);

			comparer = result.Comparer;

			if (OnCompared != null)
			{
				OnCompared(this, EventArgs.Empty);
			}
		}


		public void DoConvert()
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
					if (Utils.ShowQuestion(null, msgContinueAfterProblems, MessageBoxButtons.YesNo) == DialogResult.No)
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

			switch (result.Result)
			{
				case BackgroundOperations.OperationResult.Canceled:
				case BackgroundOperations.OperationResult.Failed:
					return;
				case BackgroundOperations.OperationResult.SucceededWithWarnings:
				case BackgroundOperations.OperationResult.SucceededWithErrors:
					if (Utils.ShowQuestion(null, msgContinueAfterProblems, MessageBoxButtons.YesNo) == DialogResult.No)
					{
						return;
					}
					break;
			}

			// Start to copy playlists.
			Logger.WriteLine("Copy playlists...");

			// Create a worker and show the conversion progress.
			BackgroundWorker worker = BackgroundOperations.CreatePlaylistsCopier();
			worker.RunWorkerCompleted += OnCopyPlaylistsComplete;
			worker.RunWorkerAsync(new BackgroundOperations.CopyPlaylistsArguments(sourceFolder, targetFolder, comparer));
		}

		private void OnCopyPlaylistsComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			BackgroundOperations.CopyPlaylistsResult result = (BackgroundOperations.CopyPlaylistsResult)e.Result;

			BackgroundOperations.LogResult(result.Result, result.ErrorMessage);

			comparer = null;

			if (OnConverted != null)
			{
				OnConverted(this, EventArgs.Empty);
			}
		}
	}
}