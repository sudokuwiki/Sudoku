﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Externals;
using System.Runtime.CompilerServices;
using Sudoku.Data.Extensions;

namespace Sudoku.Data
{
	/// <summary>
	/// Encapsulates a basic sudoku grid.
	/// </summary>
	[DebuggerStepThrough]
	public class Grid : ICloneable<Grid>, IEnumerable, IEnumerable<short>, IEquatable<Grid>, IReadOnlyGrid
	{
		/// <summary>
		/// Indicates the empty grid string.
		/// </summary>
		public const string EmptyString = "000000000000000000000000000000000000000000000000000000000000000000000000000000000";

		/// <summary>
		/// Indicates an empty grid, where all values are zero.
		/// </summary>
		public static readonly IReadOnlyGrid Empty = new Grid();


		/// <summary>
		/// Binary masks of all cells.
		/// </summary>
		/// <remarks>
		/// <para>This array stores binary representation of decimals.</para>
		/// <para>
		/// There are 81 cells in a sudoku grid, so this data structure uses
		/// an array of size 81. Each element is a <see cref="short"/> value,
		/// where the lower 9 bits indicates if the digit 1 to 9 exists or not.
		/// If the value is <see langword="true"/> (i.e. binary is for 1), this digit will
		/// <b>not</b> be exist. The higher 3 bits indicates the cell status. The
		/// cases are below:
		/// <list type="table">
		/// <item>
		/// <term>0b001 (1)</term>
		/// <description>The cell is <see cref="CellStatus.Empty"/>.</description>
		/// </item>
		/// <item>
		/// <term>0b010 (2)</term>
		/// <description>The cell is <see cref="CellStatus.Modifiable"/>.</description>
		/// </item>
		/// <item>
		/// <term>0b100 (4)</term>
		/// <description>The cell is <see cref="CellStatus.Given"/>.</description>
		/// </item>
		/// </list>
		/// </para>
		/// </remarks>
		/// <seealso cref="CellStatus"/>
		protected internal readonly short[] _masks;

		/// <summary>
		/// Same as <see cref="_masks"/>, but this field stores the all masks at
		/// the initial grid. The field will not be modified until this instance
		/// destructs.
		/// </summary>
		protected internal readonly short[] _initialMasks;


		/// <summary>
		/// Initializes an instance with the binary mask array.
		/// </summary>
		/// <param name="masks">The mask array.</param>
		/// <exception cref="ArgumentException">
		/// Throws when the length of the specified argument is not 81.
		/// </exception>
		public Grid(short[] masks)
		{
			if (masks.Length != 81)
			{
				throw new ArgumentException(
					message: "The specified argument is invalid, because the length of this argument is not 81.",
					paramName: nameof(masks));
			}

			_masks = masks;
			_initialMasks = (short[])masks.Clone();

			ValueChanged += OnValueChanged;
		}

		/// <summary>
		/// Provides default initialization.
		/// </summary>
		private Grid()
		{
			// 512 is equal to binary number '0b01_000_000_000', where the higher 2 bits
			// can be combined a binary number of cell status.
			_masks = new short[81];
			for (int i = 0; i < 81; i++)
			{
				_masks[i] = (short)CellStatus.Empty << 9;
			}
			_initialMasks = (short[])_masks.Clone();

			// Initializes the event handler.
			// Note that the default event initialization hides the back delegate field,
			// so we should use 'fake' event field to trigger the event by
			// 'Event.Invoke(objectToTrigger, eventArg)', where the variable
			// 'objectToTrigger' is always 'this'.
			ValueChanged += OnValueChanged;
		}


		/// <inheritdoc/>
		public bool HasSolved
		{
			get
			{
				for (int i = 0; i < 81; i++)
				{
					if (GetCellStatus(i) == CellStatus.Empty)
					{
						return false;
					}
				}

				return SimplyValidate();
			}
		}

		/// <inheritdoc/>
		public virtual int this[int offset]
		{
			get
			{
				if (GetCellStatus(offset) == CellStatus.Empty)
				{
					// Empty cells does not have a fixed value.
					return -1;
				}
				else
				{
					short mask = _masks[offset];
					for (int i = 0; i < 9; i++, mask >>= 1)
					{
						if ((mask & 1) == 0)
						{
							return i;
						}
					}

					// Modifiables and givens contain no fixed digit? What the hell?
					return -1;
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				if (value >= 0 && value < 9)
				{
					ref short result = ref _masks[offset];
					short copy = result;

					// Set cell status to 'CellStatus.Modifiable'.
					result = (short)((short)CellStatus.Modifiable << 9 | 511 & ~(1 << value));

					// To trigger the event, which is used for eliminate
					// all same candidates in peer cells.
					ValueChanged.Invoke(this, new ValueChangedEventArgs(offset, copy, result, value));
				}
				else if (value == -1)
				{
					// If 'value' is -1, we should reset the grid.
					// Note that reset candidates may not trigger the event.
					if (GetCellStatus(offset) == CellStatus.Modifiable)
					{
						Reset();
					}
				}
			}
		}

		/// <inheritdoc/>
		public virtual bool this[int offset, int digit]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_masks[offset] >> digit & 1) != 0;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				ref short result = ref _masks[offset];
				short copy = result;
				if (value)
				{
					result |= (short)(1 << digit);
				}
				else
				{
					result &= (short)~(1 << digit);
				}

				// To trigger the event.
				ValueChanged.Invoke(this, new ValueChangedEventArgs(offset, copy, result, -1));
			}
		}


		/// <summary>
		/// Indicates the event when the mask in a certain cell has changed.
		/// </summary>
		public event ValueChangedEventHandler ValueChanged;


		/// <summary>
		/// To fix a grid, which means all modifiable values will be changed
		/// to given ones.
		/// </summary>
		public virtual void Fix()
		{
			for (int i = 0; i < 81; i++)
			{
				if (GetCellStatus(i) == CellStatus.Modifiable)
				{
					SetCellStatus(i, CellStatus.Given);
				}
			}
		}

		/// <summary>
		/// To unfix a grid, which means all given values will be changed
		/// to modifiable ones.
		/// </summary>
		public virtual void Unfix()
		{
			for (int i = 0; i < 81; i++)
			{
				if (GetCellStatus(i) == CellStatus.Given)
				{
					SetCellStatus(i, CellStatus.Modifiable);
				}
			}
		}

		/// <summary>
		/// To reset the grid.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void Reset() => Array.Copy(_initialMasks, _masks, 81);

		/// <summary>
		/// Set the status in a cell.
		/// </summary>
		/// <param name="offset">The cell offset you want to change.</param>
		/// <param name="cellStatus">The cell status you want to set.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void SetCellStatus(int offset, CellStatus cellStatus)
		{
			ref short mask = ref _masks[offset];
			short copy = mask;
			mask = (short)((int)cellStatus << 9 | mask & 511);

			ValueChanged.Invoke(this, new ValueChangedEventArgs(offset, copy, mask, -1));
		}

		/// <summary>
		/// Set a mask in a cell.
		/// </summary>
		/// <param name="offset">The cell offset you want to change.</param>
		/// <param name="value">The cell mask you want to set.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void SetMask(int offset, short value)
		{
			ref short mask = ref _masks[offset];
			short copy = mask;
			mask = value;

			ValueChanged.Invoke(this, new ValueChangedEventArgs(offset, copy, mask, -1));
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			obj is Grid comparer && Equals(comparer);

		/// <inheritdoc/>
		public bool Equals(Grid other)
		{
			for (int i = 0; i < 81; i++)
			{
				if (_masks[i] != other._masks[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int result = GetType().GetHashCode() ^ nameof(_masks).GetHashCode();

			for (int i = 0; i < 81; i++)
			{
				result ^= (i + 1) * _masks[i];
			}

			return result;
		}

		/// <inheritdoc/>
		public int[] ToArray()
		{
			int[] result = new int[81];
			for (int i = 0; i < 81; i++)
			{
				// 'this[i]' is always in range -1 to 8 (-1 is empty, and 0 to 8 is 1 to 9 for
				// mankind representation).
				result[i] = this[i] + 1;
			}

			return result;
		}

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short GetMask(int offset) => _masks[offset];

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short GetCandidates(int offset) => (short)(_masks[offset] & 511);

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short GetCandidatesReversal(int offset) => (short)(~_masks[offset] & 511);

		/// <inheritdoc/>
		public sealed override string ToString() => ToString(null, null);

		/// <summary>
		/// Returns a string that represents the current object, with the
		/// specified format.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <returns>The string result.</returns>
		public string ToString(string format) => ToString(format, null);

		/// <inheritdoc/>
		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			if (formatProvider.HasFormatted(this, format, out string? result))
			{
				return result;
			}

			// Format checking.
			CheckFormatString(format ?? throw new ArgumentNullException(nameof(format)));

			// Returns the grid string.
			// Here you can also use switch expression to return.
			switch (format)
			{
				case null:
				case ".":
				{
					return new GridFormatter(this, false).ToString();
				}
				case "+":
				case ".+":
				case "+.":
				{
					return new GridFormatter(this, false)
					{
						WithModifiables = true
					}.ToString();
				}
				case "0":
				{
					return new GridFormatter(this, false)
					{
						Placeholder = '0'
					}.ToString();
				}
				case ":":
				{
					return new GridFormatter(this, false)
					{
						WithCandidates = true
					}.ToString().Match(@"(?<=\:)(\d{3}\s+)*\d{3}").NullableToString();
				}
				case "!":
				case ".!":
				case "!.":
				{
					return new GridFormatter(this, false)
					{
						WithModifiables = true
					}.ToString().Replace("+", string.Empty);
				}
				case "0!":
				case "!0":
				{
					return new GridFormatter(this, false)
					{
						Placeholder = '0',
						WithModifiables = true
					}.ToString().Replace("+", string.Empty);
				}
				case ".:":
				{
					return new GridFormatter(this, false)
					{
						WithCandidates = true
					}.ToString();
				}
				case "0:":
				{
					return new GridFormatter(this, false)
					{
						Placeholder = '0',
						WithCandidates = true
					}.ToString();
				}
				case "0+":
				case "+0":
				{
					return new GridFormatter(this, false)
					{
						Placeholder = '0',
						WithModifiables = true
					}.ToString();
				}
				case "+:":
				case "+.:":
				case ".+:":
				{
					return new GridFormatter(this, false)
					{
						WithModifiables = true,
						WithCandidates = true
					}.ToString();
				}
				case "0+:":
				case "+0:":
				{
					return new GridFormatter(this, false)
					{
						Placeholder = '0',
						WithModifiables = true,
						WithCandidates = true
					}.ToString();
				}
				case ".!:":
				case "!.:":
				{
					return new GridFormatter(this, false)
					{
						WithModifiables = true
					}.ToString().Replace("+", string.Empty);
				}
				case "0!:":
				case "!0:":
				{
					return new GridFormatter(this, false)
					{
						Placeholder = '0',
						WithModifiables = true
					}.ToString().Replace("+", string.Empty);
				}
				case "#":
				{
					// Formats representing 'intelligence processor' is equal to
					// format '.+:' and '0+:'.
					goto case ".+:";
				}
				case "#0":
				{
					goto case ".+:";
				}
				case "#.":
				{
					goto case "0+:";
				}
				case "@":
				case "@.":
				{
					return new GridFormatter(this, true)
					{
						SubtleGridLines = true
					}.ToString();
				}
				case "@0":
				{
					return new GridFormatter(this, true)
					{
						Placeholder = '0',
						SubtleGridLines = true
					}.ToString();
				}
				case "@!":
				case "@.!":
				case "@!.":
				{
					return new GridFormatter(this, true)
					{
						TreatValueAsGiven = true,
						SubtleGridLines = true
					}.ToString();
				}
				case "@0!":
				case "@!0":
				{
					return new GridFormatter(this, true)
					{
						Placeholder = '0',
						TreatValueAsGiven = true,
						SubtleGridLines = true
					}.ToString();
				}
				case "@*":
				case "@.*":
				case "@*.":
				{
					return new GridFormatter(this, true).ToString();
				}
				case "@0*":
				case "@*0":
				{
					return new GridFormatter(this, true)
					{
						Placeholder = '0'
					}.ToString();
				}
				case "@!*":
				case "@*!":
				{
					return new GridFormatter(this, true)
					{
						TreatValueAsGiven = true
					}.ToString();
				}
				case "@:":
				{
					return new GridFormatter(this, true)
					{
						WithCandidates = true,
						SubtleGridLines = true
					}.ToString();
				}
				case "@:!":
				case "@!:":
				{
					return new GridFormatter(this, true)
					{
						WithCandidates = true,
						TreatValueAsGiven = true,
						SubtleGridLines = true
					}.ToString();
				}
				case "@*:":
				case "@:*":
				{
					return new GridFormatter(this, true)
					{
						WithCandidates = true
					}.ToString();
				}
				case "@!*:":
				case "@*!:":
				case "@!:*":
				case "@*:!":
				case "@:!*":
				case "@:*!":
				{
					return new GridFormatter(this, true)
					{
						WithCandidates = true,
						TreatValueAsGiven = true
					}.ToString();
				}
				default:
				{
					throw new FormatException("The specified format is invalid.");
				}
			}
		}

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CellStatus GetCellStatus(int offset) =>
			(CellStatus)(_masks[offset] >> 9 & (int)CellStatus.All);

		/// <inheritdoc/>
		public Grid Clone() => new Grid((short[])_masks.Clone());

		/// <inheritdoc/>
		public IEnumerator<short> GetEnumerator() =>
			((IEnumerable<short>)_masks).GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// The method, which will be invoked when the mask has changed.
		/// </summary>
		/// <param name="sender">The instance triggering the event.</param>
		/// <param name="e">The data.</param>
		private void OnValueChanged(object sender, ValueChangedEventArgs e)
		{
			var (offset, _, _, setValue) = e;
			if (setValue != -1)
			{
				foreach (int peerOffset in new GridMap(offset).Offsets)
				{
					if (peerOffset == offset)
					{
						// Same cell.
						continue;
					}

					// To check if the peer cell is empty or not.
					if (GetCellStatus(peerOffset) == CellStatus.Empty)
					{
						ref short peerValue = ref _masks[peerOffset];
						peerValue |= (short)(1 << setValue);
					}
				}
			}
		}

		/// <summary>
		/// Simply validate.
		/// </summary>
		/// <returns>The <see cref="bool"/> result.</returns>
		private bool SimplyValidate()
		{
			int count = 0;
			for (int i = 0; i < 81; i++)
			{
				if (GetCellStatus(i) == CellStatus.Given)
				{
					count++;
				}

				int curDigit, peerDigit;
				if (GetCellStatus(i) != CellStatus.Empty)
				{
					curDigit = this[i];
					foreach (int peerOffset in new GridMap(i).Offsets)
					{
						if (peerOffset == i)
						{
							continue;
						}

						if ((peerDigit = this[peerOffset]) != -1 && curDigit == peerDigit)
						{
							return false;
						}
					}
				}
			}

			// Each unique puzzle has at least 17 hints.
			return count >= 17;
		}


		/// <summary>
		/// <para>
		/// Parses a string value and converts to this type.
		/// </para>
		/// <para>
		/// If you want to parse a PM grid, we recommend you use the method
		/// <see cref="Parse(string, GridParsingType)"/> instead of this method.
		/// </para>
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>The result instance had converted.</returns>
		/// <seealso cref="Parse(string, GridParsingType)"/>
		public static Grid Parse(string str) => new GridParser(str).Parse();

		/// <summary>
		/// Parses a string value and converts to this type,
		/// using a specified grid parsing type.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="gridParsingType">The grid parsing type.</param>
		/// <returns>The result instance had converted.</returns>
		public static Grid Parse(string str, GridParsingType gridParsingType) =>
			new GridParser(str).Parse(gridParsingType);

		/// <summary>
		/// Try to parse a string and converts to this type, and returns a
		/// <see cref="bool"/> value indicating the result of the conversion.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="result">
		/// (<see langword="out"/> parameter) The result parsed. If the conversion is failed,
		/// this argument will be <see langword="null"/>.
		/// </param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		public static bool TryParse(string str, [NotNullWhen(true)] out Grid? result)
		{
			try
			{
				result = Parse(str);
				return true;
			}
			catch
			{
				result = null;
				return false;
			}
		}

		/// <summary>
		/// To check the format string, delegated from the method
		/// <see cref="ToString(string, IFormatProvider)"/>.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <seealso cref="ToString(string, IFormatProvider)"/>
		private static void CheckFormatString(string format)
		{
			if (format.Contains('@'))
			{
				if (!format.StartsWith('@'))
				{
					throw Throwing.FormatErrorWithMessage(
						"Multi-line identifier '@' must be at the first place.",
						nameof(format));
				}
				else if ((format.Contains('0') || format.Contains('.')) && format.Contains(':'))
				{
					throw Throwing.FormatErrorWithMessage(
						"In multi-line environment, '0' and '.' cannot appear with ':' together.",
						nameof(format));
				}
				else if (format.IsMatch(@"\@[^0\!\*\.\:]+"))
				{
					throw Throwing.FormatErrorWithMessage(
						"Multi-line identifier '@' must follow only character '!', '*', '0', '.' or ':'.",
						nameof(format));
				}
				else if (format.Contains('#'))
				{
					if (!format.StartsWith('#'))
					{
						throw Throwing.FormatErrorWithMessage(
							"Intelligence option character '#' must be at the first place.",
							nameof(format));
					}
					else if (format.IsMatch(@"\#[^\.0]+"))
					{
						throw Throwing.FormatErrorWithMessage(
							"Intelligence option character '#' must be with placeholder '0' or '.'.",
							nameof(format));
					}
					else if (format.Contains('0') && format.Contains('.'))
					{
						throw Throwing.FormatErrorWithMessage(
							"Placeholder character '0' and '.' cannot appear both.",
							nameof(format));
					}
					else if (format.Contains('+') && format.Contains('!'))
					{
						throw Throwing.FormatErrorWithMessage(
							"Cell status character '+' and '!' cannot appear both.",
							nameof(format));
					}
					else if (format.Contains(':') && !format.EndsWith(':'))
					{
						throw Throwing.FormatErrorWithMessage(
							"Candidate leading character ':' must be at the last place.",
							nameof(format));
					}
				}
			}
		}

		/// <summary>
		/// Try to parse a string and converts to this type, and returns a
		/// <see cref="bool"/> value indicating the result of the conversion.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="gridParsingType">The grid parsing type.</param>
		/// <param name="result">
		/// (<see langword="out"/> parameter) The result parsed. If the conversion is failed,
		/// this argument will be <see langword="null"/>.
		/// </param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		public static bool TryParse(
			string str, GridParsingType gridParsingType, [NotNullWhen(true)] out Grid? result)
		{
			try
			{
				result = Parse(str, gridParsingType);
				return true;
			}
			catch
			{
				result = null;
				return false;
			}
		}

		/// <summary>
		/// Creates an instance using grid values.
		/// </summary>
		/// <param name="gridValues">The array of grid values.</param>
		/// <returns>The result instance.</returns>
		public static Grid CreateInstance(int[] gridValues)
		{
			var result = Empty.Clone();
			for (int i = 0; i < 81; i++)
			{
				int value = gridValues[i];
				if (value != 0)
				{
					// Calls the indexer to trigger the event
					// (Clear the candidates in peer cells).
					result[i] = value - 1;

					// Set the status to 'CellStatus.Given'.
					result.SetCellStatus(i, CellStatus.Given);
				}
			}

			return result;
		}

		/// <summary>
		/// Creates an instance using grid values.
		/// </summary>
		/// <param name="gridValues">The array of grid values.</param>
		/// <returns>The result instance.</returns>
		public static Grid CreateInstance(int[,] gridValues)
		{
			var result = Empty.Clone();
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					int value = gridValues[i, j];
					if (value != 0)
					{
						int pos = i * 9 + j;
						result[pos] = value - 1;
						result.SetCellStatus(pos, CellStatus.Given);
					}
				}
			}

			return result;
		}


		/// <summary>
		/// Indicates whether two instances have a same value.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator ==(Grid left, Grid right) => left.Equals(right);

		/// <summary>
		/// Indicates whether two instances have two different values.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator !=(Grid left, Grid right) => !(left == right);
	}
}