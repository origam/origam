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
#endregion

namespace Koolwired.Imap
{
    #region Header
    /// <summary>
    /// Represents the ImapFolderNodeList class.
    /// </summary>
    #endregion
    public class ImapFolderNodeList : List<ImapFolderNode>
    {
        #region Private Variables
        ImapFolderNode _parent;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets an ImapFolderNode Parent.
        /// </summary>
        public ImapFolderNode Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        /// <summary>
        /// Gets or sets an ImapFolderNode.
        /// </summary>
        /// <param name="name">A string containing the name of the folder.</param>
        /// <returns>Returns an ImapFolderNode.</returns>
        public ImapFolderNode this[string name]
        {
            get { return this[GetIndexOf(name)]; }
            set { this[GetIndexOf(name)] = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of an ImapFolderNode.
        /// </summary>
        /// <param name="parent">A ImapFolderNode indicating the parent to this node.</param>
        public ImapFolderNodeList(ImapFolderNode parent)
        {
            _parent = parent;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a node to the tree.
        /// </summary>
        /// <param name="node">An ImapFolderNode to be added to the tree.</param>
        /// <returns>Returns the created ImapFolderNode.</returns>
        public new ImapFolderNode Add(ImapFolderNode node)
        {
            base.Add(node);
            node.Parent = _parent;
            return node;
        }
        /// <summary>
        /// Adds a node to the tree.
        /// </summary>
        /// <param name="value">A string containing the name of the folder to add to the tree.</param>
        /// <returns>Returns the created ImapFolderNode.</returns>
        public ImapFolderNode Add(string value)
        {
            ImapFolderNode node = new ImapFolderNode(_parent);
            node.Value = value;
            return node;
        }
        /// <summary>
        /// Removes a node from the tree.
        /// </summary>
        /// <param name="node">A ImapFolderNode to remove from the tree.</param>
        public new void Remove(ImapFolderNode node)
        {
            if (node != null)
                node.Parent = null;
            base.Remove(node);
        }
        /// <summary>
        /// Determines if a folder node by the specified name exists in the tree.
        /// </summary>
        /// <param name="value">A string containing the folder name.</param>
        /// <returns>Returns a boolean value indicating if the folder node exists.</returns>
        public bool HasNode(string value)
        {
            try
            {
                GetIndexOf(value);
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
        #endregion

        #region Private Methods
        int GetIndexOf(string folder)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i].Value == folder)
                    return i;
            throw new IndexOutOfRangeException("Folder " + folder + " does not exist in the collection.");
        }
        #endregion
    }
}
