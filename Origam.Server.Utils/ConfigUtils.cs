using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;

public class ConfigUtils
{
    public static string GetValue(string section, string key)
    {
        NameValueCollection sectionData = (NameValueCollection)
            System.Configuration.ConfigurationManager.GetSection(section);
        if (sectionData == null)
        {
            throw (new ArgumentOutOfRangeException("Section Name not found: " + section));
        }

        string value = sectionData.Get(key);

        return value;
    }

    public static NameValueCollection GetCollection(string section)
    {
        NameValueCollection sectionData = (NameValueCollection)
            System.Configuration.ConfigurationManager.GetSection(section);
        if (sectionData == null)
        {
            throw (new ArgumentOutOfRangeException("Section Name not found: " + section));
        }
        return sectionData;
    }

    public static string AboutUrl()
    {
        return ConfigurationManager.AppSettings["aboutUrl"] ?? "https://www.origam.com";
    }

    public static string BottomLogoUrl()
    {
        return ConfigurationManager.AppSettings["bottomLogoUrl"] ?? "https://www.origam.com";
    }

    public static string SignOutUrl()
    {
        return ConfigurationManager.AppSettings["signOutUrl"] ?? "~/";
    }

    public static bool UserRegistrationAllowed()
    {
        string allowed = ConfigurationManager.AppSettings["userRegistration_Allowed"];
        if (string.IsNullOrEmpty(allowed))
        {
            return false;
        }
        else
        {
            return XmlConvert.ToBoolean(allowed);
        }
    }
}
