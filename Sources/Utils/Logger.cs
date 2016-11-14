using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Text;
using System.Drawing.Imaging;

namespace MusicSynchronizer
{
	public enum LogLevel
	{
		Debug,
		Info,
		Warning,
		Error,
	}

	public static class Logger
	{
		public delegate void ClearHandler();
		public delegate void WriteHandler(LogLevel level, string message);

		public static event ClearHandler OnClear;
		public static event WriteHandler OnWrite;


		public static bool TimestampEnabled { get; set; }
		public static string TimestampFormat { get; set; }

		private static object locker = new object();
		private static bool isNewLine = true;
		private static readonly string[] logPrefixes = { "", "", "WARNING: ", "ERROR: " };


		static Logger()
		{
			TimestampEnabled = true;
			TimestampFormat = "[HH:mm:ss]  ";
		}


		public static void WriteDebug(string message)
		{
			PerformWrite(LogLevel.Debug, message, true);
		}

		public static void WriteDebug(string message, params object[] args)
		{
			PerformWrite(LogLevel.Debug, string.Format(message, args), true);
		}

		public static void WriteWarning(string message)
		{
			PerformWrite(LogLevel.Warning, message, true);
		}

		public static void WriteWarning(string message, params object[] args)
		{
			PerformWrite(LogLevel.Warning, string.Format(message, args), true);
		}

		public static void WriteError(string message)
		{
			PerformWrite(LogLevel.Error, message, true);
		}

		public static void WriteError(string message, params object[] args)
		{
			PerformWrite(LogLevel.Error, string.Format(message, args), true);
		}

		public static void WriteLine()
		{
			PerformWrite(LogLevel.Info, "", true);
		}

		public static void WriteLine(string message)
		{
			PerformWrite(LogLevel.Info, message, true);
		}

		public static void WriteLine(string message, params object[] args)
		{
			PerformWrite(LogLevel.Info, string.Format(message, args), true);
		}

		public static void Write(string message)
		{
			PerformWrite(LogLevel.Info, message, false);
		}

		public static void Write(string message, params object[] args)
		{
			PerformWrite(LogLevel.Info, string.Format(message, args), false);
		}


		private static void PerformWrite(LogLevel level, string message, bool finishLine)
		{
			string decoratedText = "";

			// Ensure new line if level-related prefix is required.
			string levelPrefix = logPrefixes[(int)level];
			if (!string.IsNullOrEmpty(levelPrefix) &&
				!isNewLine)
			{
				decoratedText += "\r\n";
				isNewLine = true;
			}

			// Append the timestamp to new lines..
			if (TimestampEnabled && isNewLine)
			{
				decoratedText += DateTime.Now.ToString(TimestampFormat);
			}

			// Append the level-related prefix.
			decoratedText += levelPrefix;

			// Append the message.
			decoratedText += message;

			// Finish the line if required.
			if (finishLine)
			{
				decoratedText += "\r\n";
			}

			// Log the final message.
			lock (locker)
			{
				if (OnWrite != null)
				{
					OnWrite(level, decoratedText);
				}
			}

			// Remember the new line state.
			isNewLine = finishLine;
		}


		public static void Clear()
		{
			lock (locker)
			{
				if (OnClear != null)
				{
					OnClear();
				}
			}
		}
	}
}
