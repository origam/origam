using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Testing {
	public abstract class HashBox : BlackBox {
		public HashBox(Stream fs) : base(fs) {}
		protected abstract HashAlgorithm GetHashInstance();
		protected abstract byte[][][] GetTestVectors();
		protected abstract bool IsKeyed();
		public override int Start() {
			int tests = 0;
			HashAlgorithm hash = GetHashInstance();
			byte[][][] vectors = GetTestVectors();
			for(int i = 0; i < vectors.Length; i++) {
				if (IsKeyed())
					((KeyedHashAlgorithm)hash).Key = vectors[i][2];
				tests += StartTests(hash, vectors[i][0], vectors[i][1]);
			}
			return tests;
		}
		protected int StartTests(HashAlgorithm hash, byte[] input, byte[] result) {
			try {
				byte[] ch = hash.ComputeHash(input, 0, input.Length);
				if (!ArrayEquals(ch, result))
					AddError("HB-ST1");
			} catch {
				AddError("HB-ST2");
			}
			try {
				// feed input byte-by-byte
				for(int i = 0; i < input.Length - 1; i++) {
					hash.TransformBlock(input, i, 1, input, i);
				}
				if (input.Length > 0)
					hash.TransformFinalBlock(input, input.Length - 1, 1);
				else
					hash.TransformFinalBlock(input, 0, 0);
				if (!ArrayEquals(hash.Hash, result)) {
					AddError("HB-ST3");
					Console.WriteLine(Encoding.ASCII.GetString(input));
				}
			} catch {
				AddError("HB-ST4");
			} finally {
				hash.Initialize();
			}
			return 4;
		}
	}
}
