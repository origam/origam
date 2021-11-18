imports System
imports Org.Mentalis.Security.Certificates

' <summary>
' This example shows how you can validate a certificate.
' </summary>
class ValidateCertificate
	public shared sub Main(args as string())
		Console.WriteLine("This example shows how you can validate a certificate.")
		' load the certificate from a file
		dim cert as Certificate = Certificate.CreateFromCerFile("client.cer")
		' build a certificate chain
		dim cc as CertificateChain = new CertificateChain(cert)
		' validate the chain
		dim status as CertificateStatus = cc.VerifyChain(nothing, AuthType.Client)
		' interpret the result
		if (status = CertificateStatus.ValidCertificate)
			Console.WriteLine("The certificate is valid.")
		else
			Console.WriteLine("The certificate is not valid [" + status.ToString() + "].")
		end if
	end sub
end class