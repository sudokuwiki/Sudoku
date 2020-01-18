﻿namespace Sudoku.Debugging
{
	internal static class Program
	{
		private static void Main()
		{
			var solver = new Sudoku.Solving.Manual.ManualSolver
			{
				//IttoRyuWhenPossible = true,
				OptimizedApplyingOrder = true,
				EnableFullHouse = true,
				EnableLastDigit = true
			};
			var grid = Sudoku.Data.Meta.Grid.Parse(
				"206000140000000000410000020005030200000000004000816300000302601004051703150070000");
			var analysisResult = solver.Solve(grid);
			System.Console.WriteLine(analysisResult);
		}
	}
}
