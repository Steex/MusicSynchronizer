﻿using System;
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
			public IEnumerable<SongInfo> Songs { get; private set; }

			public MusicEraseArguments(IEnumerable<SongInfo> songs)
			{
				Songs = songs;
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

		public class CopyPlaylistsArguments
		{
			public string SourceRoot { get; private set; }
			public string TargetRoot { get; private set; }
			public MusicComparer Comparer { get; private set; }

			public CopyPlaylistsArguments(string sourceRoot, string targetRoot, MusicComparer comparer)
			{
				SourceRoot = sourceRoot;
				TargetRoot = targetRoot;
				Comparer = comparer;
			}
		}

		public class CopyPlaylistsResult
		{
			public OperationResult Result { get; private set; }
			public string ErrorMessage { get; private set; }

			public CopyPlaylistsResult(OperationResult result, string errorMessage)
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


		public static BackgroundWorker CreateMusicEraser()
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

				int songsToDelete = args.Songs.Count();
				int songsDeleted = 0;

				foreach (SongInfo song in args.Songs)
				{
					OperationResult stepResult = DeleteFile(song.TargetPath);
					workResult = Utils.Max(workResult, stepResult);

					songsDeleted += 1;
					worker.ReportProgress(Utils.CalculatePercents(songsToDelete, songsDeleted), song.TargetPath);

					if (worker.CancellationPending)
					{
						e.Result = new MusicEraseResult(OperationResult.Canceled, null);
						return;
					}
				}

				e.Result = new MusicEraseResult(workResult, null);
			}
			catch (Exception ex)
			{
				e.Result = new MusicEraseResult(OperationResult.Failed, ex.Message);
			}
		}


		public static BackgroundWorker CreateMusicConverter()
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

				int songsToConvert = args.Comparer.Songs.Count(s => s.State == SongState.New || s.State == SongState.Changed);
				int songsConverted = 0;

				foreach (SongInfo song in args.Comparer.Songs.Where(s => s.State == SongState.New || s.State == SongState.Changed))
				{
					OperationResult stepResult = ProcessSong(song.SourcePath, song.TargetPath);
					workResult = Utils.Max(workResult, stepResult);

					songsConverted += 1;
					worker.ReportProgress(Utils.CalculatePercents(songsToConvert, songsConverted), song.TargetPath);

					if (worker.CancellationPending)
					{
						e.Result = new MusicConvertResult(OperationResult.Canceled, null);
						return;
					}
				}

				e.Result = new MusicConvertResult(workResult, null);
			}
			catch (Exception ex)
			{
				e.Result = new MusicConvertResult(OperationResult.Failed, ex.Message);
			}
		}


		private static OperationResult ProcessSong(string sourcePath, string targetPath)
		{
			switch (Path.GetExtension(sourcePath))
			{
				case ".mp3":
					return CopySong(sourcePath, targetPath);
				case ".flac":
					return ConvertSong(sourcePath, targetPath);
				default:
					Logger.WriteWarning("Skipping file with unknown format: \"{0}\"", sourcePath);
					return OperationResult.SucceededWithWarnings;
			}
		}

		private static OperationResult CopySong(string sourcePath, string targetPath)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
				File.Copy(sourcePath, targetPath, true);
				Logger.WriteLine("Successfully written \"{0}\"", targetPath);
				return OperationResult.Succeeded;
			}
			catch (DirectoryNotFoundException)
			{
				Logger.WriteWarning("Cannot find the source or target directory for \"{0}\"", sourcePath);
				return OperationResult.SucceededWithWarnings;
			}
			catch (FileNotFoundException)
			{
				Logger.WriteWarning("Cannot find \"{0}\"", sourcePath);
				return OperationResult.SucceededWithWarnings;
			}
			catch (IOException)
			{
				Logger.WriteError("Cannot write \"{0}\": the file is in use.", sourcePath);
				return OperationResult.SucceededWithErrors;
			}
			catch (UnauthorizedAccessException)
			{
				Logger.WriteError("Cannot write \"{0}\": the user does not have the required permission.", targetPath);
				return OperationResult.SucceededWithErrors;
			}
			catch (Exception ex)
			{
				Logger.WriteError("Cannot convert \"{0}\": {1}", sourcePath, ex.Message);
				return OperationResult.SucceededWithErrors;
			}
		}

		private static OperationResult ConvertSong(string sourcePath, string targetPath)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

			// Create a temporary file.
			string tmpPath = Path.GetTempFileName();

			// Perform conversion.
			OperationResult result = DecodeFlac(sourcePath, tmpPath);
			if (result <= OperationResult.SucceededWithWarnings)
			{
				result = Utils.Max(result, EncodeMp3(tmpPath, targetPath));
			}

			// Delete the temporary file.
			File.Delete(tmpPath);

			// Report the result.
			switch (result)
			{
				case OperationResult.Succeeded:
					Logger.WriteLine("Successfully written \"{0}\"", targetPath);
					break;
				case OperationResult.SucceededWithWarnings:
					Logger.WriteLine("Converted with warnings \"{0}\"", targetPath);
					break;
				case OperationResult.SucceededWithErrors:
				case OperationResult.Failed:
					Logger.WriteLine("Cannot convert \"{0}\"", sourcePath);
					break;
				default:
					// nothing to report
					break;
			}

			// Return the result.
			return result;
		}

		private static OperationResult DecodeFlac(string sourcePath, string tmpPath)
		{
			ProcessStartInfo decodeInfo = new ProcessStartInfo();
			decodeInfo.FileName = @"D:\Programs\Multimedia\FLAC\flac.exe";
			decodeInfo.Arguments = string.Format("-d -f -F -o \"{1}\" \"{0}\"", sourcePath, tmpPath);
			decodeInfo.CreateNoWindow = true;
			decodeInfo.UseShellExecute = false;
			decodeInfo.RedirectStandardOutput = true;
			decodeInfo.RedirectStandardError = true;

			Process decodeProcess = Process.Start(decodeInfo);
			decodeProcess.WaitForExit();

			string outText = decodeProcess.StandardOutput.ReadToEnd();
			string errText = decodeProcess.StandardError.ReadToEnd();

			return decodeProcess.ExitCode == 0 ? OperationResult.Succeeded : OperationResult.SucceededWithErrors;
		}

		private static OperationResult EncodeMp3(string tmpPath, string targetPath)
		{
			ProcessStartInfo encodeInfo = new ProcessStartInfo();
			encodeInfo.FileName = @"D:\Programs\Multimedia\LAME\lame.exe";
			encodeInfo.Arguments = string.Format("-V2 \"{0}\" \"{1}\"", tmpPath, targetPath);
			encodeInfo.CreateNoWindow = true;
			encodeInfo.UseShellExecute = false;
			//encodeInfo.RedirectStandardOutput = true;
			//encodeInfo.RedirectStandardError = true;

			Process encodeProcess = Process.Start(encodeInfo);
			encodeProcess.WaitForExit();

			//string outText = encodeProcess.StandardOutput.ReadToEnd();
			//string errText = encodeProcess.StandardError.ReadToEnd();

			return encodeProcess.ExitCode == 0 ? OperationResult.Succeeded : OperationResult.SucceededWithErrors;
		}


		public static BackgroundWorker CreatePlaylistsCopier()
		{
			BackgroundWorker worker = new BackgroundWorker();

			worker.WorkerSupportsCancellation = false;
			worker.WorkerReportsProgress = false;
			worker.DoWork += PlaylistsCopierWorker_DoWork;

			return worker;
		}

		private static void PlaylistsCopierWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				BackgroundWorker worker = (BackgroundWorker)sender;
				CopyPlaylistsArguments args = (CopyPlaylistsArguments)e.Argument;

				OperationResult workResult = OperationResult.Succeeded;

				// Delete existing playlists.
				foreach (string playlist in args.Comparer.TargetPlaylists)
				{
					OperationResult stepResult = DeleteFile(playlist);
					workResult = Utils.Max(workResult, stepResult);
				}

				// Convert source playlists.
				foreach (string playlist in args.Comparer.SourcePlaylists)
				{
					string targetPlaylist = Path.Combine(args.TargetRoot, Path.GetFileName(playlist));
					OperationResult stepResult = ConvertPlaylist(playlist, targetPlaylist);
					workResult = Utils.Max(workResult, stepResult);
				}

				e.Result = new CopyPlaylistsResult(workResult, null);
			}
			catch (Exception ex)
			{
				e.Result = new CopyPlaylistsResult(OperationResult.Failed, ex.Message);
			}
		}

		private static OperationResult ConvertPlaylist(string sourcePath, string targetPath)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
				File.Copy(sourcePath, targetPath, true);
				Logger.WriteLine("Successfully written \"{0}\"", targetPath);
				return OperationResult.Succeeded;
			}
			catch (DirectoryNotFoundException)
			{
				Logger.WriteWarning("Cannot find the source or target directory for \"{0}\"", sourcePath);
				return OperationResult.SucceededWithWarnings;
			}
			catch (FileNotFoundException)
			{
				Logger.WriteWarning("Cannot find \"{0}\"", sourcePath);
				return OperationResult.SucceededWithWarnings;
			}
			catch (IOException)
			{
				Logger.WriteError("Cannot write \"{0}\": the file is in use.", sourcePath);
				return OperationResult.SucceededWithErrors;
			}
			catch (UnauthorizedAccessException)
			{
				Logger.WriteError("Cannot write \"{0}\": the user does not have the required permission.", targetPath);
				return OperationResult.SucceededWithErrors;
			}
			catch (Exception ex)
			{
				Logger.WriteError("Cannot copy playlist \"{0}\": {1}", sourcePath, ex.Message);
				return OperationResult.SucceededWithErrors;
			}
		}


		private static OperationResult DeleteFile(string path)
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
