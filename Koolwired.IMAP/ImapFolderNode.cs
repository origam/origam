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
    /// Represents the ImapFolderNode class.
    /// </summary>
    #endregion
    public class ImapFolderNode
    {
        #region Private Variables
        ImapFolderNodeList _children;
        ImapFolderNode _parent;
        string _value;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the parent ImapFolderNode for this ImapFolderNode.
        /// </summary>
        public ImapFolderNode Parent
        {
            get { return _parent; }
            set
            {
                if (value == _parent || value.GetType() == typeof(ImapFolder))
                    return;
                if (_parent != null)
                    _parent.Children.Remove(this);
                if (value != null && !value.Children.Contains(this))
                    value.Children.Add(this);
                _parent = value;
            }
        }
        /// <summary>
        /// Gets the root ImapFolderNode.
        /// </summary>
        public ImapFolderNode Root {
            get
            {
                if (_parent == null)
                    return this;
                else
                    return _parent.Root;
            }
        }
        /// <summary>
        /// Gets or sets the list of child ImapFolderNode's
        /// </summary>
        public ImapFolderNodeList Children
        {
            get { return _children; }
            set { _children = value; }
        }
        /// <summary>
        /// Gets or sets a string value for the node.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of the ImapFolderNode.
        /// </summary>
        public ImapFolderNode()
        {
            Parent = null;
            _children = new ImapFolderNodeList(this);
        }
        /// <summary>
        /// Creates an instance of the ImapFolderNode.
        /// </summary>
        /// <param name="value">A string containing the name of the node.</param>
        public ImapFolderNode(string value)
        {
            this.Value = value;
            _children = new ImapFolderNodeList(this);
        }
        /// <summary>
        /// Creates an instance of the ImapFolderNode.
        /// </summary>
        /// <param name="parent">A ImapFolderNode indicating the parent node.</param>
        public ImapFolderNode(ImapFolderNode parent)
        {
            Parent = parent;
            _children = new ImapFolderNodeList(this);
        }
        /// <summary>
        /// Creates an instance of the ImapFolderNode.
        /// </summary>
        /// <param name="children">A ImapFolderNodeList indicating the children of this node.</param>
        public ImapFolderNode(ImapFolderNodeList children)
        {
            Parent = null;
            _children = children;
            _children.Parent = this;
        }
        /// <summary>
        /// Creates an instance of the ImapFolderNode.
        /// </summary>
        /// <param name="parent">A ImapFolderNode indicating the parent node.</param>
        /// <param name="children">A ImapFolderNodeList indicating the children of this node.</param>
        public ImapFolderNode(ImapFolderNode parent, ImapFolderNodeList children)
        {
            Parent = parent;
            _children = Children;
            Children.Parent = this;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Converts the ImapFolderNode to a string representation of the folder.
        /// </summary>
        /// <returns>Returns a string representation of the folder.</returns>
        public override string ToString()
        {
            ImapFolderNode node = this;
            Stack<string> stack = new Stack<string>();
            StringBuilder sb = new StringBuilder();
            char delim = ((ImapFolder)this.Root).Delimiter;
            while (node != null)
            {
                stack.Push(node.Value);
                node = node.Parent;
            }
            while (stack.Count > 0)
            {
                sb.Append(stack.Pop());
                if (stack.Count > 0)
                    sb.Append(delim);
            }
            return sb.ToString();
        }
        #endregion
    }
}
