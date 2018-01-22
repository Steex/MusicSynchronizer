using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

namespace MusicSynchronizer
{
	public static class BackgroundOperations
	{
		public enum OperationResult
		{
			Succeeded,
			SucceededWithWarnings,
			SucceededWithErrors,
			Failed,
			Canceled,
		}

		public class CompareArguments
		{
			public string SourceRoot { get; private set; }
			public string TargetRoot { get; private set; }
			public List<string> Playlists { get; private set; }

			public CompareArguments(string sourceRoot, string targetRoot, IEnumerable<string> playlists)
			{
				SourceRoot = sourceRoot;
				TargetRoot = targetRoot;
				Playlists = playlists.ToList();
			}
		}

		public class CompareResult
		{
			public OperationResult Result { get; private set; }
			public MusicComparer Comparer { get; private set; }
			public string ErrorMessage { get; private set; }

			public CompareResult(OperationResult result, MusicComparer comparer, string errorMessage)
			{
				Result = result;
				Comparer = comparer;
				ErrorMessage = errorMessage;
			}
		}

		public class MusicEraseArguments
		{
			public MusicComparer Comparer { get; private set; }

			public MusicEraseArguments(MusicComparer comparer)
			{
				Comparer = comparer;
			}
		}

		public class MusicEraseResult
		{
			public OperationResult Result { get; private set; }
			public string ErrorMessage { get; private set; }

			public MusicEraseResult(OperationResult result, string errorMessage)
			{
				Result = result;
				ErrorMessage = errorMessage;
			}
		}

		public class MusicConvertArguments
		{
			public MusicComparer Comparer { get; private set; }

			public MusicConvertArguments(MusicComparer comparer)
			{
				Comparer = comparer;
			}
		}

		public class MusicConvertResult
		{
			public OperationResult Result { get; private set; }
			public string ErrorMessage { get; private set; }

			public MusicConvertResult(OperationResult result, string errorMessage)
			{
				Result = result;
				ErrorMessage = errorMessage;
			}
		}


		public static BackgroundWorker CreateComparer()
		{
			BackgroundWorker worker = new BackgroundWorker();

			worker.WorkerSupportsCancellation = false;
			worker.WorkerReportsProgress = false;
			worker.DoWork += ComparerWorker_DoWork;

			return worker;
		}

		private static void ComparerWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				BackgroundWorker worker = (BackgroundWorker)sender;
				CompareArguments args = (CompareArguments)e.Argument;

				MusicComparer comparer = new MusicComparer(args.SourceRoot, args.TargetRoot, args.Playlists);

				e.Result = new CompareResult(OperationResult.Succeeded, comparer, null);
			}
			catch (Exception ex)
			{
				e.Result = new CompareResult(OperationResult.Failed, null, ex.Message);
			}
		}


		public static BackgroundWorker CreateMusicEraser(string sourceRoot, string targetRoot, IEnumerable<string> playlists)
		{
			BackgroundWorker worker = new BackgroundWorker();

			worker.WorkerSupportsCancellation = true;
			worker.WorkerReportsProgress = true;
			worker.DoWork += MusicEraserWorker_DoWork;

			return worker;
		}

		private static void MusicEraserWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				BackgroundWorker worker = (BackgroundWorker)sender;
				MusicEraseArguments args = (MusicEraseArguments)e.Argument;

				OperationResult workResult = OperationResult.Succeeded;

				int songsToDelete = args.Comparer.Songs.Count(s => s.State == SongState.Changed || s.State == SongState.Deleted);
				int songsDeleted = 0;

				foreach (SongInfo song in args.Comparer.Songs.Where(s => s.State == SongState.Changed || s.State == SongState.Deleted))
				{
					OperationResult stepResult = DeleteSong(song.TargetPath);
					workResult = Utils.Max(workResult, stepResult);

					songsDeleted += 1;
					worker.ReportProgress(Utils.CalculatePercents(songsToDelete, songsDeleted), song.TargetPath);

					if (worker.CancellationPending)
					{
						e.Result = new MusicEraseResult(workResult, null);
						return;
					}
				}

				e.Result = new MusicEraseResult(OperationResult.Succeeded, null);
			}
			catch (Exception ex)
			{
				e.Result = new MusicEraseResult(OperationResult.Failed, ex.Message);
			}
		}

		private static OperationResult DeleteSong(string path)
		{
			try
			{
				File.Delete(path);
				Logger.WriteLine("Successfully deleted \"{0}\"", path);
				return OperationResult.Succeeded;
			}
			catch (DirectoryNotFoundException)
			{
				Logger.WriteWarning("Cannot find \"{0}\"", path);
				return OperationResult.SucceededWithWarnings;
			}
			catch (IOException)
			{
				Logger.WriteError("Cannot delete \"{0}\": the file is in use.", path);
				return OperationResult.SucceededWithErrors;
			}
			catch (UnauthorizedAccessException)
			{
				Logger.WriteError("Cannot delete \"{0}\": the user does not have the required permission.", path);
				return OperationResult.SucceededWithErrors;
			}
			catch (Exception ex)
			{
				Logger.WriteError("Cannot delete \"{0}\": {1}", path, ex.Message);
				return OperationResult.SucceededWithErrors;
			}
		}


		public static BackgroundWorker CreateMusicConverter(string sourceRoot, string targetRoot, IEnumerable<string> playlists)
		{
			BackgroundWorker worker = new BackgroundWorker();

			worker.WorkerSupportsCancellation = true;
			worker.WorkerReportsProgress = true;
			worker.DoWork += MusicConverterWorker_DoWork;

			return worker;
		}

		private static void MusicConverterWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				BackgroundWorker worker = (BackgroundWorker)sender;
				MusicConvertArguments args = (MusicConvertArguments)e.Argument;

				OperationResult workResult = OperationResult.Succeeded;

				int songsToConvert = args.Comparer.Songs.Count(s => s.State == SongState.Changed || s.State == SongState.Deleted);
				int songsConverted = 0;

				foreach (SongInfo song in args.Comparer.Songs.Where(s => s.State == SongState.Changed || s.State == SongState.Deleted))
				{
					OperationResult stepResult = ConvertSong(song);
					workResult = Utils.Max(workResult, stepResult);

					songsConverted += 1;
					worker.ReportProgress(Utils.CalculatePercents(songsToConvert, songsConverted), song.TargetPath);

					if (worker.CancellationPending)
					{
						e.Result = new MusicConvertResult(workResult, null);
						return;
					}
				}

				e.Result = new MusicConvertResult(OperationResult.Succeeded, null);
			}
			catch (Exception ex)
			{
				e.Result = new MusicConvertResult(OperationResult.Failed, ex.Message);
			}
		}

		private static OperationResult ConvertSong(SongInfo song)
		{
			return OperationResult.Succeeded;

			/*try
			{
				File.Delete(path);
				Logger.WriteLine("Successfully deleted \"{0}\"", path);
				return OperationResult.Succeeded;
			}
			catch (DirectoryNotFoundException)
			{
				Logger.WriteWarning("Cannot find \"{0}\"", path);
				return OperationResult.SucceededWithWarnings;
			}
			catch (IOException)
			{
				Logger.WriteError("Cannot delete \"{0}\": the file is in use.", path);
				return OperationResult.SucceededWithErrors;
			}
			catch (UnauthorizedAccessException)
			{
				Logger.WriteError("Cannot delete \"{0}\": the user does not have the required permission.", path);
				return OperationResult.SucceededWithErrors;
			}
			catch (Exception ex)
			{
				Logger.WriteError("Cannot delete \"{0}\": {1}", path, ex.Message);
				return OperationResult.SucceededWithErrors;
			}*/
		}


		public static void LogResult(OperationResult result, string errorMesage = null)
		{
			switch (result)
			{
				case OperationResult.Succeeded:
					Logger.WriteLine("Done");
					break;
				case OperationResult.SucceededWithWarnings:
					Logger.WriteLine("Done with warnings");
					break;
				case OperationResult.SucceededWithErrors:
					Logger.WriteWarning("Done with errors. Subsequent operations may fail.");
					break;
				case OperationResult.Failed:
					Logger.WriteError("Operation has failed with message: \"{0}\"", errorMesage);
					break;
				case OperationResult.Canceled:
					Logger.WriteError("Canceled by user");
					break;
			}
		}

	}
}
