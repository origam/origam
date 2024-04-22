namespace Origam.Server.Authorization;

public class UpperInvariantLookupNormalizer : Microsoft.AspNetCore.Identity.ILookupNormalizer
{
    public string NormalizeEmail(string email)
    {
            return email;
        }

    public string NormalizeName(string name)
    {
            return name;
        }
}