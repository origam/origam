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

using System;
using System.Web;

namespace Origam.Server
{
    public class AdaptivePropertyProvider
    {
        private readonly string _propertyName;
        private readonly object _propertyValue;

        #region Constructor(s)

        protected internal AdaptivePropertyProvider(string propertyName, object propertyValue)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            _propertyName = propertyName;
            _propertyValue = propertyValue;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[GetPropertyName()] = propertyValue;
            }
        }

        #endregion

        #region Overrides of object

        public override string ToString()
        {
            if (HttpContext.Current != null)
            {
                object item = HttpContext.Current.Items[GetPropertyName()];

                return item != null ? item.ToString() : null;
            }

            if (!ReferenceEquals(_propertyValue, null))
            {
                return _propertyValue.ToString();
            }

            return null;
        }

        #endregion

        #region Private helper methods

        private string GetPropertyName()
        {
            return string.Format("{0}{1}", Analytics.PropertyNamePrefix, _propertyName);
        }

        #endregion
    }
}

