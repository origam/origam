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
using Origam.DA.ObjectPersistence;
using Origam.Rule.Xslt;
using Origam.Schema.EntityModel;
using Origam.Schema;

namespace Origam.Rule;
public class AsTransform
{
    public static IXsltEngine GetXsltEngine(
        XsltEngineType xsltEngineType, IPersistenceProvider persistence=null)
    {
        switch(xsltEngineType)
        {
            case XsltEngineType.XslTransform:
                return new OldXsltEngine(persistence);
            case XsltEngineType.XslCompiledTransform:
                return new CompiledXsltEngine(persistence);
            default:
                throw new Exception("Unknown XsltEngine type.");
        }
    }
    public static IXsltEngine GetXsltEngine(
        IPersistenceProvider persistence, Guid transformationId)
    {
        AbstractSchemaItem transformation = persistence.RetrieveInstance(
            typeof(AbstractSchemaItem), 
            new ModelElementKey(transformationId))
            as AbstractSchemaItem;
        if(transformation is XslTransformation)
        {
            return GetXsltEngine(
                (transformation as XslTransformation).XsltEngineType,
                persistence);
        }
        else
        {
            return GetXsltEngine(
                XsltEngineType.XslTransform,
                persistence);
        }
    }
}
