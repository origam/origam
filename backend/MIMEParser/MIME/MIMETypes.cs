/******************************************************************************
    Copyright 2003-2004 Hamid Qureshi and Unruled Boy
    OpenPOP.Net is free software; you can redistribute it and/or modify
    it under the terms of the Lesser GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    OpenPOP.Net is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    Lesser GNU General Public License for more details.

    You should have received a copy of the Lesser GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
/*******************************************************************************/

/*
*Name:			OpenPOP.MIMETypes
*Function:		MIME Types
*Author:		Unruled Boy
*Created:		2004/3/29
*Modified:		2004/3/30 09:10 GMT+8
*Description:
*Changes:
*				2004/3/30 09:10 GMT+8 by Unruled Boy
*					1.Adding full list of MIME Types
*/

using System.Collections;
using System.IO;

namespace OpenPOP;

/// <summary>
/// MIMETypes
/// </summary>
public class MIMETypes
{
    public const string MIMEType_MSTNEF = "application/ms-tnef";
    private const string Content_Transfer_Encoding_Tag = "Content-Transfer-Encoding";
    private const string Content_Transfer_Charset_Tag = "charset";
    private static Hashtable _MIMETypeList = null;

    public static string GetContentTransferEncoding(string strBuffer, int pos)
    {
        int begin = 0,
            end = 0;
        begin = strBuffer
            .ToLower()
            .IndexOf(value: Content_Transfer_Encoding_Tag.ToLower(), startIndex: pos);
        if (begin != -1)
        {
            end = strBuffer.ToLower().IndexOf(value: "\r\n".ToLower(), startIndex: begin + 1);
            return strBuffer
                .Substring(
                    startIndex: begin + Content_Transfer_Encoding_Tag.Length + 1,
                    length: end - begin - Content_Transfer_Encoding_Tag.Length
                )
                .Trim();
        }

        return "";
    }

    public static string GetContentCharset(string strBuffer, int pos)
    {
        int begin = 0,
            end = 0;
        begin = strBuffer
            .ToLower()
            .IndexOf(value: Content_Transfer_Charset_Tag.ToLower(), startIndex: pos);
        if (begin != -1)
        {
            end = strBuffer.ToLower().IndexOf(value: "\r\n".ToLower(), startIndex: begin + 1);
            string result = strBuffer
                .Substring(
                    startIndex: begin + Content_Transfer_Charset_Tag.Length + 1,
                    length: end - begin - Content_Transfer_Charset_Tag.Length
                )
                .Trim();
            if (result.StartsWith(value: "\""))
            {
                result = result.Substring(startIndex: 1, length: result.Length - 2);
            }
            return result;
        }

        return "";
    }

    public static bool IsMSTNEF(string strContentType)
    {
        if (strContentType != null & strContentType != "")
        {
            if (strContentType.ToLower() == MIMEType_MSTNEF.ToLower())
            {
                return true;
            }

            return false;
        }

        return false;
    }

    public static string ContentType(string strExtension)
    {
        if (_MIMETypeList.ContainsKey(key: strExtension))
        {
            return _MIMETypeList[key: strExtension].ToString();
        }

        return null;
    }

    public static Hashtable MIMETypeList
    {
        get { return _MIMETypeList; }
        set { _MIMETypeList = value; }
    }

    ~MIMETypes()
    {
        _MIMETypeList.Clear();
        _MIMETypeList = null;
    }

    public MIMETypes()
    {
        _MIMETypeList.Add(key: ".323", value: "text/h323");
        _MIMETypeList.Add(key: ".3gp", value: "video/3gpp");
        _MIMETypeList.Add(key: ".3gpp", value: "video/3gpp");
        _MIMETypeList.Add(key: ".acp", value: "audio/x-mei-aac");
        _MIMETypeList.Add(key: ".act", value: "text/xml");
        _MIMETypeList.Add(key: ".actproj", value: "text/plain");
        _MIMETypeList.Add(key: ".ade", value: "application/msaccess");
        _MIMETypeList.Add(key: ".adp", value: "application/msaccess");
        _MIMETypeList.Add(key: ".ai", value: "application/postscript");
        _MIMETypeList.Add(key: ".aif", value: "audio/aiff");
        _MIMETypeList.Add(key: ".aifc", value: "audio/aiff");
        _MIMETypeList.Add(key: ".aiff", value: "audio/aiff");
        _MIMETypeList.Add(key: ".asf", value: "video/x-ms-asf");
        _MIMETypeList.Add(key: ".asm", value: "text/plain");
        _MIMETypeList.Add(key: ".asx", value: "video/x-ms-asf");
        _MIMETypeList.Add(key: ".au", value: "audio/basic");
        _MIMETypeList.Add(key: ".avi", value: "video/avi");
        _MIMETypeList.Add(key: ".bmp", value: "image/bmp");
        _MIMETypeList.Add(key: ".bwp", value: "application/x-bwpreview");
        _MIMETypeList.Add(key: ".c", value: "text/plain");
        _MIMETypeList.Add(key: ".cat", value: "application/vnd.ms-pki.seccat");
        _MIMETypeList.Add(key: ".cc", value: "text/plain");
        _MIMETypeList.Add(key: ".cdf", value: "application/x-cdf");
        _MIMETypeList.Add(key: ".cer", value: "application/x-x509-ca-cert");
        _MIMETypeList.Add(key: ".cod", value: "text/plain");
        _MIMETypeList.Add(key: ".cpp", value: "text/plain");
        _MIMETypeList.Add(key: ".crl", value: "application/pkix-crl");
        _MIMETypeList.Add(key: ".crt", value: "application/x-x509-ca-cert");
        _MIMETypeList.Add(key: ".cs", value: "text/plain");
        _MIMETypeList.Add(key: ".css", value: "text/css");
        _MIMETypeList.Add(key: ".csv", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".cxx", value: "text/plain");
        _MIMETypeList.Add(key: ".dbs", value: "text/plain");
        _MIMETypeList.Add(key: ".def", value: "text/plain");
        _MIMETypeList.Add(key: ".der", value: "application/x-x509-ca-cert");
        _MIMETypeList.Add(key: ".dib", value: "image/bmp");
        _MIMETypeList.Add(key: ".dif", value: "video/x-dv");
        _MIMETypeList.Add(key: ".dll", value: "application/x-msdownload");
        _MIMETypeList.Add(key: ".doc", value: "application/msword");
        _MIMETypeList.Add(key: ".dot", value: "application/msword");
        _MIMETypeList.Add(key: ".dsp", value: "text/plain");
        _MIMETypeList.Add(key: ".dsw", value: "text/plain");
        _MIMETypeList.Add(key: ".dv", value: "video/x-dv");
        _MIMETypeList.Add(key: ".edn", value: "application/vnd.adobe.edn");
        _MIMETypeList.Add(key: ".eml", value: "message/rfc822");
        _MIMETypeList.Add(key: ".eps", value: "application/postscript");
        _MIMETypeList.Add(key: ".etd", value: "application/x-ebx");
        _MIMETypeList.Add(key: ".etp", value: "text/plain");
        _MIMETypeList.Add(key: ".exe", value: "application/x-msdownload");
        _MIMETypeList.Add(key: ".ext", value: "text/plain");
        _MIMETypeList.Add(key: ".fdf", value: "application/vnd.fdf");
        _MIMETypeList.Add(key: ".fif", value: "application/fractals");
        _MIMETypeList.Add(key: ".fky", value: "text/plain");
        _MIMETypeList.Add(key: ".gif", value: "image/gif");
        _MIMETypeList.Add(key: ".gz", value: "application/x-gzip");
        _MIMETypeList.Add(key: ".h", value: "text/plain");
        _MIMETypeList.Add(key: ".hpp", value: "text/plain");
        _MIMETypeList.Add(key: ".hqx", value: "application/mac-binhex40");
        _MIMETypeList.Add(key: ".hta", value: "application/hta");
        _MIMETypeList.Add(key: ".htc", value: "text/x-component");
        _MIMETypeList.Add(key: ".htm", value: "text/html");
        _MIMETypeList.Add(key: ".html", value: "text/html");
        _MIMETypeList.Add(key: ".htt", value: "text/webviewhtml");
        _MIMETypeList.Add(key: ".hxx", value: "text/plain");
        _MIMETypeList.Add(key: ".i", value: "text/plain");
        _MIMETypeList.Add(key: ".iad", value: "application/x-iad");
        _MIMETypeList.Add(key: ".ico", value: "image/x-icon");
        _MIMETypeList.Add(key: ".ics", value: "text/calendar");
        _MIMETypeList.Add(key: ".idl", value: "text/plain");
        _MIMETypeList.Add(key: ".iii", value: "application/x-iphone");
        _MIMETypeList.Add(key: ".inc", value: "text/plain");
        _MIMETypeList.Add(key: ".infopathxml", value: "application/ms-infopath.xml");
        _MIMETypeList.Add(key: ".inl", value: "text/plain");
        _MIMETypeList.Add(key: ".ins", value: "application/x-internet-signup");
        _MIMETypeList.Add(key: ".iqy", value: "text/x-ms-iqy");
        _MIMETypeList.Add(key: ".isp", value: "application/x-internet-signup");
        _MIMETypeList.Add(key: ".java", value: "text/java");
        _MIMETypeList.Add(key: ".jfif", value: "image/jpeg");
        _MIMETypeList.Add(key: ".jnlp", value: "application/x-java-jnlp-file");
        _MIMETypeList.Add(key: ".jpe", value: "image/jpeg");
        _MIMETypeList.Add(key: ".jpeg", value: "image/jpeg");
        _MIMETypeList.Add(key: ".jpg", value: "image/jpeg");
        _MIMETypeList.Add(key: ".jsl", value: "text/plain");
        _MIMETypeList.Add(key: ".kci", value: "text/plain");
        _MIMETypeList.Add(key: ".la1", value: "audio/x-liquid-file");
        _MIMETypeList.Add(key: ".lar", value: "application/x-laplayer-reg");
        _MIMETypeList.Add(key: ".latex", value: "application/x-latex");
        _MIMETypeList.Add(key: ".lavs", value: "audio/x-liquid-secure");
        _MIMETypeList.Add(key: ".lgn", value: "text/plain");
        _MIMETypeList.Add(key: ".lmsff", value: "audio/x-la-lms");
        _MIMETypeList.Add(key: ".lqt", value: "audio/x-la-lqt");
        _MIMETypeList.Add(key: ".lst", value: "text/plain");
        _MIMETypeList.Add(key: ".m1v", value: "video/mpeg");
        _MIMETypeList.Add(key: ".m3u", value: "audio/mpegurl");
        _MIMETypeList.Add(key: ".m4e", value: "video/mpeg4");
        _MIMETypeList.Add(key: ".MAC", value: "image/x-macpaint");
        _MIMETypeList.Add(key: ".mak", value: "text/plain");
        _MIMETypeList.Add(key: ".man", value: "application/x-troff-man");
        _MIMETypeList.Add(key: ".map", value: "text/plain");
        _MIMETypeList.Add(key: ".mda", value: "application/msaccess");
        _MIMETypeList.Add(key: ".mdb", value: "application/msaccess");
        _MIMETypeList.Add(key: ".mde", value: "application/msaccess");
        _MIMETypeList.Add(key: ".mdi", value: "image/vnd.ms-modi");
        _MIMETypeList.Add(key: ".mfp", value: "application/x-shockwave-flash");
        _MIMETypeList.Add(key: ".mht", value: "message/rfc822");
        _MIMETypeList.Add(key: ".mhtml", value: "message/rfc822");
        _MIMETypeList.Add(key: ".mid", value: "audio/mid");
        _MIMETypeList.Add(key: ".midi", value: "audio/mid");
        _MIMETypeList.Add(key: ".mk", value: "text/plain");
        _MIMETypeList.Add(key: ".mnd", value: "audio/x-musicnet-download");
        _MIMETypeList.Add(key: ".mns", value: "audio/x-musicnet-stream");
        _MIMETypeList.Add(key: ".MP1", value: "audio/mp1");
        _MIMETypeList.Add(key: ".mp2", value: "video/mpeg");
        _MIMETypeList.Add(key: ".mp2v", value: "video/mpeg");
        _MIMETypeList.Add(key: ".mp3", value: "audio/mpeg");
        _MIMETypeList.Add(key: ".mp4", value: "video/mp4");
        _MIMETypeList.Add(key: ".mpa", value: "video/mpeg");
        _MIMETypeList.Add(key: ".mpe", value: "video/mpeg");
        _MIMETypeList.Add(key: ".mpeg", value: "video/mpeg");
        _MIMETypeList.Add(key: ".mpf", value: "application/vnd.ms-mediapackage");
        _MIMETypeList.Add(key: ".mpg", value: "video/mpeg");
        _MIMETypeList.Add(key: ".mpg4", value: "video/mp4");
        _MIMETypeList.Add(key: ".mpga", value: "audio/rn-mpeg");
        _MIMETypeList.Add(key: ".mpv2", value: "video/mpeg");
        _MIMETypeList.Add(key: ".NMW", value: "application/nmwb");
        _MIMETypeList.Add(key: ".nws", value: "message/rfc822");
        _MIMETypeList.Add(key: ".odc", value: "text/x-ms-odc");
        _MIMETypeList.Add(key: ".odh", value: "text/plain");
        _MIMETypeList.Add(key: ".odl", value: "text/plain");
        _MIMETypeList.Add(key: ".p10", value: "application/pkcs10");
        _MIMETypeList.Add(key: ".p12", value: "application/x-pkcs12");
        _MIMETypeList.Add(key: ".p7b", value: "application/x-pkcs7-certificates");
        _MIMETypeList.Add(key: ".p7c", value: "application/pkcs7-mime");
        _MIMETypeList.Add(key: ".p7m", value: "application/pkcs7-mime");
        _MIMETypeList.Add(key: ".p7r", value: "application/x-pkcs7-certreqresp");
        _MIMETypeList.Add(key: ".p7s", value: "application/pkcs7-signature");
        _MIMETypeList.Add(key: ".PCT", value: "image/pict");
        _MIMETypeList.Add(key: ".pdf", value: "application/pdf");
        _MIMETypeList.Add(key: ".pdx", value: "application/vnd.adobe.pdx");
        _MIMETypeList.Add(key: ".pfx", value: "application/x-pkcs12");
        _MIMETypeList.Add(key: ".pic", value: "image/pict");
        _MIMETypeList.Add(key: ".PICT", value: "image/pict");
        _MIMETypeList.Add(key: ".pko", value: "application/vnd.ms-pki.pko");
        _MIMETypeList.Add(key: ".png", value: "image/png");
        _MIMETypeList.Add(key: ".pnt", value: "image/x-macpaint");
        _MIMETypeList.Add(key: ".pntg", value: "image/x-macpaint");
        _MIMETypeList.Add(key: ".pot", value: "application/vnd.ms-powerpoint");
        _MIMETypeList.Add(key: ".ppa", value: "application/vnd.ms-powerpoint");
        _MIMETypeList.Add(key: ".pps", value: "application/vnd.ms-powerpoint");
        _MIMETypeList.Add(key: ".ppt", value: "application/vnd.ms-powerpoint");
        _MIMETypeList.Add(key: ".prc", value: "text/plain");
        _MIMETypeList.Add(key: ".prf", value: "application/pics-rules");
        _MIMETypeList.Add(key: ".ps", value: "application/postscript");
        _MIMETypeList.Add(key: ".pub", value: "application/vnd.ms-publisher");
        _MIMETypeList.Add(key: ".pwz", value: "application/vnd.ms-powerpoint");
        _MIMETypeList.Add(key: ".qt", value: "video/quicktime");
        _MIMETypeList.Add(key: ".qti", value: "image/x-quicktime");
        _MIMETypeList.Add(key: ".qtif", value: "image/x-quicktime");
        _MIMETypeList.Add(key: ".qtl", value: "application/x-quicktimeplayer");
        _MIMETypeList.Add(key: ".qup", value: "application/x-quicktimeupdater");
        _MIMETypeList.Add(key: ".r1m", value: "application/vnd.rn-recording");
        _MIMETypeList.Add(key: ".r3t", value: "text/vnd.rn-realtext3d");
        _MIMETypeList.Add(key: ".RA", value: "audio/vnd.rn-realaudio");
        _MIMETypeList.Add(key: ".RAM", value: "audio/x-pn-realaudio");
        _MIMETypeList.Add(key: ".rat", value: "application/rat-file");
        _MIMETypeList.Add(key: ".rc", value: "text/plain");
        _MIMETypeList.Add(key: ".rc2", value: "text/plain");
        _MIMETypeList.Add(key: ".rct", value: "text/plain");
        _MIMETypeList.Add(key: ".rec", value: "application/vnd.rn-recording");
        _MIMETypeList.Add(key: ".rgs", value: "text/plain");
        _MIMETypeList.Add(key: ".rjs", value: "application/vnd.rn-realsystem-rjs");
        _MIMETypeList.Add(key: ".rjt", value: "application/vnd.rn-realsystem-rjt");
        _MIMETypeList.Add(key: ".RM", value: "application/vnd.rn-realmedia");
        _MIMETypeList.Add(key: ".rmf", value: "application/vnd.adobe.rmf");
        _MIMETypeList.Add(key: ".rmi", value: "audio/mid");
        _MIMETypeList.Add(key: ".RMJ", value: "application/vnd.rn-realsystem-rmj");
        _MIMETypeList.Add(key: ".RMM", value: "audio/x-pn-realaudio");
        _MIMETypeList.Add(key: ".rms", value: "application/vnd.rn-realmedia-secure");
        _MIMETypeList.Add(key: ".rmvb", value: "application/vnd.rn-realmedia-vbr");
        _MIMETypeList.Add(key: ".RMX", value: "application/vnd.rn-realsystem-rmx");
        _MIMETypeList.Add(key: ".RNX", value: "application/vnd.rn-realplayer");
        _MIMETypeList.Add(key: ".rp", value: "image/vnd.rn-realpix");
        _MIMETypeList.Add(key: ".RPM", value: "audio/x-pn-realaudio-plugin");
        _MIMETypeList.Add(key: ".rqy", value: "text/x-ms-rqy");
        _MIMETypeList.Add(key: ".rsml", value: "application/vnd.rn-rsml");
        _MIMETypeList.Add(key: ".rt", value: "text/vnd.rn-realtext");
        _MIMETypeList.Add(key: ".rtf", value: "application/msword");
        _MIMETypeList.Add(key: ".rul", value: "text/plain");
        _MIMETypeList.Add(key: ".RV", value: "video/vnd.rn-realvideo");
        _MIMETypeList.Add(key: ".s", value: "text/plain");
        _MIMETypeList.Add(key: ".sc2", value: "application/schdpl32");
        _MIMETypeList.Add(key: ".scd", value: "application/schdpl32");
        _MIMETypeList.Add(key: ".sch", value: "application/schdpl32");
        _MIMETypeList.Add(key: ".sct", value: "text/scriptlet");
        _MIMETypeList.Add(key: ".sd2", value: "audio/x-sd2");
        _MIMETypeList.Add(key: ".sdp", value: "application/sdp");
        _MIMETypeList.Add(key: ".sit", value: "application/x-stuffit");
        _MIMETypeList.Add(key: ".slk", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".sln", value: "application/octet-stream");
        _MIMETypeList.Add(key: ".SMI", value: "application/smil");
        _MIMETypeList.Add(key: ".smil", value: "application/smil");
        _MIMETypeList.Add(key: ".snd", value: "audio/basic");
        _MIMETypeList.Add(key: ".snp", value: "application/msaccess");
        _MIMETypeList.Add(key: ".spc", value: "application/x-pkcs7-certificates");
        _MIMETypeList.Add(key: ".spl", value: "application/futuresplash");
        _MIMETypeList.Add(key: ".sql", value: "text/plain");
        _MIMETypeList.Add(key: ".srf", value: "text/plain");
        _MIMETypeList.Add(key: ".ssm", value: "application/streamingmedia");
        _MIMETypeList.Add(key: ".sst", value: "application/vnd.ms-pki.certstore");
        _MIMETypeList.Add(key: ".stl", value: "application/vnd.ms-pki.stl");
        _MIMETypeList.Add(key: ".swf", value: "application/x-shockwave-flash");
        _MIMETypeList.Add(key: ".tab", value: "text/plain");
        _MIMETypeList.Add(key: ".tar", value: "application/x-tar");
        _MIMETypeList.Add(key: ".tdl", value: "text/xml");
        _MIMETypeList.Add(key: ".tgz", value: "application/x-compressed");
        _MIMETypeList.Add(key: ".tif", value: "image/tiff");
        _MIMETypeList.Add(key: ".tiff", value: "image/tiff");
        _MIMETypeList.Add(key: ".tlh", value: "text/plain");
        _MIMETypeList.Add(key: ".tli", value: "text/plain");
        _MIMETypeList.Add(key: ".torrent", value: "application/x-bittorrent");
        _MIMETypeList.Add(key: ".trg", value: "text/plain");
        _MIMETypeList.Add(key: ".txt", value: "text/plain");
        _MIMETypeList.Add(key: ".udf", value: "text/plain");
        _MIMETypeList.Add(key: ".udt", value: "text/plain");
        _MIMETypeList.Add(key: ".uls", value: "text/iuls");
        _MIMETypeList.Add(key: ".user", value: "text/plain");
        _MIMETypeList.Add(key: ".usr", value: "text/plain");
        _MIMETypeList.Add(key: ".vb", value: "text/plain");
        _MIMETypeList.Add(key: ".vcf", value: "text/x-vcard");
        _MIMETypeList.Add(key: ".vcproj", value: "text/plain");
        _MIMETypeList.Add(key: ".viw", value: "text/plain");
        _MIMETypeList.Add(key: ".vpg", value: "application/x-vpeg005");
        _MIMETypeList.Add(key: ".vspscc", value: "text/plain");
        _MIMETypeList.Add(key: ".vsscc", value: "text/plain");
        _MIMETypeList.Add(key: ".vssscc", value: "text/plain");
        _MIMETypeList.Add(key: ".wav", value: "audio/wav");
        _MIMETypeList.Add(key: ".wax", value: "audio/x-ms-wax");
        _MIMETypeList.Add(key: ".wbk", value: "application/msword");
        _MIMETypeList.Add(key: ".wiz", value: "application/msword");
        _MIMETypeList.Add(key: ".wm", value: "video/x-ms-wm");
        _MIMETypeList.Add(key: ".wma", value: "audio/x-ms-wma");
        _MIMETypeList.Add(key: ".wmd", value: "application/x-ms-wmd");
        _MIMETypeList.Add(key: ".wmv", value: "video/x-ms-wmv");
        _MIMETypeList.Add(key: ".wmx", value: "video/x-ms-wmx");
        _MIMETypeList.Add(key: ".wmz", value: "application/x-ms-wmz");
        _MIMETypeList.Add(key: ".wpl", value: "application/vnd.ms-wpl");
        _MIMETypeList.Add(key: ".wprj", value: "application/webzip");
        _MIMETypeList.Add(key: ".wsc", value: "text/scriptlet");
        _MIMETypeList.Add(key: ".wvx", value: "video/x-ms-wvx");
        _MIMETypeList.Add(key: ".XBM", value: "image/x-xbitmap");
        _MIMETypeList.Add(key: ".xdp", value: "application/vnd.adobe.xdp+xml");
        _MIMETypeList.Add(key: ".xfd", value: "application/vnd.adobe.xfd+xml");
        _MIMETypeList.Add(key: ".xfdf", value: "application/vnd.adobe.xfdf");
        _MIMETypeList.Add(key: ".xla", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xlb", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xlc", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xld", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xlk", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xll", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xlm", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xls", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xlt", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xlv", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xlw", value: "application/vnd.ms-excel");
        _MIMETypeList.Add(key: ".xml", value: "text/xml");
        _MIMETypeList.Add(key: ".xpl", value: "audio/scpls");
        _MIMETypeList.Add(key: ".xsl", value: "text/xml");
        _MIMETypeList.Add(key: ".z", value: "application/x-compress");
        _MIMETypeList.Add(key: ".zip", value: "application/x-zip-compressed");
    }

    /// <summary>Returns the MIME content-type for the supplied file extension</summary>
    /// <returns>string MIME type (Example: \"text/plain\")</returns>
    public static string GetMimeType(string strFileName)
    {
        try
        {
            string strFileExtension = new FileInfo(fileName: strFileName).Extension;
            string strContentType = null;

            strContentType = MIMETypes.ContentType(strExtension: strFileExtension);

            if (strContentType.ToString() != null)
            {
                return strContentType.ToString();
            }

            return "application/octet-stream";
        }
        catch (System.Exception)
        {
            return "application/octet-stream";
        }
    }
}
