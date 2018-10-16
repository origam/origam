namespace Koolwired.Imap
{
    internal class ImapRegularExpressions
    {
        /*
         * ^\*\s(?<ID>\d+)\s
         * FETCH\s
         * \(UID\s(?<UID>\d+)\).*$
         */
        internal const string PARSE_FETCH_UID = @"^\*\s(?<ID>\d+)\s[Ff][Ee][Tt][Cc][Hh]\s\(UID\s(?<UID>\d+)\).*$";
        /*
         * \*\s(?<ID>\d+)\s
         * FETCH\s(\(|)
         * FLAGS\s\((?<FLAGS>[^\)]*)\)\s
         * INTERNALDATE\s\"(?<RECEIVED>[^\"]+)\"\s
         * RFC822.SIZE\s(?<SIZE>\d+)\s
         * ENVELOPE\s\(
         * \"(?<SENT>([^\"]*|NIL))\"\s
         * (?<SUBJECT>([^\(]*|NIL))
         * (\((?<FROM>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<FROM>NIL))\s
         * (\((?<SENDER>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<SENDER>NIL))\s
         * (\((?<REPLY>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<REPLY>NIL))\s
         * (\((?<TO>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<TO>NIL))\s
         * (\((?<CC>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<CC>NIL))\s
         * (\((?<BCC>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<BCC>NIL))\s
         * (?<REFERENCE>\"<([^>]+)>\"|NIL)\s
         * (?<MESSAGEID>\"<([^>]+)>\"|NIL)\)(\)|)
         */
        internal const string PARSE_FETCH_ALL = @"\*\s(?<ID>\d+)\sFETCH\s(\(|)FLAGS\s\((?<FLAGS>[^\)]*)\)\sINTERNALDATE\s\""(?<RECEIVED>[^\""]+)\""\sRFC822.SIZE\s(?<SIZE>\d+)\sENVELOPE\s\(\""(?<SENT>([^\""]*|NIL))\""\s(?<SUBJECT>([^\(]*|NIL))(\((?<FROM>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<FROM>NIL))\s(\((?<SENDER>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<SENDER>NIL))\s(\((?<REPLY>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<REPLY>NIL))\s(\((?<TO>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<TO>NIL))\s(\((?<CC>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<CC>NIL))\s(\((?<BCC>(?>\((?<LEVEL>)|\)(?<-LEVEL>)|(?! \( | \) ).)+(?(LEVEL)(?!)))\)|(?<BCC>NIL))\s(?<REFERENCE>\""<([^>]+)>\""|NIL)\s(?<MESSAGEID>\""<([^>]+)>\""|NIL)\)(\)|)$";
    }
}