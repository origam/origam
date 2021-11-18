using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Testing {
	public class DataProtectionCryptoServiceProviderBox : BlackBox {
		public DataProtectionCryptoServiceProviderBox(Stream fs) : base(fs) {}
		public override string Name {
			get {
				return "DataProtectionCryptoServiceProvider";
			}
		}
		public override int Start() {
			int tests = 0;
			DataProtectionCryptoServiceProvider dp = new DataProtectionCryptoServiceProvider();
			tests += TestDP(dp, Encoding.ASCII.GetBytes(""));
			tests += TestDP(dp, Encoding.ASCII.GetBytes("a"));
			tests += TestDP(dp, Encoding.ASCII.GetBytes("Hello World!"));
			tests += TestDP(dp, Encoding.ASCII.GetBytes("DataProtectionCryptoServiceProvider"));
			tests += TestDP(dp, new byte[200]);
			return tests;
		}
		protected int TestDP(DataProtectionCryptoServiceProvider dp, byte[] data) {
			byte[] entropy = new byte[m_Random.Next(5, 123)];
			m_Random.NextBytes(entropy);
			byte[] enc, dec;
			// test without entropy
			try {
				enc = dp.ProtectData(ProtectionType.CurrentUser, data);
				dec = dp.UnprotectData(enc);
				if (!ArrayEquals(dec, data))
					AddError("DP-T01");
			} catch {
				AddError("DP-T02");
			}
			try {
				enc = dp.ProtectData(ProtectionType.LocalMachine, data);
				dec = dp.UnprotectData(enc);
				if (!ArrayEquals(dec, data))
					AddError("DP-T03");
			} catch {
				AddError("DP-T04");
			}
			// test with entropy
			dp.Entropy = entropy;
			try {
				enc = dp.ProtectData(ProtectionType.CurrentUser, data);
				dec = dp.UnprotectData(enc);
				if (!ArrayEquals(dec, data))
					AddError("DP-T05");
			} catch {
				AddError("DP-T06");
			}
			try {
				enc = dp.ProtectData(ProtectionType.LocalMachine, data);
				dec = dp.UnprotectData(enc);
				if (!ArrayEquals(dec, data))
					AddError("DP-T07");
			} catch {
				AddError("DP-T08");
			}
			// test MAC
			try {
				enc = dp.ProtectData(ProtectionType.LocalMachine, data);
				enc[enc.Length / 2] ^= 0xFF; // corrupt a byte
				dec = dp.UnprotectData(enc);
				AddError("DP-T09");
			} catch (CryptographicException) {
			} catch {
				AddError("DP-T10");
			}
			dp.Entropy = null;
			return 10;
		}
		Random m_Random = new Random();
	}
}
