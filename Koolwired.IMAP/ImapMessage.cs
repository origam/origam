#region References
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace Koolwired.Imap
{
    public class ImapMessage
    {
        #region Private Properties
        private long _id;
        private long _uid;
        private ImapMessageHeaders _headers;
        #endregion
        #region Public Properties
        public long ID
        {
            get { return _id; }
            set { _id = value; }
        }
        public long UID
        {
            get { return _uid; }
            set { _uid = value; }
        }
        public ImapMessageHeaders Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }
        #endregion

        #region Public Constructor

        #endregion

        #region Private Methods
        void Parse(string data, MessageParser parser)
        {
            switch (parser)
            {
                case MessageParser.UID:
                    Match match = Regex.Match(data, ImapRegularExpressions.PARSE_FETCH_UID, RegexOptions.ExplicitCapture);
                    this.ID = Convert.ToInt64(match.Groups["ID"].Value);
                    this.UID = Convert.ToInt64(match.Groups["UID"].Value);
                    break;
                case MessageParser.Header:
                    break;
                case MessageParser.Body:
                    break;
            }
        }
        #endregion
    }
}
