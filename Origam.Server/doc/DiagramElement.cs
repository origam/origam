#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections.Generic;
using Origam.Schema;

namespace Origam.Server.Doc
{
    public class DiagramElement
    {
        public DiagramElement(string id, string label, ISchemaItem link, string linkFilter, int top, int left, string cssClass) 
            : this(id, label, link, linkFilter, top, left, 0, cssClass)
        {
        }

        public DiagramElement(string id, string label, ISchemaItem link, string linkFilter, int top, int left, int width)
            : this(id, label, link, linkFilter, top, left, width, null)
        {
        }

        public DiagramElement(string id, string label, ISchemaItem link, string linkFilter, int top, int left, int width, string cssClass)
        {
            Id = id;
            Label = label;
            Top = top;
            Left = left;
            Width = width;
            Link = link;
            LinkFilter = linkFilter;
            Class = cssClass;
        }

        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        private string _class = "";
        public string Class
        {
            get
            {
                return _class;
            }
            set
            {
                _class = value;
            }
        }

        private string _label;
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
            }
        }

        private string _linkFilter;
        public string LinkFilter
        {
            get
            {
                return _linkFilter;
            }
            set
            {
                _linkFilter = value;
            }
        }

        private ISchemaItem _link;
        public ISchemaItem Link
        {
            get
            {
                return _link;
            }
            set
            {
                _link = value;
            }
        }

        private int _top;
        public int Top
        {
            get
            {
                return _top;
            }
            set
            {
                _top = value;
            }
        }

        private int _left;
        public int Left
        {
            get
            {
                return _left;
            }
            set
            {
                _left = value;
            }
        }

        private int _height = 0;
        public int Height
        {
            get
            {
                int maxBottom = 0;
                foreach (DiagramElement child in this.Children)
                {
                    int bottom = child.Top + child.Height;
                    if (bottom > maxBottom)
                    {
                        maxBottom = bottom;
                    }
                }
                if (this.Children.Count > 0)
                {
                    maxBottom += 30;
                }
                return _height + maxBottom;
            }
            set
            {
                _height = value;
            }
        }

        private int _width = 0;
        public int Width
        {
            get
            {
                if (this.Children.Count == 0) return _width;

                int maxRight = 0;
                foreach (DiagramElement child in this.Children)
                {
                    int right = child.Left + 20 + child.Width;
                    if (right > maxRight)
                    {
                        maxRight = right;
                    }
                }
                return maxRight;
            }
            set
            {
                _width = value;
            }
        }

        private List<DiagramElement> _children = new List<DiagramElement>();
        public List<DiagramElement> Children
        {
            get
            {
                return _children;
            }
        }

        public override string ToString()
        {
            return this.Label + " x=" + this.Left + " y=" + this.Top;
        }
    }
}
