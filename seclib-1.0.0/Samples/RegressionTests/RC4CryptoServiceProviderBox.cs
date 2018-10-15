using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Testing {
	public class RC4CryptoServiceProviderBox : RC4Box {
		public RC4CryptoServiceProviderBox(Stream fs) : base(fs) {}
		public override string Name {
			get {
				return "RC4CryptoServiceProvider";
			}
		}
		protected override RC4 GetRC4Instance() {
			return new RC4CryptoServiceProvider();
		}
	}
}
