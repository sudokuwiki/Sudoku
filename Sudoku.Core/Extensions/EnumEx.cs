﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sudoku.Extensions
{
	/// <summary>
	/// Provides extension methods on <see cref="Enum"/>.
	/// </summary>
	/// <seealso cref="Enum"/>
	[DebuggerStepThrough]
	public static class EnumEx
	{
		/// <summary>
		/// Get the custom name of the specified enumeration field.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enumeration field.</typeparam>
		/// <param name="this">The instance.</param>
		/// <returns>The custom name.</returns>
		public static string? GetCustomName<TEnum>(this TEnum @this)
			where TEnum : Enum
		{
			var field = typeof(TEnum).GetField(@this.ToString());
			return field is null
				? null
				: (Attribute.GetCustomAttribute(field, typeof(NameAttribute)) as NameAttribute)?.Name;
		}

		/// <summary>
		/// Get all enumeration fields.
		/// </summary>
		/// <typeparam name="TEnum">The type of enumeration type.</typeparam>
		/// <returns>The fields.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TEnum[] GetValues<TEnum>()
			where TEnum : Enum => (TEnum[])Enum.GetValues(typeof(TEnum));
	}
}
