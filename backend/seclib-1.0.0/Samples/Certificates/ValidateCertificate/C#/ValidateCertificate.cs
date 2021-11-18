using System;
using Org.Mentalis.Security.Certificates;

/// <summary>
/// This example shows how you can validate a certificate.
/// </summary>
class ValidateCertificate {
	static void Main(string[] args) {
		Console.WriteLine("This example shows how you can validate a certificate.");
		// load the certificate from a file
		Certificate cert = Certificate.CreateFromCerFile(@"client.cer");
		// build a certificate chain
		CertificateChain cc = new CertificateChain(cert);
		// validate the chain
		CertificateStatus status = cc.VerifyChain(null, AuthType.Client);
		// interpret the result
		if (status == CertificateStatus.ValidCertificate) {
			Console.WriteLine("The certificate is valid.");
		} else {
			Console.WriteLine("The certificate is not valid [" + status.ToString() + "].");
		}
	}
}