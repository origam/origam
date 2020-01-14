#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using System.Xml;

namespace Origam.Server.Doc
{
    public class DiagramConnection
    {
        public DiagramConnection(string id, string source, string target, string label,
            DiagramConnectionAnchorType sourceAnchorType, DiagramConnectionAnchorType targetAnchorType,
            DiagramConnectionPosition sourcePosition, DiagramConnectionPosition targetPosition,
            int curviness)
        {
            Id = id;
            Source = source;
            Target = target;
            Label = label;
            SourceAnchorType = sourceAnchorType;
            TargetAnchorType = targetAnchorType;
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            Curviness = curviness;
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
        
        private string _source;
        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        private string _target;
        public string Target
        {
            get
            {
                return _target;
            }
            set
            {
                _target = value;
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

        private DiagramConnectionPosition _sourcePosition;
        public DiagramConnectionPosition SourcePosition
        {
            get
            {
                return _sourcePosition;
            }
            set
            {
                _sourcePosition = value;
            }
        }

        private DiagramConnectionPosition _targetPosition;
        public DiagramConnectionPosition TargetPosition
        {
            get
            {
                return _targetPosition;
            }
            set
            {
                _targetPosition = value;
            }
        }

        private DiagramConnectionAnchorType _sourceAnchorType;
        public DiagramConnectionAnchorType SourceAnchorType
        {
            get
            {
                return _sourceAnchorType;
            }
            set
            {
                _sourceAnchorType = value;
            }
        }

        private DiagramConnectionAnchorType _targetAnchorType;
        public DiagramConnectionAnchorType TargetAnchorType
        {
            get
            {
                return _targetAnchorType;
            }
            set
            {
                _targetAnchorType = value;
            }
        }

        private int _curviness;
        public int Curviness
        {
            get
            {
                return _curviness;
            }
            set
            {
                _curviness = value;
            }
        }

        public string SourceAnchor()
        {
            if (SourceAnchorType == DiagramConnectionAnchorType.Custom)
            {
                return string.Format("[{0}, {1}]", XmlConvert.ToString(SourcePosition.X), XmlConvert.ToString(SourcePosition.Y));
            }
            else
            {
                return "\"" + SourceAnchorType.ToString() + "\"";
            }
        }

        public string TargetAnchor()
        {
            if (TargetAnchorType == DiagramConnectionAnchorType.Custom)
            {
                return string.Format("[{0}, {1}]", XmlConvert.ToString(TargetPosition.X), XmlConvert.ToString(TargetPosition.Y));
            }
            else
            {
                return "\"" + TargetAnchorType.ToString() + "\"";
            }
        }
    }
}
