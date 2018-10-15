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
using System.Collections.Generic;
using log4net.Filter;
using log4net.Core;

namespace Origam.Server.Logging
{
    public class AndFilter : FilterSkeleton
    {
        private bool _acceptOnMatch;
        private readonly IList<IFilter> _filters = new List<IFilter>();

        public override FilterDecision Decide(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
                throw new ArgumentNullException("loggingEvent");

            foreach (IFilter filter in _filters)
            {
                if (filter.Decide(loggingEvent) != FilterDecision.Accept)
                    return FilterDecision.Neutral; // one of the filter has failed
            }

            // All conditions are true
            if (_acceptOnMatch)
                return FilterDecision.Accept;
            else
                return FilterDecision.Deny;
        }

        public IFilter Filter
        {
            set { _filters.Add(value); }
        }

        public bool AcceptOnMatch
        {
            get { return _acceptOnMatch; }
            set { _acceptOnMatch = value; }
        }
    }
}
