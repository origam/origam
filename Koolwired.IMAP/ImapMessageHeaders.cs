using System;
using System.Collections.Generic;
using System.Text;

namespace Koolwired.Imap
{
    public class ImapMessageHeaders
    {
        #region Private Properties
        private string _raw;
        #endregion
        #region Public Properties
        public string Raw
        {
            get { return _raw; }
            set { _raw = value; }
        }
        #endregion

        #region Constructor
        public ImapMessageHeaders(string data)
        {
            this.Raw = data;
        }
        #endregion
    }
}
