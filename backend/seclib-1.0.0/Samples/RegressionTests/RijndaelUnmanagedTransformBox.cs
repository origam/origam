using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Testing {
	public class RijndaelUnmanagedTransformBox : BlackBox {
		public RijndaelUnmanagedTransformBox(Stream fs) : base(fs) {}
		public override string Name {
			get {
				return "RijndaelUnmanagedTransform";
			}
		}
		public override int Start() {
			int tests = 0;
			int[] keySizes = new int[] {128, 192, 256};
			int[] blockSizes = new int[] {128, 192, 256};
			PaddingMode[] paddingModes = new PaddingMode[] {PaddingMode.None, PaddingMode.PKCS7, PaddingMode.Zeros};
			CipherMode[] cipherModes = new CipherMode[] {CipherMode.CBC, CipherMode.ECB};
			for(int key = 0; key < keySizes.Length; key++) {
				for(int block = 0; block < blockSizes.Length; block++) {
					for(int padding = 0; padding < paddingModes.Length; padding++) {
						for(int cipher = 0; cipher < cipherModes.Length; cipher++) {
							tests += StartTest(keySizes[key], blockSizes[block], paddingModes[padding], cipherModes[cipher]);
						}
					}
				}
			}
			return tests;
		}
		private int StartTest(int keySize, int blockSize, PaddingMode padding, CipherMode mode) {
			Rijndael rm, ru;
			try {
				rm = new RijndaelManaged();
				ru = new RijndaelCryptoServiceProvider();
				rm.KeySize = ru.KeySize = keySize;
				rm.BlockSize = ru.BlockSize = blockSize;
				rm.Padding = ru.Padding = padding;
				rm.Mode = ru.Mode = mode;
				rm.GenerateIV();
				ru.IV = rm.IV;
				rm.GenerateKey();
				ru.Key = rm.Key;
			} catch {
				AddError("RUT-ST01");
				return 1;
			}
			ICryptoTransform rmt, rut;
			// test encryption
			try {
				rmt = rm.CreateEncryptor();
				rut = ru.CreateEncryptor();
			} catch {
				AddError("RUT-ST02");
				return 2;
			}
			if (rmt.InputBlockSize != rut.InputBlockSize)
				AddError("RUT-ST03");
			if (rmt.OutputBlockSize != rut.OutputBlockSize)
				AddError("RUT-ST04");
			byte[] toEncrypt = new byte[123];
			new RNGCryptoServiceProvider().GetBytes(toEncrypt);
			byte[] me, ue;
			try {
				me = rmt.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
			} catch {
				AddError("RUT-ST05");
				return 5;
			}
			try {
				ue = rut.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
			} catch {
				AddError("RUT-ST06");
				return 6;
			}
			if (!ArrayEquals(me, ue))
				AddError("RUT-ST07");
			byte[] menc = null;
			byte[] uenc = null;
			int uo = 0, mo = 0;
			try {
				menc = new byte[toEncrypt.Length + rmt.OutputBlockSize];
				uenc = new byte[toEncrypt.Length + rut.OutputBlockSize];
				for(int i = 0; i < toEncrypt.Length; i += rmt.InputBlockSize) {
					if (toEncrypt.Length > i + rmt.InputBlockSize) {
						mo += rmt.TransformBlock(toEncrypt, i, rmt.InputBlockSize, menc, mo);
						uo += rut.TransformBlock(toEncrypt, i, rut.InputBlockSize, uenc, uo);
					} else {
						if (padding == PaddingMode.PKCS7) {
							byte[] temp = rmt.TransformFinalBlock(toEncrypt, i, toEncrypt.Length - i);
							Array.Copy(temp, 0, menc, mo, temp.Length);
							mo += temp.Length;
							temp = rut.TransformFinalBlock(toEncrypt, i, toEncrypt.Length - i);
							Array.Copy(temp, 0, uenc, uo, temp.Length);
							uo += temp.Length;
						}
					}
				}
			} catch {
				AddError("RUT-ST08");
			}
			if (mo != uo || !ArrayEquals(menc, uenc, 0, mo))
				AddError("RUT-ST09");
			if (padding == PaddingMode.PKCS7) {
				if (mo != me.Length || uo != ue.Length)
					AddError("RUT-ST10");
				if (!ArrayEquals(menc, me, 0, mo))
					AddError("RUT-ST11");
				if (!ArrayEquals(uenc, ue, 0, uo))
					AddError("RUT-ST12");
			}
			// test decryption
			try {
				rmt = rm.CreateDecryptor();
				rut = ru.CreateDecryptor();
			} catch {
				AddError("RUT-ST13");
				return 13;
			}
			if (rmt.InputBlockSize != rut.InputBlockSize)
				AddError("RUT-ST14");
			if (rmt.OutputBlockSize != rut.OutputBlockSize)
				AddError("RUT-ST15");
			byte[] md, ud;
			try {
				md = rmt.TransformFinalBlock(me, 0, me.Length);
			} catch {
				AddError("RUT-ST16");
				return 16;
			}
			try {
				ud = rut.TransformFinalBlock(ue, 0, ue.Length);
			} catch {
				AddError("RUT-ST17");
				return 17;
			}
			if (!ArrayEquals(md, ud))
				AddError("RUT-ST18");
			byte[] mdec = null;
			byte[] udec = null;
			int uod = 0, mod = 0;
			try {
				mdec = new byte[me.Length];
				udec = new byte[ue.Length];
				for(int i = 0; i < me.Length; i += rmt.InputBlockSize) {
					if (me.Length > i + rmt.InputBlockSize) {
						mod += rmt.TransformBlock(me, i, rmt.InputBlockSize, mdec, mod);
						uod += rut.TransformBlock(ue, i, rut.InputBlockSize, udec, uod);
					} else {
						if (padding == PaddingMode.PKCS7) {
							byte[] temp = rmt.TransformFinalBlock(me, i, me.Length - i);
							Array.Copy(temp, 0, mdec, mod, temp.Length);
							mod += temp.Length;
							temp = rut.TransformFinalBlock(ue, i, ue.Length - i);
							Array.Copy(temp, 0, udec, uod, temp.Length);
							uod += temp.Length; 
						}
					}
				}
			} catch {
				AddError("RUT-ST19");
			}
			if (mod != uod || !ArrayEquals(mdec, udec, 0, mod))
				AddError("RUT-ST20");
			if (padding == PaddingMode.PKCS7) {
				if (mod != md.Length || uod != ud.Length)
					AddError("RUT-ST21");
				if (!ArrayEquals(mdec, md, 0, md.Length))
					AddError("RUT-ST22");
				if (!ArrayEquals(udec, ud, 0, ud.Length))
					AddError("RUT-ST23");
				if (!ArrayEquals(ud, toEncrypt))
					AddError("RUT-ST24");
			}
			return 24;
		}
		private static string ToString(byte[] bytes) {
			return ToString(bytes, bytes.Length);
		}
		private static string ToString(byte[] bytes, int length) {
			StringBuilder sb = new StringBuilder(bytes.Length * 2);
			for(int i = 0; i < length; i++) {
				sb.Append(bytes[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}
