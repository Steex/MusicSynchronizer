using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Text;
using System.Drawing.Imaging;

namespace Aliasworlds
{
	public static class Utils
	{
		public static readonly char[] ListSeparators = { ',', ' ', ';' };


		public static void ShowErrorMessage(IWin32Window owner, Exception exception)
		{
			string message = "An error has occurred with the message:\n\n" + exception;
			MessageBox.Show(owner, message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void ShowErrorMessage(IWin32Window owner, string message)
		{
			MessageBox.Show(owner, message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void ShowWarningMessage(IWin32Window owner, string message)
		{
			MessageBox.Show(owner, message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		public static DialogResult ShowQuestion(IWin32Window owner, string message, MessageBoxButtons buttons)
		{
			return MessageBox.Show(owner, message, Application.ProductName, buttons, MessageBoxIcon.Question);
		}


		public static bool ParseBool(string str, bool defaultValue)
		{
			return str == "1";
		}

		public static string BoolToString(bool value)
		{
			return value ? "1" : "0";
		}


		public static float EnsureRange(float value, float min, float max)
		{
			if (value < min)
			{
				return min;
			}
			else if (value > max)
			{
				return max;
			}
			else
			{
				return value;
			}
		}

		public static int EnsureRange(int value, int min, int max)
		{
			if (value < min)
			{
				return min;
			}
			else if (value > max)
			{
				return max;
			}
			else
			{
				return value;
			}
		}

		
		public static string GetRelativePath(string path, string root)
		{
			if (root == null || root.Length == 0)
			{
				return path;
			}

			path = path.Replace('\\', '/');
			root = root.Replace('\\', '/');

			if (!root.EndsWith("/"))
			{
				root += '/';
			}

			if (path.StartsWith(root, StringComparison.InvariantCultureIgnoreCase))
			{
				return path.Substring(root.Length);
			}
			else
			{
				return path;
			}
		}

	}
}
