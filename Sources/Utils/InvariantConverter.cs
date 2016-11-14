using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Text;
using System.ComponentModel;

namespace MusicSynchronizer
{
	public static class InvariantConverter
	{
		static Dictionary<Type, TypeConverter> converters;


		static InvariantConverter()
		{
			converters = new Dictionary<Type, TypeConverter>();

			converters.Add(typeof(bool), new BoolTypeConverter());
		}


		public static string ToString<T>(T value)
		{
			return (string)ToString((object)value);
		}

		public static object ToString(object value)
		{
			Type objectType = value.GetType();

			TypeConverter converter = GetConverter(objectType);

			if (converter != null)
			{
				return converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(string));
			}
			else
			{
				throw new InvalidCastException(string.Format("Can't convert the '{0}' to 'string'", objectType));
			}
		}


		public static T FromString<T>(string value)
		{
			return (T)FromString(value, typeof(T));
		}

		public static object FromString(string value, Type targetType)
		{
			TypeConverter converter = GetConverter(targetType);

			if (converter != null)
			{
				return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
			}
			else
			{
				throw new InvalidCastException(string.Format("Can't convert the 'string' to '{0}'", value.GetType(), targetType));
			}
		}


		private static TypeConverter GetConverter(Type type)
		{
			TypeConverter converter;
			if (converters.TryGetValue(type, out converter))
			{
				return converter;
			}

			/*if (type.IsEnum)
			{
				Type generic = typeof(EnumTagConverter<>);
				Type specific = generic.MakeGenericType(type);
				converter = (TypeConverter)specific.GetConstructor(Type.EmptyTypes).Invoke(null);
				converters[type] = converter;
				return converter;
			}*/

			converter = TypeDescriptor.GetConverter(type);
			if (converter != null)
			{
				converters[type] = converter;
				return converter;
			}

			return null;
		}


		/// <summary>
		/// Converts a value to string using <ref>InvariantConverter</ref>
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The invariant string representation of the value.</returns>
		public static string ToInvString<T>(this T value)
		{
			return InvariantConverter.ToString(value);
		}
	}


	internal class BoolTypeConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo info, object value)
		{
			if (value is bool)
			{
				return value;
			}
			else if (value is string)
			{
				return (value as string == "1") ? true : false;
			}

			throw new InvalidCastException(string.Format("Can't convert the '{0}' to 'bool'", value.GetType()));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(bool))
			{
				return value;
			}
			else if (destinationType == typeof(string))
			{
				return (bool)value ? "1" : "0";
			}

			throw new InvalidCastException(string.Format("Can't convert the 'bool' to '{0}'", destinationType));
		}
	}

}
