﻿using System.Collections.Generic;
using Sudoku.Drawing;
using Sudoku.Solving.Utils;

namespace Sudoku.Solving.Manual.Uniqueness.Bugs
{
	/// <summary>
	/// Provides a usage of <b>bivalue universal grave</b> (BUG) type 2 technique.
	/// </summary>
	public sealed class BugType2TechniqueInfo : UniquenessTechniqueInfo
	{
		/// <summary>
		/// The table of extra difficulty values.
		/// </summary>
		private static readonly decimal[] DifficultyExtra = new[]
		{
			.1M, .2M, .2M, .3M, .3M, .3M, .4M, .4M, .4M, .4M,
			.5M, .5M, .5M, .5M, .5M, .6M, .6M, .6M, .6M, .6M
		};


		/// <summary>
		/// Initializes an instance with the specified information.
		/// </summary>
		/// <param name="conclusions">All conclusions.</param>
		/// <param name="views">All views.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="cells">All cell offsets.</param>
		public BugType2TechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			int digit, IReadOnlyList<int> cells)
			: base(conclusions, views) => (Digit, Cells) = (digit, cells);


		/// <summary>
		/// Indicates the digit.
		/// </summary>
		public int Digit { get; }

		/// <summary>
		/// Indicates the cell offsets.
		/// </summary>
		public IReadOnlyList<int> Cells { get; }

		/// <inheritdoc/>
		public override string Name => "Bivalue Universal Grave (Type 2)";

		/// <inheritdoc/>
		public override decimal Difficulty => 5.6M + DifficultyExtra[Cells.Count - 1];

		/// <inheritdoc/>
		public override DifficultyLevel DifficultyLevel => DifficultyLevel.Hard;


		/// <inheritdoc/>
		public override string ToString()
		{
			int digit = Digit + 1;
			string cellsStr = CellCollection.ToString(Cells);
			string elimStr = ConclusionCollection.ToString(Conclusions);
			return $"{Name}: {digit} with cells {cellsStr} => {elimStr}";
		}
	}
}