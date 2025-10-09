﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Origam.Windows.Editor
{
    public class XmlElementPathsByNamespace : Collection<XmlElementPath>
    {
        Dictionary<string, XmlElementPath> pathsByNamespace = new Dictionary<string, XmlElementPath>();
        XmlNamespaceCollection namespacesWithoutPaths = new XmlNamespaceCollection();

        public XmlElementPathsByNamespace(XmlElementPath path)
        {
            SeparateIntoPathsByNamespace(path);
            AddSeparatedPathsToCollection();
            FindNamespacesWithoutAssociatedPaths(path.NamespacesInScope);
            pathsByNamespace.Clear();
        }

        void SeparateIntoPathsByNamespace(XmlElementPath path)
        {
            foreach (QualifiedName category in path.Elements)
            {
                XmlElementPath matchedPath = FindOrCreatePath(category.Namespace);
                matchedPath.AddElement(category);
            }
        }

        XmlElementPath FindOrCreatePath(string elementNamespace)
        {
            XmlElementPath path = FindPath(elementNamespace);
            if (path != null)
            {
                return path;
            }
            return CreatePath(elementNamespace);
        }

        XmlElementPath FindPath(string elementNamespace)
        {
            XmlElementPath path;
            if (pathsByNamespace.TryGetValue(elementNamespace, out path))
            {
                return path;
            }
            return null;
        }

        XmlElementPath CreatePath(string elementNamespace)
        {
            XmlElementPath path = new XmlElementPath();
            pathsByNamespace.Add(elementNamespace, path);
            return path;
        }

        void AddSeparatedPathsToCollection()
        {
            foreach (KeyValuePair<string, XmlElementPath> dictionaryEntry in pathsByNamespace)
            {
                Add(dictionaryEntry.Value);
            }
        }

        void FindNamespacesWithoutAssociatedPaths(XmlNamespaceCollection namespacesInScope)
        {
            foreach (XmlNamespace ns in namespacesInScope)
            {
                if (!HavePathForNamespace(ns))
                {
                    namespacesWithoutPaths.Add(ns);
                }
            }
        }

        bool HavePathForNamespace(XmlNamespace ns)
        {
            return pathsByNamespace.ContainsKey(ns.Name);
        }

        public XmlNamespaceCollection NamespacesWithoutPaths
        {
            get { return namespacesWithoutPaths; }
        }
    }
}