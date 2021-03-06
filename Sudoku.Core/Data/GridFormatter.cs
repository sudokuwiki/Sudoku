﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Sudoku.Extensions;

namespace Sudoku.Data
{
	/// <summary>
	/// Provides operations for grid formatting.
	/// </summary>
	[DebuggerStepThrough]
	internal sealed partial class GridFormatter
	{
		/// <summary>
		/// Initializes an instance with a <see cref="bool"/> value
		/// indicating multi-line.
		/// </summary>
		/// <param name="multiline">
		/// The multi-line identifier. If the value is <see langword="true"/>, the output will
		/// be multi-line.
		/// </param>
		public GridFormatter(bool multiline) => Multiline = multiline;


		/// <summary>
		/// Indicates whether the output should be multi-line.
		/// </summary>
		public bool Multiline { get; }


		/// <summary>
		/// Represents a string value indicating this instance.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <returns>The string.</returns>
		public string ToString(Grid grid)
		{
			return Multiline
				? WithCandidates ? ToMultiLineStringCore(grid) : ToMultiLineSimpleGridCore(grid)
				: HodokuCompatible ? ToHodokuLibraryFormatString(grid) : ToSingleLineStringCore(grid);
		}

		/// <summary>
		/// To string with Hodoku library format compatible string.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <returns>The string.</returns>
		private string ToHodokuLibraryFormatString(Grid grid)
		{
			string result = ToSingleLineStringCore(grid);
			return $":0000:x:{result}:::";
		}

		/// <summary>
		/// To single line string.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <returns>The result.</returns>
		private string ToSingleLineStringCore(Grid grid)
		{
			var sb = new StringBuilder();
			var elims = new StringBuilder();
			Grid tempGrid = null!; // This assignment is very dangerous (Non-nullable is assigned null)!
			if (WithCandidates)
			{
				// Get a temp grid only used for checking.
				tempGrid = Grid.Parse(grid.ToString(".+"));
			}

			int offset = 0;
			foreach (short value in grid)
			{
				switch (GetCellStatus(value))
				{
					case CellStatus.Empty:
					{
						if (WithCandidates)
						{
							for (int i = 0, temp = value; i < 9; i++, temp >>= 1)
							{
								// Check if the value has been set 'true'
								// and the value has already deleted at the grid
								// with only givens and modifiables.
								if ((temp & 1) != 0 && !tempGrid[offset, i])
								{
									// The value is 'true', which means that
									// The digit has been deleted.
									elims.Append($"{i + 1}{offset / 9 + 1}{offset % 9 + 1} ");
								}
							}
						}

						sb.Append(Placeholder);
						break;
					}
					case CellStatus.Modifiable:
					{
						sb.Append(
							WithModifiables
								? $"+{GetFirstFalseCandidate(value) + 1}"
								: $"{Placeholder}");

						break;
					}
					case CellStatus.Given:
					{
						sb.Append($"{GetFirstFalseCandidate(value) + 1}");
						break;
					}
					default:
					{
						break;
					}
				}

				offset++;
			}

			string elimsStr = elims.ToString();
			return $"{sb}{(string.IsNullOrEmpty(elimsStr) ? string.Empty : $":{elimsStr}")}";
		}

		/// <summary>
		/// Get the first <see langword="false"/> candidate.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The first one.</returns>
		private static int GetFirstFalseCandidate(short value)
		{
			// To truncate the value to range 0 to 511.
			value = (short)~(value & 511);

			// Special case: the value is 0.
			if (value == 0)
			{
				return -1;
			}
			else
			{
				for (int i = 0; i < 9; i++, value >>= 1)
				{
					if ((value & 1) != 0)
					{
						return i;
					}
				}

				// All values are 0, which means the value is 0,
				// so return -1 is necessary, even though the special case has been
				// handled before.
				return -1;
			}
		}

		/// <summary>
		/// To multi-line normal grid string without any candidates.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <returns>The result.</returns>
		private string ToMultiLineSimpleGridCore(Grid grid)
		{
			string t = grid.ToString(TreatValueAsGiven ? $"{Placeholder}!" : $"{Placeholder}");
			return new StringBuilder()
				.AppendLine(SubtleGridLines ? ".-------+-------+-------." : "+-------+-------+-------+")
				.AppendLine($"| {t[0]} {t[1]} {t[2]} | {t[3]} {t[4]} {t[5]} | {t[6]} {t[7]} {t[8]} |")
				.AppendLine($"| {t[9]} {t[10]} {t[11]} | {t[12]} {t[13]} {t[14]} | {t[15]} {t[16]} {t[17]} |")
				.AppendLine($"| {t[18]} {t[19]} {t[20]} | {t[21]} {t[22]} {t[23]} | {t[24]} {t[25]} {t[26]} |")
				.AppendLine(SubtleGridLines ? ":-------+-------+-------:" : "+-------+-------+-------+")
				.AppendLine($"| {t[27]} {t[28]} {t[29]} | {t[30]} {t[31]} {t[32]} | {t[33]} {t[34]} {t[35]} |")
				.AppendLine($"| {t[36]} {t[37]} {t[38]} | {t[39]} {t[40]} {t[41]} | {t[42]} {t[43]} {t[44]} |")
				.AppendLine($"| {t[45]} {t[46]} {t[47]} | {t[48]} {t[49]} {t[50]} | {t[51]} {t[52]} {t[53]} |")
				.AppendLine(SubtleGridLines ? ":-------+-------+-------:" : "+-------+-------+-------+")
				.AppendLine($"| {t[54]} {t[55]} {t[56]} | {t[57]} {t[58]} {t[59]} | {t[60]} {t[61]} {t[62]} |")
				.AppendLine($"| {t[63]} {t[64]} {t[65]} | {t[66]} {t[67]} {t[68]} | {t[69]} {t[70]} {t[71]} |")
				.AppendLine($"| {t[72]} {t[73]} {t[74]} | {t[75]} {t[76]} {t[77]} | {t[78]} {t[79]} {t[80]} |")
				.AppendLine(SubtleGridLines ? "'-------+-------+-------'" : "+-------+-------+-------+")
				.ToString();
		}

		/// <summary>
		/// To multi-line string with candidates.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <returns>The result.</returns>
		private string ToMultiLineStringCore(Grid grid)
		{
			// Step 1: gets the candidates information grouped by columns.
			var valuesByColumn = GetDefaultList();
			var valuesByRow = GetDefaultList();
			for (int i = 0; i < 81; i++)
			{
				short value = grid.GetMask(i);
				valuesByRow[i / 9].Add(value);
				valuesByColumn[i % 9].Add(value);
			}

			// Step 2: gets the maximal number of candidates in a cell,
			// which is used for aligning by columns.
			int[] maxLengths = new int[9];
			foreach (var (i, values) in valuesByColumn)
			{
				ref int maxLength = ref maxLengths[i];

				// Iteration on row index.
				for (int j = 0; j < 9; j++)
				{
					// Gets the number of candidates.
					int candidatesCount = 0;
					short value = valuesByColumn[i][j];

					// Iteration on each candidate.
					// Counts the number of candidates.
					for (int k = 0, copy = value; k < 9; k++, copy >>= 1)
					{
						if ((copy & 1) == 0)
						{
							candidatesCount++;
						}
					}

					// Compares the values.
					int comparer = Math.Max(candidatesCount, GetCellStatus(value) switch
					{
						// The output will be '<digit>' and consist of 3 characters.
						CellStatus.Given => Math.Max(candidatesCount, 3),
						// The output will be '*digit*' and consist of 3 characters.
						CellStatus.Modifiable => Math.Max(candidatesCount, 3),
						// Normal output: 'series' (at least 1 character).
						_ => candidatesCount,
					});
					if (comparer > maxLength)
					{
						maxLength = comparer;
					}
				}
			}

			// Step 3: outputs all characters.
			var sb = new StringBuilder();
			for (int i = 0; i < 13; i++)
			{
				switch (i)
				{
					case 0: // Print tabs of the first line.
					{
						if (SubtleGridLines) PrintTabLines('.', '.', '-', maxLengths, sb);
						else PrintTabLines('+', '+', '-', maxLengths, sb);
						break;
					}
					case 4:
					case 8: // Print tabs of mediate lines.
					{
						if (SubtleGridLines) PrintTabLines(':', '+', '-', maxLengths, sb);
						else PrintTabLines('+', '+', '-', maxLengths, sb);
						break;
					}
					case 12: // Print tabs of the foot line.
					{
						if (SubtleGridLines) PrintTabLines('\'', '\'', '-', maxLengths, sb);
						else PrintTabLines('+', '+', '-', maxLengths, sb);
						break;
					}
					default: // Print values and tabs.
					{
						PrintValueLines(valuesByRow[i switch
						{
							_ when i >= 1 && i < 4 => i - 1,
							_ when i >= 5 && i < 8 => i - 2,
							_ when i >= 9 && i < 12 => i - 3,
							_ => throw Throwing.ImpossibleCaseWithMessage("On the border.")
						}], '|', '|', maxLengths, sb);
						break;
					}
				}
			}

			// The last step: returns the value.
			return sb.ToString();
		}

		private void PrintValueLines(IList<short> valuesByRow, char c1, char c2, int[] maxLengths, StringBuilder sb)
		{
			sb.Append(c1);
			PrintValues(valuesByRow, 0, 2, maxLengths, sb);
			sb.Append(c2);
			PrintValues(valuesByRow, 3, 5, maxLengths, sb);
			sb.Append(c2);
			PrintValues(valuesByRow, 6, 8, maxLengths, sb);
			sb.AppendLine(c1);
		}

		private void PrintValues(IList<short> valuesByRow, int start, int end, int[] maxLengths, StringBuilder sb)
		{
			sb.Append(" ");
			for (int i = start; i <= end; i++)
			{
				// Get digit.
				short value = valuesByRow[i];
				var cellStatus = GetCellStatus(value);
				int digit = cellStatus != CellStatus.Empty ? (~value).FindFirstSet() : -1;

				sb.Append((cellStatus switch
				{
					CellStatus.Given => $"<{digit + 1}>",
					CellStatus.Modifiable => TreatValueAsGiven ? $"<{digit + 1}>" : $"*{digit + 1}*",
					_ => PrintCandidates(value)
				}).PadRight(maxLengths[i]));
				sb.Append(i != end ? "  " : " ");
			}
		}

		private static string PrintCandidates(short value)
		{
			var sb = new StringBuilder();
			for (int i = 1; i <= 9; i++, value >>= 1)
			{
				if ((value & 1) == 0)
				{
					sb.Append(i);
				}
			}

			return sb.ToString();
		}

		private static void PrintTabLines(char c1, char c2, char fillingChar, int[] maxLengths, StringBuilder sb)
		{
			sb.Append(c1);
			sb.Append(string.Empty.PadRight(maxLengths[0] + maxLengths[1] + maxLengths[2] + 6, fillingChar));
			sb.Append(c2);
			sb.Append(string.Empty.PadRight(maxLengths[3] + maxLengths[4] + maxLengths[5] + 6, fillingChar));
			sb.Append(c2);
			sb.Append(string.Empty.PadRight(maxLengths[6] + maxLengths[7] + maxLengths[8] + 6, fillingChar));
			sb.AppendLine(c1);
		}

		/// <summary>
		/// Get cell status for a value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The cell status.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static CellStatus GetCellStatus(short value) => (CellStatus)(value >> 9 & (int)CellStatus.All);

		/// <summary>
		/// Get the default list.
		/// </summary>
		/// <returns>The list.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Dictionary<int, IList<short>> GetDefaultList()
		{
			return new Dictionary<int, IList<short>>
			{
				[0] = new List<short>(),
				[1] = new List<short>(),
				[2] = new List<short>(),
				[3] = new List<short>(),
				[4] = new List<short>(),
				[5] = new List<short>(),
				[6] = new List<short>(),
				[7] = new List<short>(),
				[8] = new List<short>()
			};
		}
	}
}
