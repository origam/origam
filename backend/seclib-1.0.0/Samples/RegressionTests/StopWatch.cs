/*
    StopWatch class for C#
		Version: 1.0
*/
/*
    Copyright © 2002, The KPD-Team
    All rights reserved.
    http://www.mentalis.org/

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions
  are met:

    - Redistributions of source code must retain the above copyright
       notice, this list of conditions and the following disclaimer. 

    - Neither the name of the KPD-Team, nor the names of its contributors
       may be used to endorse or promote products derived from this
       software without specific prior written permission. 

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
  THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
  STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
  OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Runtime.InteropServices;

namespace Org.Mentalis.Security.Testing {
	/// <summary>
	/// Represents a high-resolution stopwatch. It can be used to measure very small intervals of time.
	/// </summary>
	public sealed class StopWatch {
		/// <summary>
		/// The QueryPerformanceCounter function retrieves the current value of the high-resolution performance counter.
		/// </summary>
		/// <param name="x">Pointer to a variable that receives the current performance-counter value, in counts.</param>
		/// <returns>If the function succeeds, the return value is nonzero.</returns>
		[DllImport("kernel32.dll")]
		extern static int QueryPerformanceCounter(ref long x);
		/// <summary>
		/// The QueryPerformanceFrequency function retrieves the frequency of the high-resolution performance counter, if one exists. The frequency cannot change while the system is running.
		/// </summary>
		/// <param name="x">Pointer to a variable that receives the current performance-counter frequency, in counts per second. If the installed hardware does not support a high-resolution performance counter, this parameter can be zero.</param>
		/// <returns>If the installed hardware supports a high-resolution performance counter, the return value is nonzero.</returns>
		[DllImport("kernel32.dll")]
		extern static int QueryPerformanceFrequency(ref long x);
		/// <summary>
		/// Initializes a new instance of the StopWatch class.
		/// </summary>
		/// <exception cref="NotSupportedException">The system does not have a high-resolution performance counter.</exception>
		public StopWatch() {
			Frequency = GetFrequency();
			Reset();
		}
		/// <summary>
		/// Resets the stopwatch. This method should be called when you start measuring.
		/// </summary>
		/// <exception cref="NotSupportedException">The system does not have a high-resolution performance counter.</exception>
		public void Reset() {
			StartTime = GetValue();
		}
		/// <summary>
		/// Returns the time that has passed since the Reset() method was called.
		/// </summary>
		/// <remarks>The time is returned in tenths-of-a-millisecond. If the Peek method returns '10000', it means the interval took exactely one second.</remarks>
		/// <returns>A long that contains the time that has passed since the Reset() method was called.</returns>
		/// <exception cref="NotSupportedException">The system does not have a high-resolution performance counter.</exception>
		public long Peek() {
			return (long)(((GetValue() - StartTime) / (double)Frequency) * 10000);
		}
		/// <summary>
		/// Retrieves the current value of the high-resolution performance counter.
		/// </summary>
		/// <exception cref="NotSupportedException">The system does not have a high-resolution performance counter.</exception>
		/// <returns>A long that contains the current performance-counter value, in counts.</returns>
		private long GetValue() {
			long ret = 0;
			if (QueryPerformanceCounter(ref ret) == 0)
				throw new NotSupportedException("Error while querying the high-resolution performance counter.");
			return ret;
		}
		/// <summary>
		/// Retrieves the frequency of the high-resolution performance counter, if one exists. The frequency cannot change while the system is running.
		/// </summary>
		/// <exception cref="NotSupportedException">The system does not have a high-resolution performance counter.</exception>
		/// <returns>A long that contains the current performance-counter frequency, in counts per second.</returns>
		private long GetFrequency() {
			long ret = 0;
			if (QueryPerformanceFrequency(ref ret) == 0)
				throw new NotSupportedException("Error while querying the performance counter frequency.");
			return ret;
		}
		/// <summary>
		/// Gets or sets the start time.
		/// </summary>
		/// <value>A long that holds the start time.</value>
		private long StartTime {
			get {
				return m_StartTime;
			}
			set {
				m_StartTime = value;
			}
		}
		/// <summary>
		/// Gets or sets the frequency of the high-resolution performance counter.
		/// </summary>
		/// <value>A long that holds the frequency of the high-resolution performance counter.</value>
		private long Frequency {
			get {
				return m_Frequency;
			}
			set {
				m_Frequency = value;
			}
		}
		// private variables
		/// <summary>Holds the value of the StartTime property.</summary>
		private long m_StartTime;
		/// <summary>Holds the value of the Frequency property.</summary>
		private long m_Frequency;
	}
}
