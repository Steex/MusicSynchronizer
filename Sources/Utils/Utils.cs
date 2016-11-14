using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Diagnostics;
using Microsoft.Win32;

namespace MusicSynchronizer
{
	public static class Utils
	{
		public static readonly char[] DirectorySeparators = { '/', '\\' };
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


		public static T ReadRegistryValue<T>(RegistryKey key, string name, T defaultValue)
		{
			try
			{
				string strValue = (string)key.GetValue(name, InvariantConverter.ToString(defaultValue));
				return InvariantConverter.FromString<T>(strValue);
			}
			catch
			{
				return defaultValue;
			}
		}

		public static IEnumerable<string> ReadRegistryList(RegistryKey key, string baseName)
		{
			return ReadRegistryList<string>(key, baseName);
		}

		public static IEnumerable<T> ReadRegistryList<T>(RegistryKey key, string baseName)
		{
			for (int index = 1; ; ++index)
			{
				object value = key.GetValue(baseName + index.ToString());
				if (value != null)
				{
					yield return InvariantConverter.FromString<T>(value.ToString());
				}
				else
				{
					break;
				}
			}
		}

		public static void WriteRegistryValue<T>(RegistryKey key, string name, T value)
		{
			if (value != null)
			{
				key.SetValue(name, InvariantConverter.ToString(value));
			}
			else
			{
				key.SetValue(name, "");
			}
		}

		public static void WriteRegistryList<T>(RegistryKey key, string baseName, IEnumerable<T> list)
		{
			if (list != null)
			{
				int index = 1;
				foreach (T item in list)
				{
					Utils.WriteRegistryValue(key, baseName + index.ToString(), InvariantConverter.ToString(item));
					++index;
				}
			}
		}


		public static T DeserializeValueFromRegistry<T>(RegistryKey key, string name, T defaultValue)
		{
			try
			{
				string strValue = (string)key.GetValue(name, InvariantConverter.ToString(defaultValue));
				return DeserializeValue<T>(strValue);
			}
			catch
			{
				return defaultValue;
			}
		}

		public static IEnumerable<T> DeserializeListFromRegistry<T>(RegistryKey key, string baseName)
		{
			for (int index = 1; ; ++index)
			{
				object strValue = key.GetValue(baseName + index.ToString());
				if (strValue is string)
				{
					yield return DeserializeValue<T>((string)strValue);
				}
				else
				{
					break;
				}
			}
		}

		public static void SerializeValueToRegistry<T>(RegistryKey key, string name, T value)
		{
			key.SetValue(name, SerializeValue(value));
		}

		public static void SerializeListToRegistry<T>(RegistryKey key, string baseName, IEnumerable<T> list)
		{
			if (list != null)
			{
				int index = 1;
				foreach (T item in list)
				{
					key.SetValue(baseName + index.ToString(), SerializeValue(item));
					++index;
				}
			}

		}


		public static string SerializeValue(object value)
		{
			if (value == null)
			{
				return "";
			}

			using (MemoryStream stream = new MemoryStream())
			{
				DataContractJsonSerializer serialzer = new DataContractJsonSerializer(value.GetType());
				serialzer.WriteObject(stream, value);
				return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
			}
		}

		public static T DeserializeValue<T>(string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				return default(T);
			}

			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
			{
				DataContractJsonSerializer serialzer = new DataContractJsonSerializer(typeof(T));
				return (T)serialzer.ReadObject(stream);
			}
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


		public static bool IsPathRelative(string path, string root)
		{
			if (string.IsNullOrWhiteSpace(root))
			{
				return false;
			}

			path = path.Replace('\\', '/');
			root = root.Replace('\\', '/');

			if (!root.EndsWith("/"))
			{
				root += '/';
			}

			return path.StartsWith(root, StringComparison.InvariantCultureIgnoreCase);
		}

		public static string GetRelativePath(string path, string root)
		{
			if (string.IsNullOrWhiteSpace(root))
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
