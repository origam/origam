#region Copyright (c) Koolwired Solutions, LLC.
/*--------------------------------------------------------------------------
 * Copyright (c) 2006-2009, Koolwired Solutions, LLC.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer. 
 * Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution. 
 * Neither the name of Koolwired Solutions, LLC. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission. 
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
 * AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
 * THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
 * OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
 * WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 *--------------------------------------------------------------------------*/
#endregion

#region History
/*--------------------------------------------------------------------------
 * Modification History: 
 * Date       Programmer      Description
 * 03/14/2009 Keith Kikta     Inital release. 
 *--------------------------------------------------------------------------*/
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace Koolwired.Imap
{
    #region Header
    /// <summary>
    /// Represents the ImapFolder class.
    /// </summary>
    #endregion
    public class ImapFolder : ImapFolderNode
    {
        #region Private Variables
        const string PARSE_FOLDER = @"[^\(]*\(\\(?<children>[^\)]+)\)\s\""(?<seperator>[^\""]*)\""\s\""(?<folder>[^\""]*)\""";
        char _delimiter;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets a string that is used as the folder delimiter.
        /// </summary>
        public char Delimiter
        {
            get { return _delimiter; }
            set { _delimiter = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of the imap folder.
        /// </summary>
        public ImapFolder() { }
        #endregion

        #region Internal Methods
        internal static ImapFolder ParseFolder(List<string> folders)
        {
            ImapFolder folder = new ImapFolder();
            for (int i = 0; i < folders.Count; i++)
            {
                Match match = Regex.Match(folders[i], PARSE_FOLDER);
                folder.Delimiter = match.Groups["seperator"].Value[0];
                string[] parts = match.Groups["folder"].Value.Split(folder.Delimiter);
                ImapFolderNode node = folder;
                for (int j = 0; j < parts.Length; j++)
                    if (node.Children.HasNode(parts[j]))
                        node = node.Children[parts[j]];
                    else
                        node = node.Children.Add(new ImapFolderNode(parts[j]));
            }
            return folder;
        }
        #endregion
    }
}
