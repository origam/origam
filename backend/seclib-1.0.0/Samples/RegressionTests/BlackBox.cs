using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Org.Mentalis.Security.Testing {
	public abstract class BlackBox {
		static void Main(string[] args) {
			StartTests();
		}
		public BlackBox(Stream fs) {
			if (fs == null)
				m_ErrorStream = Stream.Null;
			else
				m_ErrorStream = fs;
		}
		/// <summary>
		/// Starts the BlackBox tests
		/// </summary>
		/// <returns>The number of tests performed.</returns>
		public abstract int Start();
		/// <summary>
		/// The name of the class(es) that will be tested
		/// </summary>
		public abstract string Name { get; }
		public int Errors {
			get {
				return m_Errors;
			}
		}
		public string ErrorLog {
			get {
				if (m_ErrorLog.Length > 2)
					return m_ErrorLog.Substring(0, m_ErrorLog.Length - 2);
				else
					return m_ErrorLog;
			}
		}
		public int Warnings {
			get {
				return m_Warnings;
			}
		}
		public string WarningLog {
			get {
				if (m_WarningLog.Length > 2)
					return m_WarningLog.Substring(0, m_WarningLog.Length - 2);
				else
					return m_WarningLog;
			}
		}
		protected Stream ErrorStream {
			get {
				return m_ErrorStream;
			}
		}
		protected void AddError(string error) {
			m_ErrorLog += error + "; ";
			ErrorStream.Write(Encoding.ASCII.GetBytes("ERROR: " + error + "\r\n"), 0, error.Length + 9);
			m_Errors++;
		}
		protected void AddWarning(string warning) {
			m_WarningLog += warning + "; ";
			ErrorStream.Write(Encoding.ASCII.GetBytes("WARNING: " + warning + "\r\n"), 0, warning.Length + 11);
			m_Warnings++;
		}
		protected void WriteText(string text) {
			ErrorStream.Write(Encoding.ASCII.GetBytes(text), 0, text.Length);
		}
		private int m_Errors = 0;
		private string m_ErrorLog = "";
		private Stream m_ErrorStream;
		private int m_Warnings = 0;
		private string m_WarningLog = "";
		/// <summary>
		/// Checks whether two byte arrays have the same elements.
		/// </summary>
		/// <param name="input1">The first array.</param>
		/// <param name="input2">The second array.</param>
		/// <returns>true if both arrays have the same elements, false otherwise.</returns>
		protected bool ArrayEquals(byte[] input1, byte[] input2) {
			if ((input1 == null && input2 != null) || (input1 != null && input2 == null))
				return false;
			if (input1 == null && input2 == null)
				return true;
			if (input1.Length != input2.Length)
				return false;
			return ArrayEquals(input1, input2, 0, input1.Length);
		}
		/// <summary>
		/// Checks whether two byte arrays have the same elements.
		/// </summary>
		/// <param name="input1">The first array.</param>
		/// <param name="input2">The second array.</param>
		/// <returns>true if both arrays have the same elements, false otherwise.</returns>
		protected bool ArrayEquals(byte[] input1, byte[] input2, int start, int count) {
			if ((input1 == null && input2 != null) || (input1 != null && input2 == null))
				return false;
			if (input1 == null && input2 == null)
				return true;
			if (start < 0)
				throw new IndexOutOfRangeException();
			if (input1.Length < start + count || input2.Length < start + count)
				return false;
			for(int i = 0; i < count; i++) {
				if (input1[i + start] != input2[i + start])
					return false;
			}
			return true;
		}
		/// <summary>
		/// Checks whether two object arrays have the same elements.
		/// </summary>
		/// <param name="input1">The first array.</param>
		/// <param name="input2">The second array.</param>
		/// <returns>true if both arrays have the same elements, false otherwise.</returns>
		protected bool ObjectArrayEquals(object[] input1, object[] input2) {
			if ((input1 == null && input2 != null) || (input1 != null && input2 == null))
				return false;
			if (input1 == null && input2 == null)
				return true;
			if (input1.Length != input2.Length)
				return false;
			for(int i = 0; i < input1.Length; i++) {
				if (!input1[i].Equals(input2[i]))
					return false;
			}
			return true;
		}
		/// <summary>
		/// Generates a buffer filled with random data.
		/// </summary>
		/// <param name="count">The number of bytes in the buffer.</param>
		/// <returns>The random buffer.</returns>
		protected byte[] GenerateRandomBuffer(int count) {
			RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
			byte[] ret = new byte[count];
			rnd.GetBytes(ret);
			return ret;
		}
		/*
		 *   Starts all the BlackBox tests
		 */
		public static void StartTests() {
			if (File.Exists("output.dbg"))
				Console.WriteLine("WARNING: output.dbg already exists; output of this session will be appended.\r\n");
			FileStream fs = File.Open("output.dbg", FileMode.Append, FileAccess.Write, FileShare.Read);
			BlackBox[] tests = new BlackBox[] { new CertificateExceptionBox(fs), new CertificateStoreBox(fs), new CertificateBox(fs), new CertificateChainBox(fs), 
												new RijndaelUnmanagedTransformBox(fs), new ARCFourManagedBox(fs), new RC4CryptoServiceProviderBox(fs), 
												new RIPEMD160ManagedBox(fs), new MD2CryptoServiceProviderBox(fs), new HMACBox(fs),
												new DataProtectionCryptoServiceProviderBox(fs) };
			StopWatch sw = new StopWatch();
			int errors = 0;
			int warnings = 0;
			int testcnt = 0;
			string name;
			long time;
			fs.Write(Encoding.ASCII.GetBytes("CertificateServices BlackBox tests\r\n"), 0, 36);
			fs.Write(Encoding.ASCII.GetBytes(Environment.OSVersion.ToString() + "\r\n"), 0, Environment.OSVersion.ToString().Length + 2);
			fs.Write(Encoding.ASCII.GetBytes(".NET CLR " + Environment.Version.ToString() + "\r\n"), 0, Environment.Version.ToString().Length + 11);
			ConsoleAttributes.ForeColor = ConsoleColor.Gray;
			Console.WriteLine("CertificateServices BlackBox tests");
			#if (DEBUG)
				Console.WriteLine("   running in DEBUG mode");
			#else
				Console.WriteLine("   running in RELEASE mode");
			#endif
			Console.WriteLine("   on " + Environment.OSVersion.ToString());
			Console.WriteLine("   and .NET CLR " + Environment.Version.ToString() + "\r\n");
			for(int i = 0; i < tests.Length; i++) {
				HasSubItems = false;
				name = tests[i].Name;
				if (name.Length > 27)
					name = name.Substring(0, 27) + "...";
				else
					name = name + "..." + new String(' ', 27 - name.Length);
				Console.Write(name + "  ");
				sw.Reset();
				testcnt += tests[i].Start();
				time = sw.Peek();
				if (!HasSubItems) {
					if (tests[i].Errors + tests[i].Warnings > 0) {
						WriteResult(false, time);
/*						if (tests[i].Errors > 0)
							Console.WriteLine("  " + tests[i].ErrorLog);
						if (tests[i].Warnings > 0)
							Console.WriteLine("  " + tests[i].WarningLog); */
					} else {
						WriteResult(true, time);
					}
				}
				errors += tests[i].Errors;
				warnings += tests[i].Warnings;
			}
			string result = String.Format("\r\n\r\nAll tests complete\r\n  {0} error(s), {1} warning(s)\r\n  {2} tests performed\r\n\r\n", errors, warnings, testcnt);
			fs.Write(Encoding.ASCII.GetBytes(result), 0, result.Length);
			Console.WriteLine(result + "\r\n");
			fs.Close();
			Console.WriteLine("Output saved to " + Environment.CurrentDirectory.TrimEnd('\\') + @"\output.dbg");
			Console.WriteLine("\r\nPress ENTER to continue...");
			Console.ReadLine();
		}
		protected static void WriteSubItem(string name) {
			if (!HasSubItems)
				Console.WriteLine();
			HasSubItems = true;
			if (name.Length > 24)
				name = "   " + name.Substring(0, 24) + "...";
			else
				name = "   " + name + "..." + new String(' ', 24 - name.Length);
			Console.Write(name + "  ");
		}
		protected static void WriteResult(bool success, long time) {
			string t = "         " + (time / 10000.0).ToString() + "s";
			if (success) {
				Console.Write("[  ");
				ConsoleAttributes.ForeColor = ConsoleColor.LightGreen;
				Console.Write("OK");
				ConsoleAttributes.ForeColor = ConsoleColor.Gray;
				Console.Write("  ]" + t + "\r\n");
			} else {
				Console.Write("[");
				ConsoleAttributes.ForeColor = ConsoleColor.LightRed;
				Console.Write("FAILED");
				ConsoleAttributes.ForeColor = ConsoleColor.Gray;
				Console.Write("]" + t + "\r\n");
			}
		}
		public string TempPath {
			get {
				if (m_TempPath == null) {
					m_TempPath = Environment.GetEnvironmentVariable("TEMP");
					if (m_TempPath == null)
						m_TempPath = Environment.GetEnvironmentVariable("TMP");
					if (m_TempPath == null)
						throw new DirectoryNotFoundException();
					m_TempPath = m_TempPath.Trim('"');
					if (!m_TempPath.EndsWith(@"\"))
						m_TempPath += @"\";
				}
				return m_TempPath;
			}
		}
		private string m_TempPath;
		private static bool HasSubItems;
	}
}
