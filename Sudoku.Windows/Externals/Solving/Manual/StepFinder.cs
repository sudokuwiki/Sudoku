﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sudoku.Data;
using Sudoku.Windows;
using Sudoku.Solving.Checking;
using Sudoku.Solving.Manual.Alses;
using Sudoku.Solving.Manual.Chaining;
using Sudoku.Solving.Manual.Fishes;
using Sudoku.Solving.Manual.Intersections;
using Sudoku.Solving.Manual.LastResorts;
using Sudoku.Solving.Manual.Sdps;
using Sudoku.Solving.Manual.Singles;
using Sudoku.Solving.Manual.Subsets;
using Sudoku.Solving.Manual.Uniqueness.Bugs;
using Sudoku.Solving.Manual.Uniqueness.Loops;
using Sudoku.Solving.Manual.Uniqueness.Polygons;
using Sudoku.Solving.Manual.Uniqueness.Rectangles;
using Sudoku.Solving.Manual.Wings.Irregular;
using Sudoku.Solving.Manual.Wings.Regular;
using Intersection = System.ValueTuple<int, int, Sudoku.Data.GridMap, Sudoku.Data.GridMap>;
using TechniqueGroup = System.Linq.IGrouping<string, Sudoku.Solving.TechniqueInfo>;

namespace Sudoku.Solving.Manual
{
	/// <summary>
	/// Provides a step finder.
	/// </summary>
	public sealed class StepFinder
	{
		/// <summary>
		/// The settings used.
		/// </summary>
		private readonly Settings _settings;


		/// <summary>
		/// Initializes an instance with the specified settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public StepFinder(Settings settings) => _settings = settings;


		/// <summary>
		/// Search for all possible steps in a grid.
		/// </summary>
		/// <param name="grid">The grid.</param>
		public async Task<IEnumerable<TechniqueGroup>> SearchAsync(IReadOnlyGrid grid)
		{
			if (grid.HasSolved || !grid.IsValid(out _))
			{
				return Array.Empty<TechniqueGroup>();
			}

			// Get all region maps.
			var regionMaps = new GridMap[27];
			for (int i = 0; i < 27; i++)
			{
				regionMaps[i] = GridMap.CreateInstance(i);
			}

			// Get intersection table in order to run faster in intersection technique searchers.
			var intersection = new Intersection[18, 3];
			int[] key = { 0, 3, 6, 1, 4, 7, 2, 5, 8 };
			for (int i = 0; i < 18; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					int baseSet = i + 9;
					int coverSet = i < 9 ? i / 3 * 3 + j : key[(i - 9) / 3 * 3 + j];
					intersection[i, j] = (
						baseSet, coverSet, regionMaps[baseSet],
						regionMaps[coverSet]);
				}
			}

			var searchers = new TechniqueSearcher[]
			{
				new SingleTechniqueSearcher(_settings.EnableFullHouse, _settings.EnableLastDigit),
				new LcTechniqueSearcher(intersection),
				new SubsetTechniqueSearcher(),
				new NormalFishTechniqueSearcher(),
				new RegularWingTechniqueSearcher(_settings.CheckRegularWingSize),
				new IrregularWingTechniqueSearcher(),
				new TwoStrongLinksTechniqueSearcher(),
				new UrTechniqueSearcher(_settings.CheckIncompletedUniquenessPatterns),
				new XrTechniqueSearcher(),
				new UlTechniqueSearcher(),
				new EmptyRectangleTechniqueSearcher(regionMaps),
				new AlcTechniqueSearcher(intersection, _settings.CheckAlmostLockedQuadruple),
				new SdcTechniqueSearcher(regionMaps),
				new BdpTechniqueSearcher(),
				new BugTechniqueSearcher(regionMaps, _settings.UseExtendedBugSearcher),
				new AlsXzTechniqueSearcher(_settings.AllowOverlapAlses, _settings.AlsHighlightRegionInsteadOfCell),
				new AlsXyWingTechniqueSearcher(_settings.AllowOverlapAlses, _settings.AlsHighlightRegionInsteadOfCell),
				new DeathBlossomTechniqueSearcher(
					regionMaps, _settings.AllowOverlapAlses, _settings.AlsHighlightRegionInsteadOfCell),
				new GroupedAicTechniqueSearcher(
					true, false, false, _settings.AicMaximumLength, _settings.ReductDifferentPathAic,
					_settings.OnlySaveShortestPathAic, _settings.CheckHeadCollision,
					_settings.CheckContinuousNiceLoop, regionMaps),
				new GroupedAicTechniqueSearcher(
					false, true, false, _settings.AicMaximumLength, _settings.ReductDifferentPathAic,
					_settings.OnlySaveShortestPathAic, _settings.CheckHeadCollision,
					_settings.CheckContinuousNiceLoop, regionMaps),
				new GroupedAicTechniqueSearcher(
					false, false, true, _settings.AicMaximumLength, _settings.ReductDifferentPathAic,
					_settings.OnlySaveShortestPathAic, _settings.CheckHeadCollision,
					_settings.CheckContinuousNiceLoop, regionMaps),
				//new HobiwanFishTechniqueSearcher(
				//	HobiwanFishMaximumSize, HobiwanFishMaximumExofinsCount,
				//	HobiwanFishMaximumEndofinsCount, HobiwanFishCheckTemplates, regionMaps),
				new BowmanBingoTechniqueSearcher(_settings.BowmanBingoMaximumLength),
				new PomTechniqueSearcher(),
				new TemplateTechniqueSearcher(false),
				new CccTechniqueSearcher(),
			};

			var bag = new Bag<TechniqueInfo>();
			foreach (var searcher in searchers)
			{
				await Task.Run(() => searcher.AccumulateAll(bag, grid));
			}

			return await Task.Run(() => from step in bag group step by step.Name);
		}
	}
}