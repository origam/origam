#region license
/*
Copyright 2005 - 2017 Advantage Solutions, s. r. o.

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
using System.Collections;

namespace Origam.Server
{
    public class UIRequest
    {
        private IDictionary _parameters = new Hashtable();
        private string _formSessionId;
        private string _parentSessionId;
        private string _sourceActionId;
        private bool _isStandalone = false;
        private bool _isDataOnly = false;
        private UIRequestType _type;
        private string _caption;
        private string _objectId;
        private string _icon;
        private bool _isSingleRecordEdit;
        private bool _requestCurrentRecordId;
        private int _dialogWidth;
        private int _dialogHeight;
        private bool _supportsPagedData = false;
        private bool _isModalDialog = false;
        private bool _isNewSession = true;
        private List<string> _cachedFormIds = new List<string>();

        public UIRequest()
        {
        }

        public bool SupportsPagedData
        {
            get { return _supportsPagedData; }
            set { _supportsPagedData = value; }
        }

        public IDictionary Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public List<string> CachedFormIds
        {
            get { return _cachedFormIds; }
            set { _cachedFormIds = value; }
        }

        public string FormSessionId
        {
            get { return _formSessionId; }
            set { _formSessionId = value; }
        }

        public bool IsNewSession
        {
            get { return _isNewSession; }
            set { _isNewSession = value; }
        }

        public string ParentSessionId
        {
            get { return _parentSessionId; }
            set { _parentSessionId = value; }
        }

        public string SourceActionId
        {
            get { return _sourceActionId; }
            set { _sourceActionId = value; }
        }

        public bool IsStandalone
        {
            get { return _isStandalone; }
            set { _isStandalone = value; }
        }

        public bool IsDataOnly
        {
            get { return _isDataOnly; }
            set { _isDataOnly = value; }
        }

        public string TypeString
        {
            get { return _type.ToString(); }
        }

        public UIRequestType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        
        public string Caption
        {
            get { return _caption; }
            set { _caption = value; }
        }

        public string Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }
	
        public string ObjectId
        {
            get { return _objectId; }
            set { _objectId = value; }
        }

        public bool IsSingleRecordEdit
        {
            get { return _isSingleRecordEdit; }
            set { _isSingleRecordEdit = value; }
        }

        public bool RequestCurrentRecordId
        {
            get { return _requestCurrentRecordId; }
            set { _requestCurrentRecordId = value; }
        }

        public int DialogWidth
        {
            get { return _dialogWidth; }
            set { _dialogWidth = value; }
        }

        public int DialogHeight
        {
            get { return _dialogHeight; }
            set { _dialogHeight = value; }
        }

        public bool IsModalDialog
        {
            get { return _isModalDialog; }
            set { _isModalDialog = value; }
        }
    }
}
