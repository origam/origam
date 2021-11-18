using System;
using System.IO;
using Org.Mentalis.Security.Certificates;

/// <summary>
/// This example shows how to convert a CER/PVK file pair to a PFX file.
/// </summary>
class Cert2Pfx {
	static void Main(string[] args) {
		try {
			string pass1, pass2;
			// make sure the user enters correct command line arguments
			if (args.Length < 3) {
				Console.WriteLine("Usage: cert2pfx <certfile> <pvkfile> <pfxfile>");
				return;
			}
			if (!File.Exists(args[0])) {
				Console.WriteLine("The specified certificate could not be found.");
				return;
			}
			if (!File.Exists(args[1])) {
				Console.WriteLine("The specified PVK file could not be found.");
				return;
			}
			if (File.Exists(args[2])) {
				Console.WriteLine("The specified PFX output file already exists.");
				return;
			}
			// create a Certificate intance from the certificate file
			Certificate cert = Certificate.CreateFromCerFile(args[0]);
			// associate the certificate with the specified private key
			try {
				cert.AssociateWithPrivateKey(args[1], null, true);
			} catch {
				// PVK file is probably password protected
				Console.Write("Enter the PVK password: ");
				pass1 = Console.ReadLine();
				cert.AssociateWithPrivateKey(args[1], pass1, true);
			}
			Console.Write("Enter the PFX password: ");
			pass1 = Console.ReadLine();
			Console.Write("Enter it again: ");
			pass2 = Console.ReadLine();
			if (pass1 != pass2) {
				Console.WriteLine("Passwords do not match!");
				return;
			}
			// export the PKCS#12 file
			cert.ToPfxFile(args[2], pass1, true, false);
		} catch (Exception e) {
			Console.WriteLine("The following error has occurred:");
			Console.WriteLine(e);
		}
	}
}