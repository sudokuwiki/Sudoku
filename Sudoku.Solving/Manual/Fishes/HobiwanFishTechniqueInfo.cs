﻿using System.Collections.Generic;
using System.Linq;
using Sudoku.Data;
using Sudoku.Data.Collections;
using Sudoku.Drawing;
using static Sudoku.Solving.Constants.Processings;

namespace Sudoku.Solving.Manual.Fishes
{
	/// <summary>
	/// Provides a usage of <b>Hobiwan's fish</b> technique.
	/// </summary>
	public sealed class HobiwanFishTechniqueInfo : FishTechniqueInfo
	{
		/// <summary>
		/// The basic difficulty rating table.
		/// </summary>
		private static readonly decimal[] BasicDiff = { 0, 0, 3.2M, 3.8M, 5.2M, 6M, 6M, 6.6M, 7M };

		/// <summary>
		/// The finned difficulty rating table.
		/// </summary>
		private static readonly decimal[] FinnedDiff = { 0, 0, .2M, .2M, .2M, .3M, .3M, .3M, .4M };

		/// <summary>
		/// The sashimi difficulty rating table.
		/// </summary>
		private static readonly decimal[] SashimiDiff = { 0, 0, .3M, .3M, .4M, .4M, .5M, .6M, .7M };

		/// <summary>
		/// The Franken shape extra difficulty rating table.
		/// </summary>
		private static readonly decimal[] FrankenShapeDiffExtra = { 0, 0, .2M, 1.2M, 1.2M, 1.3M, 1.3M, 1.3M, 1.4M };

		/// <summary>
		/// The mutant shape extra difficulty rating table.
		/// </summary>
		private static readonly decimal[] MutantShapeDiffExtra = { 0, 0, .3M, 1.4M, 1.4M, 1.5M, 1.5M, 1.5M, 1.6M };


		/// <summary>
		/// Initializes an instance with the specified information.
		/// </summary>
		/// <param name="conclusions">All conclusions.</param>
		/// <param name="views">All views.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="baseSets">The base sets.</param>
		/// <param name="coverSets">The cover sets.</param>
		/// <param name="exofins">The exo-fins.</param>
		/// <param name="endofins">The endo-fins.</param>
		/// <param name="isSashimi">Indicates the sashimi fish.</param>
		public HobiwanFishTechniqueInfo(
			IReadOnlyList<Conclusion> conclusions, IReadOnlyList<View> views,
			int digit, IReadOnlyList<int> baseSets, IReadOnlyList<int> coverSets,
			IReadOnlyList<int>? exofins, IReadOnlyList<int>? endofins, bool? isSashimi)
			: base(conclusions, views, digit, baseSets, coverSets) =>
			(ExofinCells, EndofinCells, IsSashimi) = (exofins, endofins, isSashimi);


		/// <summary>
		/// Indicates whether the fish instance is sashimi.
		/// The value can be:
		/// <list type="table">
		/// <item>
		/// <term><see langword="true"/></term><description>Sashimi finned fish.</description>
		/// </item>
		/// <item>
		/// <term><see langword="false"/></term><description>Normal finned fish.</description>
		/// </item>
		/// <item>
		/// <term><see langword="null"/></term><description>Normal fish.</description>
		/// </item>
		/// </list>
		/// </summary>
		public bool? IsSashimi { get; }

		/// <summary>
		/// Indicates all exo-fins.
		/// </summary>
		public IReadOnlyList<int>? ExofinCells { get; }

		/// <summary>
		/// Indicates all endo-fins.
		/// </summary>
		public IReadOnlyList<int>? EndofinCells { get; }

		/// <inheritdoc/>
		public override string Name
		{
			get
			{
				string name = FishNames[Size];
				string finModifier = IsSashimi switch
				{
					null => string.Empty,
					true => "Sashimi ",
					false => "Finned "
				};
				// Note that 'true switch' is a normal judger for each condition.
				// If the first condition is false, it will check the second one,
				// until the result is matched (default case, or i.e. discard).
				string shapeModifier = true switch
				{
					_ when IsBasic() => string.Empty,
					_ when IsFranken() => "Franken ",
					_ => "Mutant ",
				};
				return $"{finModifier}{shapeModifier}{name}";
			}
		}

		/// <inheritdoc/>
		public override decimal Difficulty
		{
			get
			{
				return BasicDiff[Size] + IsSashimi switch
				{
					false => FinnedDiff[Size],
					true => SashimiDiff[Size],
					null => 0
				} + true switch
				{
					_ when IsBasic() => 0,
					_ when IsFranken() => FrankenShapeDiffExtra[Size],
					_ => MutantShapeDiffExtra[Size],
				};
			}
		}

		/// <inheritdoc/>
		public override DifficultyLevel DifficultyLevel
		{
			get
			{
				return Size switch
				{
					2 => DifficultyLevel.Hard,
					3 => DifficultyLevel.VeryHard,
					4 => DifficultyLevel.VeryHard,
					_ => DifficultyLevel.Fiendish,
				};
			}
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			string elimStr = new ConclusionCollection(Conclusions).ToString();
			string baseSets = new RegionCollection(BaseSets).ToString();
			string coverSets = new RegionCollection(CoverSets).ToString();
			string exo = ExofinCells is null ? string.Empty : $"f{new CellCollection(ExofinCells).ToString()} ";
			string endo = EndofinCells is null ? string.Empty : $"ef{new CellCollection(EndofinCells).ToString()} ";
			return $@"{Name}: {Digit + 1} in {baseSets}\{coverSets} {exo}{endo}=> {elimStr}";
		}

		/// <summary>
		/// To check whether the specified structure is basic.
		/// </summary>
		/// <returns>A <see cref="bool"/> value.</returns>
		private bool IsBasic()
		{
			static bool rowJudger(int region) => region / 9 == 1;
			static bool columnJudger(int region) => region / 9 == 2;
			return BaseSets.All(rowJudger) && CoverSets.All(columnJudger)
				|| BaseSets.All(columnJudger) && CoverSets.All(rowJudger);
		}

		/// <summary>
		/// To check whether the specified structure is Franken.
		/// </summary>
		/// <returns>A <see cref="bool"/> value.</returns>
		private bool IsFranken()
		{
			for (int i = 0, count = BaseSets.Count; i < count - 1; i++)
			{
				for (int j = i + 1; j < count; j++)
				{
					int bs1 = BaseSets[i];
					int bs2 = BaseSets[j];
					if (bs1 / 9 == 0 || bs2 / 9 == 0)
					{
						goto Label_RowColumnCheck;
					}
				}
			}
			for (int i = 0, count = CoverSets.Count; i < count - 1; i++)
			{
				for (int j = i + 1; j < count; j++)
				{
					int cs1 = CoverSets[i];
					int cs2 = CoverSets[j];
					if (cs1 / 9 == 0 || cs2 / 9 == 0)
					{
						goto Label_RowColumnCheck;
					}
				}
			}

			return false;

		Label_RowColumnCheck:
			for (int i = 0, count = BaseSets.Count; i < count - 1; i++)
			{
				for (int j = i + 1; j < count; j++)
				{
					int bs1 = BaseSets[i];
					int bs2 = BaseSets[j];
					if (bs1 / 9 == 1 && bs2 / 9 == 2)
					{
						return false;
					}
				}
			}
			for (int i = 0, count = CoverSets.Count; i < count - 1; i++)
			{
				for (int j = i + 1; j < count; j++)
				{
					int cs1 = CoverSets[i];
					int cs2 = CoverSets[j];
					if (cs1 / 9 == 1 && cs2 / 9 == 2)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
