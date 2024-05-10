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

using System.Collections.Generic;

namespace Origam.DA.Service.FileSystemModeCheckers
{
    public class ModelErrorSection
    {
        public ModelErrorSection(string caption, List<ErrorMessage> errorMessages)
        {
            Caption = caption;
            ErrorMessages = errorMessages;
        }

        public string Caption { get; }
        public List<ErrorMessage> ErrorMessages { get; }

        public bool IsEmpty => ErrorMessages == null || ErrorMessages.Count == 0;

        public override string ToString()
        {
            return Caption;
        }
    }

    public class ErrorMessage
    {
        public string Text { get; }
        public string Link { get; }
        public static ErrorMessage Empty = new ErrorMessage(" ");

        public ErrorMessage(string text)
        {
            Text = text;
        }

        public ErrorMessage(string text, string link)
        {
            Text = text;
            Link = link;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
