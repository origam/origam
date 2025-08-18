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
using System.IO;
using System.Xml;

using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;
using Origam.Workbench.Services;

namespace Origam.Rule;
/// <summary>
/// Summary description for ModelXmlResolver.
/// </summary>
public class ModelXmlResolver : XmlResolver
{
	public ModelXmlResolver() : base()
	{
	}
	public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
	{ 
//			if(absoluteUri.IsFile)
//			{
//				System.Xml.XmlUrlResolver x = new XmlUrlResolver();
//				return x.GetEntity(absoluteUri, role, ofObjectToReturn);
//			}
//			else
//			{
		Guid g = new Guid(absoluteUri.Authority.ToString());
		IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		ISchemaItem item = persistence.SchemaProvider.RetrieveInstance(typeof(XslTransformation), new ModelElementKey(g)) as ISchemaItem;
		string xsl;
		if(item is XslTransformation)
		{
			xsl = (item as XslTransformation).TextStore;
		}
		else if(item is XslRule)
		{
			xsl = (item as XslRule).Xsl;
		}
		else
		{
			throw new ArgumentOutOfRangeException("absoluteUri", absoluteUri, ResourceUtils.GetString("ErrorNoXslReference"));
		}
		MemoryStream ms = new MemoryStream();
		StreamWriter sw = new StreamWriter(ms);
		sw.Write(xsl);
		sw.Flush();
		ms.Position = 0;
		return ms;
//			}
	}
	public override Uri ResolveUri(Uri baseUri, string relativeUri)
	{
		return base.ResolveUri (baseUri, relativeUri);
	}
	public override System.Net.ICredentials Credentials
	{
		set
		{
		}
	}
}
