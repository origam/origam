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

using System.Collections;
using System.Linq;
using Origam.Schema.EntityModel;

namespace Origam.DA.Service
{
	/// <summary>
	/// Summary description for CustomParameterService.
	/// </summary>
	public static class CustomParameterService
	{
		private static ArrayList _customParameters = new ArrayList();
		private static bool _isInitialized = false;

		private static ArrayList CustomParameters
		{
			get
			{
				if(! _isInitialized)
				{
					Initialize();
				}

				return _customParameters;
			}
		}

		public static ICustomParameter MatchParameter(string parameterName)
		{
			foreach(ICustomParameter customParameter in CustomParameterService.CustomParameters)
			{
				if(parameterName.EndsWith(customParameter.Name))
				{
					return customParameter;
				}
			}

			return null;
		}

	    public static string GetFirstNonCustomParameter(DataStructureMethod method)
	    {
            if (method == null)
            {
                return null;
            }
            else
            {
                return method.ParameterReferences.Keys
                    .Cast<string>()
                    .FirstOrDefault(parameterName => MatchParameter(parameterName) == null);
            }
	    }


        private static void Initialize()
		{
			_customParameters.Add(new CustomParameters.CurrentDateCustomParameter());
			_customParameters.Add(new CustomParameters.CurrentDateLastMinuteCustomParameter());
			_customParameters.Add(new CustomParameters.CurrentDateTimeCustomParameter());
			_customParameters.Add(new CustomParameters.CurrentUserBusinessUnitIdParameter());
			_customParameters.Add(new CustomParameters.CurrentUserIdParameter());
			_customParameters.Add(new CustomParameters.CurrentUserOrganizationIdParameter());
			_customParameters.Add(new CustomParameters.CurrentUILanguageCustomParameter());
			_customParameters.Add(new CustomParameters.CurrentUserResourceIdCustomParameter());

			_isInitialized = true;
		}
	}
}
