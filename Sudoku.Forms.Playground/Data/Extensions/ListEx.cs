﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sudoku.Data.Extensions
{
	/// <summary>
	/// Provides extension methods on <see cref="IList{T}"/>.
	/// </summary>
	/// <seealso cref="IList{T}"/>
	[DebuggerStepThrough]
	public static class ListEx
	{
		/// <summary>
		/// Remove the last element of the specified list, which is equivalent to code:
		/// <code>
		/// list.RemoveAt(list.Count - 1);
		/// </code>
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <param name="this">(<see langword="this"/> parameter) The list.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveLastElement<T>(this IList<T> @this) =>
			@this.RemoveAt(@this.Count - 1);
	}
}