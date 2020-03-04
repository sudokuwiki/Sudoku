﻿using System.Collections.Generic;
using Sudoku.Solving.Utils;

namespace Sudoku.Solving.Manual.Uniqueness.Loops
{
	/// <summary>
	/// Indicates the detail data of UL type 2.
	/// </summary>
	public sealed class UniqueLoopType2DetailData : UniqueLoopDetailData
	{
		/// <summary>
		/// Initializes an instance with the specified information.
		/// </summary>
		/// <param name="cells">All cells.</param>
		/// <param name="digits">All digits.</param>
		/// <param name="extraDigit">The extra digit.</param>
		public UniqueLoopType2DetailData(
			IReadOnlyList<int> cells, IReadOnlyList<int> digits, int extraDigit)
			: base(cells, digits) =>
			ExtraDigit = extraDigit;


		/// <summary>
		/// Indicates the extra digit in this pattern.
		/// </summary>
		public int ExtraDigit { get; }

		/// <inheritdoc/>
		public override int Type => 2;


		/// <inheritdoc/>
		public override string ToString()
		{
			string digitsStr = DigitCollection.ToString(Digits);
			int extraDigit = ExtraDigit + 1;
			string cellsStr = CellCollection.ToString(Cells);
			return $"{digitsStr} in cells {cellsStr} with extra digit {extraDigit}";
		}
	}
}
