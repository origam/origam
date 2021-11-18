using System;
using System.IO;
using Org.Mentalis.Security.Certificates;

namespace Org.Mentalis.Security.Testing {
	public class CertificateExceptionBox : BlackBox {
		public CertificateExceptionBox(Stream fs) : base(fs) {}
		public override string Name {
			get {
				return "CertificateException";
			}
		}
		public override int Start() {
			CertificateException e;
			try {
				e = new CertificateException();
			} catch {
				AddError("CE-1");
			}
			try {
				e = new CertificateException("ErrorMessage");
				if (e.Message != "ErrorMessage")
					AddError("CE-2");
			} catch {
				AddError("CE-3");
			}
			Exception ne = new Exception();
			try {
				e = new CertificateException("ErrorMessage", ne);
				if (e.Message != "ErrorMessage")
					AddError("CE-4");
				if (e.InnerException != ne)
					AddError("CE-5");
			} catch {
				AddError("CE-6");
			}
			return 6;
		}
	}
}
