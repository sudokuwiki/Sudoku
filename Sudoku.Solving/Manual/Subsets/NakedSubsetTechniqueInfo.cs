﻿using System.Collections.Generic;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Drawing;
using Sudoku.Solving.Utils;

namespace Sudoku.Solving.Manual.Subsets
{
	/// <summary>
	/// Provides a usage of <b>naked subset</b> technique.
	/// </summary>
	public sealed class NakedSubsetTechniqueInfo : SubsetTechniqueInfo
	{
		/// <summary>
		/// Provides passing data when initializing an instance of derived types.
		/// </summary>
		/// <param name="conclusions">The conclusions.</param>
		/// <param name="views">The views of this solving step.</param>
		/// <param name="regionOffset">The region offset.</param>
		/// <param name="cellOffsets">The cell offsets.</param>
		/// <param name="digits">The digits.</param>
		/// <param name="isLocked">Indicates whether the technique is locked. </param>
		public NakedSubsetTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			int regionOffset, IReadOnlyList<int> cellOffsets, IReadOnlyList<int> digits,
			bool? isLocked) : base(conclusions, views, regionOffset, cellOffsets, digits) =>
			IsLocked = isLocked;


		/// <summary>
		/// Represents a value for this technique is a locked,
		/// partial locked or normal subset.
		/// The technique is one when the value is:
		/// <list type="table">
		/// <item><term><see langword="true"/></term><description>Locked subset,</description></item>
		/// <item><term><see langword="false"/></term><description>Partial locked subset,</description></item>
		/// <item><term><see langword="null"/></term><description>Normal subset.</description></item>
		/// </list>
		/// </summary>
		public bool? IsLocked { get; }

		/// <summary>
		/// Indicates the size of this instance.
		/// </summary>
		public int Size => Digits.Count;

		/// <inheritdoc/>
		public override string Name
		{
			get
			{
				return IsLocked switch
				{
					null => $"Naked {base.Name}",
					true => $"Locked {base.Name}",
					false => $"Naked {base.Name} (+)"
				};
			}
		}

		/// <inheritdoc/>
		public override decimal Difficulty
		{
			get
			{
				return Size switch
				{
					2 => 3M,
					3 => 3.6M,
					4 => 5M,
					_ => throw Throwing.ImpossibleCase
				} + IsLocked switch
				{
					null => 0,
					true => Size switch
					{
						2 => -1M,
						3 => -1.1M,
						_ => throw Throwing.ImpossibleCase
					},
					false => .1M
				};
			}
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			string digitsStr = new DigitCollection(Digits).ToString();
			string regionStr = new RegionCollection(RegionOffset).ToString();
			string elimStr = new ConclusionCollection(Conclusions).ToString();
			return $"{Name}: {digitsStr} in {regionStr} => {elimStr}";
		}
	}
}
