﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sudoku.Linq
{
	public static class Enumerable
	{
		[return: MaybeNull]
		public static TElement Min<TElement, TComparable>(
			this IEnumerable<TElement> elements, Func<TElement, IComparable<TComparable>> selector)
		{
			return (
				from element in elements
				orderby selector(element) ascending
				select element
			).FirstOrDefault();
		}

		public static int Count<TElement>(
			this IEnumerable<TElement> elements, Func<TElement, int> countingFormula)
		{
			int count = 0;
			foreach (var element in elements)
			{
				count += countingFormula(element);
			}

			return count;
		}

		public static int Count<TElement>(
			this IEnumerable<TElement> elements,
			Predicate<TElement> selector, Func<TElement, int> countingFormula)
		{
			int count = 0;
			foreach (var element in elements)
			{
				if (selector(element))
				{
					count += countingFormula(element);
				}
			}

			return count;
		}
	}
}
