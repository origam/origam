using System;
using System.IO;
using System.Threading;
using Org.Mentalis.Security.Certificates;

namespace Org.Mentalis.Security.Testing {
	public class CertificateChainBox : BlackBox {
		public CertificateChainBox(Stream fs) : base(fs) {}
		public override string Name {
			get {
				return "CertificateChain";
			}
		}
		public override int Start() {
			Certificate c = null;
			try {
				c = CertificateStore.CreateFromPfxFile(@"certs\server.pfx", "test").FindCertificateByUsage(new string[] {"1.3.6.1.5.5.7.3.1"});
			} catch {
				AddError("CC-S-0");
				return 1;
			}
			int tests = 0;
			// test constructor
			try {
				CertificateChain cc = new CertificateChain(null);
				AddError("CC-S-1");
			} catch (ArgumentNullException) {
			} catch {
				AddError("CC-S-2");
			}
			tests += 2;
			// do other tests
			tests = TestBuildChain();
			tests += TestVerifyCert();
			return tests;
		}
		private int TestBuildChain() {
			Certificate c = null;
			CertificateChain cc = null;
			try {
				c = CertificateStore.CreateFromPfxFile(@"certs\server.pfx", "test").FindCertificateByUsage(new string[] {"1.3.6.1.5.5.7.3.1"});
			} catch {
				AddWarning("CC-W-TBC1");
				return 0;
			}
			try {
				cc = c.GetCertificateChain();
				Certificate[] cs = null;
				try {
					cs = cc.GetCertificates();
				} catch {
					AddError("CC-TBC2");
				}
				if (cs.Length != 2)
					AddError("CC-TBC3");
				if (!cs[0].Equals(c))
					AddError("CC-TBC4");
			} catch {
				AddError("CC-TBC1");
			}
			return 4;
		}
		private int TestVerifyCert() {
			Certificate c = null;
			CertificateChain cc = null;
			try {
				c = CertificateStore.CreateFromPfxFile(@"certs\server.pfx", "test").FindCertificateByUsage(new string[] {"1.3.6.1.5.5.7.3.1"});
				cc = c.GetCertificateChain();
			} catch {
				AddWarning("CC-W-TBC2");
				return 0;
			}
			try {
				if (cc.VerifyChain("Mentalis.org Team", AuthType.Server, VerificationFlags.AllowUnknownCA) != CertificateStatus.ValidCertificate)
					AddError("CC-TVC1");
			} catch {
				AddError("CC-TVC2");
			}
			try {
				if (cc.VerifyChain("Mentalis.org Team", AuthType.Server) != CertificateStatus.UntrustedRoot)
					AddError("CC-TVC3");
			} catch {
				AddError("CC-TVC4");
			}
			try {
				if (cc.VerifyChain("Other Name", AuthType.Server, VerificationFlags.AllowUnknownCA) != CertificateStatus.NoCNMatch)
					AddError("CC-TVC5");
			} catch {
				AddError("CC-TVC6");
			}
			try {
				c = CertificateStore.CreateFromCerFile(@"certs\expired.cer").FindCertificateByUsage(new string[] {"1.3.6.1.5.5.7.3.1"});
				cc = c.GetCertificateChain();
			} catch {
				AddWarning("CC-W-TBC3");
				return 0;
			}
			try {
				IAsyncResult ret = cc.BeginVerifyChain("Mentalis.org Team", AuthType.Server, VerificationFlags.AllowUnknownCA, null, null);
				ret.AsyncWaitHandle.WaitOne();
				if (cc.EndVerifyChain(ret) != CertificateStatus.Expired)
					AddError("CC-TVC7");
			} catch {
				AddError("CC-TVC8");
			}
			return 8;
		}
	}
}
