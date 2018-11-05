#region

using System;
using System.Runtime.ExceptionServices;

#endregion

namespace RazorSharp
{

	/// <summary>
	///     Describes a type of low-level exception / segmentation fault
	/// </summary>
	internal enum CorruptionState
	{
		None,

		/// <summary>
		///     <see cref="AccessViolationException" />
		/// </summary>
		AccessViolation,

		/// <summary>
		///     <see cref="NullReferenceException" />
		/// </summary>
		NullReference,

		/// <summary>
		///     <see cref="InvalidProgramException" />
		/// </summary>
		InvalidProgram
	}

	internal static class LowLevel
	{


		/// <summary>
		///     Determines whether <paramref name="action" /> causes a segmentation fault.
		/// </summary>
		/// <param name="action"><see cref="Action" /> to perform</param>
		/// <param name="state">Corruption type</param>
		/// <returns>
		///     <c>true</c> if any exceptions described by <see cref="CorruptionState" /> are caught; <c>false</c> otherwise
		/// </returns>
		[HandleProcessCorruptedStateExceptions]
		internal static bool CorruptsState(Action action, out CorruptionState state)
		{
			try {
				action();
				state = CorruptionState.None;
				return false;
			}
			catch (AccessViolationException) {
				state = CorruptionState.AccessViolation;
			}
			catch (NullReferenceException) {
				state = CorruptionState.NullReference;
			}
			catch (InvalidProgramException) {
				state = CorruptionState.InvalidProgram;
			}


			return true;
		}
	}

}