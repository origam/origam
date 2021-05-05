#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;

namespace Origam
{
    /// <summary>
    /// Use this attribute on all classes that require help for the users.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class HelpTopicAttribute : Attribute
    {
        private string _topic;

        /// <summary>
        /// The constructor for the Topic attribute.
        /// </summary>
        /// <param name="topic">Help topic (part of url that will be appended to the base Help url).</param>
        public HelpTopicAttribute(string topic)
        {
            _topic = topic;
        }

        /// <summary>
        /// The name of the data structure entity used to store instances of this class.
        /// </summary>
        public string Topic
        {
            get { return _topic; }
        }
    }
}
