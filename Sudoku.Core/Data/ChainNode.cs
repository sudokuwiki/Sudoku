﻿using System;
using System.Runtime.CompilerServices;

namespace Sudoku.Data
{
	/// <summary>
	/// Provides a node in a chain. This data structure is used for
	/// searching for AICs.
	/// </summary>
	[Obsolete]
	public readonly struct ChainNode : IEquatable<ChainNode>
	{
		/// <summary>
		/// Initializes an instance with a specified candidate and a <see cref="bool"/>
		/// value.
		/// </summary>
		/// <param name="candidate">The candidate.</param>
		/// <param name="isOn">Indicates whether the candidate is on.</param>
		public ChainNode(int candidate, bool isOn) => (Candidate, IsOn) = (candidate, isOn);


		/// <summary>
		/// Indicates the candidate.
		/// </summary>
		public int Candidate { get; }

		/// <summary>
		/// Indicates whether the candidate is on. If the value is <see langword="true"/>,
		/// the candidate will be on (i.e. a true candidate); otherwise, <see langword="false"/>.
		/// </summary>
		public bool IsOn { get; }


		/// <include file='../../GlobalDocComments.xml' path='comments/method[@name="Deconstruct"]'/>
		/// <param name="candidate">(<see langword="out"/> parameter) The candidate.</param>
		/// <param name="isOn">
		/// (<see langword="out"/> parameter) Indicates whether the candidate is on.
		/// </param>
		public void Deconstruct(out int candidate, out bool isOn) =>
			(candidate, isOn) = (Candidate, IsOn);

		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is ChainNode comparer && Equals(comparer);

		/// <inheritdoc/>
		public bool Equals(ChainNode other) => GetHashCode() == other.GetHashCode();

		/// <inheritdoc/>
		public override int GetHashCode() => (Candidate + 1) * (IsOn ? 1 : -1);

		/// <include file='../../GlobalDocComments.xml' path='comments/method[@name="ToString" and @paramType="__noparam"]'/>
		public override string ToString()
		{
			string sign = IsOn ? string.Empty : "!";
			string candStr = CandidateToString(Candidate);
			return $"{sign}{candStr}";
		}

		/// <summary>
		/// Candidate to string.
		/// </summary>
		/// <param name="candidateOffset">The candidate.</param>
		/// <returns>The string text.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string CandidateToString(int candidateOffset)
		{
			int cell = candidateOffset / 9;
			return $"r{cell / 9 + 1}c{cell % 9 + 1}({candidateOffset % 9 + 1})";
		}


		/// <include file='../../GlobalDocComments.xml' path='comments/operator[@name="op_Equality"]'/>
		public static bool operator ==(ChainNode left, ChainNode right) => left.Equals(right);

		/// <include file='../../GlobalDocComments.xml' path='comments/operator[@name="op_Inequality"]'/>
		public static bool operator !=(ChainNode left, ChainNode right) => !(left == right);
	}
}