﻿using System;
using Foundation;

namespace Xamarin.Cognitive.Face.Extensions
{
	/// <summary>
	/// Contains extension methods for working with and mapping native iOS types.
	/// </summary>
	public static class iOSInteropExtensions
	{
		internal static float AsFloatSafe (this NSNumber number, float defaultValue = 0)
		{
			return number?.FloatValue ?? defaultValue;
		}


		internal static bool AsBoolSafe (this NSNumber number, bool defaultValue = false)
		{
			return number?.BoolValue ?? defaultValue;
		}


		internal static DateTime? AsDateSafe (this string dateString, DateTime? defaultValue = null)
		{
			DateTime? date = defaultValue;

			if (DateTime.TryParse (dateString, out DateTime dt))
			{
				date = dt;
			}

			return date;
		}
	}
}