﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Sudoku.Data.Extensions;
using Sudoku.Diagnostics.CodeAnalysis;

namespace Sudoku.Data.Meta
{
	/// <summary>
	/// Encapsulates a binary series of cell status table consisting of 81 bits,
	/// where <see langword="true"/> bit (1) is for the cell having digit,
	/// and the <see langword="false"/> bit (0) is for empty cell. Sometimes for other usages.
	/// </summary>
	[DisableDefaultConstructor]
	public partial struct GridMap : IEnumerable<bool>, IEquatable<GridMap>
	{
		/// <summary>
		/// Indicates an empty instance (making no changes).
		/// </summary>
		public static readonly GridMap Empty = new GridMap();


		/// <summary>
		/// The value used for shifting.
		/// </summary>
		private const int Shifting = 41;

		/// <summary>
		/// Inner binary representation values.
		/// </summary>
		private long _high, _low;


		/// <summary>
		/// Initializes an instance with the specified cell offset
		/// (Sets itself and all peers).
		/// </summary>
		/// <param name="offset">The cell offset.</param>
		public GridMap(int offset) : this(offset, true)
		{
		}

		/// <summary>
		/// Initializes an instance with the specified cell offset.
		/// This will set all bits of all peers of this cell. Another
		/// <see cref="bool"/> value indicates whether this initialization
		/// will set the bit of itself.
		/// </summary>
		/// <param name="offset">The cell offset.</param>
		/// <param name="setItself">
		/// A <see cref="bool"/> value indicating whether this initialization
		/// will set the bit of itself.
		/// </param>
		public GridMap(int offset, bool setItself) : this(PeerTable[offset]) =>
			this[offset] = setItself;

		/// <summary>
		/// Initializes an instance with a series of cell offsets.
		/// </summary>
		/// <param name="offsets">cell offsets.</param>
		/// <remarks>
		/// Note that all offsets will be set <see langword="true"/>, but their own peers
		/// will not be set <see langword="true"/>.
		/// </remarks>
		public unsafe GridMap(IEnumerable<int> offsets)
		{
			(_low, _high) = (0, 0);
			fixed (long* a = &_low, b = &_high)
			{
				long** series = stackalloc[] { a, b };
				foreach (int offset in offsets)
				{
					*series[offset / Shifting] |= 1L << offset % Shifting;
				}
			}
		}

		/// <summary>
		/// Initializes an instance with three binary value.
		/// </summary>
		/// <param name="high">Higher 40 bits.</param>
		/// <param name="low">Lower 41 bits.</param>
		private GridMap(long high, long low) => (_high, _low) = (high, low);


		/// <summary>
		/// Indicates the total number of cells where the corresponding
		/// value are set <see langword="true"/>.
		/// </summary>
		public readonly int Count => _low.CountSet() + _high.CountSet();

		/// <summary>
		/// Indicates all cell offsets whose corresponding value
		/// are set <see langword="true"/>.
		/// </summary>
		public readonly IEnumerable<int> Offsets
		{
			get
			{
				long value;
				int i;
				for (value = _low, i = 0; i < Shifting; i++, value >>= 1)
				{
					if ((value & 1) != 0)
					{
						yield return i;
					}
				}
				for (value = _high; i < 81; i++, value >>= 1)
				{
					if ((value & 1) != 0)
					{
						yield return i;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets a <see cref="bool"/> value on the specified cell
		/// offset.
		/// </summary>
		/// <param name="offset">The cell offset.</param>
		/// <value>
		/// A <see cref="bool"/> value on assignment.
		/// </value>
		/// <returns>
		/// A <see cref="bool"/> value indicating whether the cell has digit.
		/// </returns>
		public bool this[int offset]
		{
			readonly get =>
				((stackalloc[] { _low, _high }[offset / Shifting] >> offset % Shifting) & 1) != 0;
			set
			{
				// We should get along with pointers extremely carefully.
				if (offset < 0 || offset >= 81)
				{
					throw new ArgumentOutOfRangeException(nameof(offset));
				}

				unsafe
				{
					fixed (long* a = &_low, b = &_high)
					{
						long** series = stackalloc[] { a, b };
						long* valueToModify = series[offset / Shifting];
						if (value)
						{
							*valueToModify |= 1L << offset % Shifting;
						}
						else
						{
							*valueToModify &= ~(1 << offset % Shifting);
						}
					}
				}
			}
		}


		/// <summary>
		/// Deconstruct this instance.
		/// </summary>
		/// <param name="high">(out parameter) Higher 40 bits.</param>
		/// <param name="low">(out parameter) Lower 41 bits.</param>
		[OnDeconstruction]
		public readonly void Deconstruct(out long high, out long low) =>
			(high, low) = (_high, _low);

		/// <inheritdoc/>
		public override readonly bool Equals(object? obj) =>
			obj is GridMap comparer && Equals(comparer);

		/// <summary>
		/// Indicates whether the current object has the same value with the other one.
		/// </summary>
		/// <param name="other">The other value to compare.</param>
		/// <returns>
		/// The result of this comparsion. <see langword="true"/> if two instances hold a same
		/// value; otherwise, <see langword="false"/>.
		/// </returns>
		public readonly bool Equals(GridMap other) =>
			_high == other._high && _low == other._low;

		/// <summary>
		/// Get all cell offsets whose bits are set <see langword="true"/>.
		/// </summary>
		/// <returns>An array of cell offsets.</returns>
		public readonly int[] ToArray() => Offsets.ToArray();

		/// <inheritdoc/>
		public readonly IEnumerator<bool> GetEnumerator()
		{
			long h = _high, l = _low;
			while (l != 0)
			{
				yield return (l & 1) != 0;
				l >>= 1;
			}

			while (h != 0)
			{
				yield return (h & 1) != 0;
				h >>= 1;
			}
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode() =>
			GetType().GetHashCode() ^ (int)((_low ^ _high) & int.MaxValue);

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override readonly string ToString()
		{
			var sb = new StringBuilder();
			int i;
			long value = _low;
			for (i = 0; i < 27; i++, value >>= 1)
			{
				sb.Append(value & 1);
			}
			sb.Append(" ");
			for (; i < 41; i++, value >>= 1)
			{
				sb.Append(value & 1);
			}
			for (value = _high; i < 54; i++, value >>= 1)
			{
				sb.Append(value & 1);
			}
			sb.Append(" ");
			for (; i < 81; i++, value >>= 1)
			{
				sb.Append(value & 1);
			}

			return sb.Reverse().ToString();
		}

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Set the specified cell as <see langword="true"/> value.
		/// </summary>
		/// <param name="offset">The cell offset.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int offset) => this[offset] = true;

		/// <summary>
		/// Set the specified cell as <see langword="false"/> value.
		/// </summary>
		/// <param name="offset">The cell offset.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(int offset) => this[offset] = false;

		/// <summary>
		/// Set all peers as <see langword="true"/> value.
		/// </summary>
		/// <param name="offset">The cell offset.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPeersTrue(int offset) => UnionWith(new GridMap(offset));

		/// <summary>
		/// Set all peers as <see langword="false"/> value.
		/// </summary>
		/// <param name="offset">The cell offset.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPeersFalse(int offset) => IntersectWith(new GridMap(offset));

		/// <summary>
		/// Negate all peers' value.
		/// </summary>
		/// <param name="offset">The cell offset.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void NegatePeers(int offset) => SymmetricalExceptWith(new GridMap(offset));

		/// <summary>
		/// Intersect with the other instance.
		/// </summary>
		/// <param name="other">The other instance.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IntersectWith(GridMap other) =>
			(_low, _high) = (_low & other._low, _high & other._high);

		/// <summary>
		/// Union with the other instance.
		/// </summary>
		/// <param name="other">The other instance.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UnionWith(GridMap other) =>
			(_low, _high) = (_low | other._low, _high | other._high);

		/// <summary>
		/// Symmetrical except with the other instance.
		/// </summary>
		/// <param name="other">The other instance.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SymmetricalExceptWith(GridMap other) =>
			(_low, _high) = (_low ^ other._low, _high ^ other._high);

		/// <summary>
		/// Negate all values.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Negate() => (_low, _high) = (~_low, ~_high);


		/// <summary>
		/// Get all cell offsets in the specified region.
		/// </summary>
		/// <param name="regionOffset">The region offset.</param>
		/// <returns>All cell offsets.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int[] GetCellsIn(int regionOffset) => RegionTable[regionOffset];

		/// <summary>
		/// Create a <see cref="GridMap"/> instance with the specified region offset.
		/// This will set all bits <see langword="true"/> in this region.
		/// </summary>
		/// <param name="regionOffset">The region offset.</param>
		/// <returns>The grid map.</returns>
		public static GridMap CreateInstance(int regionOffset)
		{
			var result = Empty;
			foreach (int cell in RegionTable[regionOffset])
			{
				result[cell] = true;
			}

			return result;
		}


		/// <summary>
		/// Indicates whether two instances have a same value.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator ==(GridMap left, GridMap right) => left.Equals(right);

		/// <summary>
		/// Indicates whether two instances have two different values.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator !=(GridMap left, GridMap right) => !(left == right);

		/// <summary>
		/// Negate all bits.
		/// </summary>
		/// <param name="gridMap">The instance to negate.</param>
		/// <returns>The negative result.</returns>
		public static GridMap operator ~(GridMap gridMap) =>
			new GridMap(~gridMap._high, ~gridMap._low);

		/// <summary>
		/// Get a <see cref="GridMap"/> that contains all <paramref name="left"/> cells
		/// but not in <paramref name="right"/> cells.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The result.</returns>
		public static GridMap operator -(GridMap left, GridMap right) => ~left & right;

		/// <summary>
		/// Intersect two <see cref="GridMap"/>s.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The intersection result.</returns>
		public static GridMap operator &(GridMap left, GridMap right) =>
			new GridMap(left._high & right._high, left._low & right._low);

		/// <summary>
		/// Union two <see cref="GridMap"/>s.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The union result.</returns>
		public static GridMap operator |(GridMap left, GridMap right) =>
			new GridMap(left._high | right._high, left._low | right._low);

		/// <summary>
		/// Symmetrical except two <see cref="GridMap"/>s.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The result.</returns>
		public static GridMap operator ^(GridMap left, GridMap right) =>
			new GridMap(left._high ^ right._high, left._low ^ right._low);
	}
}
