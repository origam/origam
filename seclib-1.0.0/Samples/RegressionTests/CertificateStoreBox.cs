using System;
using System.IO;
using Org.Mentalis.Security.Certificates;

namespace Org.Mentalis.Security.Testing {
	public class CertificateStoreBox : BlackBox {
		public CertificateStoreBox(Stream fs) : base(fs) {}
		public override string Name {
			get {
				return "CertificateStore";
			}
		}
		public override int Start() {
			// start tests
			int tests = TestConstructs();
			tests += TestMethods();
			return tests;
		}
		private int TestConstructs() {
			CertificateStore cs = null;
			// empty constructor
			try {
				cs = new CertificateStore();
				if (cs.EnumCertificates().Length != 0)
					AddError("CS-TC-1");
			} catch {
				AddError("CS-TC-2");
			}
			// string constructor
			try {
				cs = new CertificateStore((string)null);
				AddError("CS-TC-3");
			} catch (ArgumentNullException) {
			} catch {
				AddError("CS-TC-4");
			}
			try {
				cs = new CertificateStore("ROOT");
				if (cs.EnumCertificates().Length == 0)
					AddError("CS-TC-5"); // it is _very_ unlikely that the ROOT store is empty
			} catch {
				AddError("CS-TC-6");
			}
			// CertificateStore constructor
			try {
				cs = new CertificateStore(cs);
				if (cs.EnumCertificates().Length == 0)
					AddError("CS-TC-7"); //copy of the ROOT store
			} catch {
				AddError("CS-TC-8");
			}
			try {
				cs = new CertificateStore((CertificateStore)null);
				AddError("CS-TC-9");
			} catch (ArgumentNullException) {
			} catch {
				AddError("CS-TC-10");
			}
			// IntPtr constructor
			try {
				cs = new CertificateStore(IntPtr.Zero);
				AddError("CS-TC-11");
			} catch (ArgumentException) {
			} catch {
				AddError("CS-TC-12");
			}
			try {
				cs = new CertificateStore(IntPtr.Zero, true);
				AddError("CS-TC-13");
			} catch (ArgumentException) {
			} catch {
				AddError("CS-TC-14");
			}
			try {
				cs = new CertificateStore(new CertificateStore().Handle, true);
			} catch {
				AddError("CS-TC-15");
			}
			return 15;
		}
		private int TestMethods() {
			Certificate c = null;
			CertificateStore cs = null;
			try {
				c = CertificateStore.CreateFromCerFile(@"certs\server.base64.cer").FindCertificateByUsage(new string[] {"1.3.6.1.5.5.7.3.1"});
				cs = new CertificateStore();
			} catch {
				AddError("CS-TM-0");
				return 1;
			}
			// AddCertificate + EnumCertificates
			try {
				cs.AddCertificate(null);
				AddError("CS-TM-1");
			} catch (ArgumentNullException) {
			} catch {
				AddError("CS-TM-2");
			}
			try {
				cs.AddCertificate(c);
				if (cs.EnumCertificates().Length != 1)
					AddError("CS-TM-3");
				else
					if (!cs.EnumCertificates()[0].Equals(c))
						AddError("CS-TM-4");
			} catch {
				AddError("CS-TM-5");
			}
			// DeleteCertificate + EnumCertificates
			try {
				cs.DeleteCertificate(null);
				AddError("CS-TM-6");
			} catch (ArgumentNullException) {
			} catch {
				AddError("CS-TM-7");
			}
			try {
				cs.DeleteCertificate(c);
				if (cs.EnumCertificates().Length != 0)
					AddError("CS-TM-8");
			} catch {
				AddError("CS-TM-9");
			}
			// CreateFromPfxFile
			try {
				cs.AddCertificate(c);
				CertificateStore cs2 = CertificateStore.CreateFromPfxFile(cs.ToPfxBuffer("test", true), "test");
				if (cs2.EnumCertificates().Length != 1)
					AddError("CS-TM-10");
				if (!c.Equals(cs2.EnumCertificates()[0]))
					AddError("CS-TM-11");
			} catch {
				AddError("CS-TM-12");
			}
			// FindCertificate and the CreateFrom*File methods are
			// already tested in other blackbox classes
			return 12;
		}
	}
}
