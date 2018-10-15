using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Testing {
	public class ARCFourManagedBox : RC4Box {
		public ARCFourManagedBox(Stream fs) : base(fs) {}
		public override string Name {
			get {
				return "ARCFourManaged";
			}
		}
		protected override RC4 GetRC4Instance() {
			return new ARCFourManaged();
		}
	}
}
