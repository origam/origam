#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;

namespace Origam.Server.OpenApi;

public class OpenApiDocumentProvider
{
    public const string DocumentName = "api";
    private readonly Lazy<string> document;

    public OpenApiDocumentProvider(ISwaggerProvider swaggerProvider)
    {
        document = new Lazy<string>(
            () => Serialize(swaggerProvider.GetSwagger(DocumentName)),
            isThreadSafe: true
        );
    }

    public string GetDocument()
    {
        return document.Value;
    }

    private static string Serialize(OpenApiDocument openApiDocument)
    {
        using var stringWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(stringWriter);
        openApiDocument.SerializeAsV3(writer);
        return stringWriter.ToString();
    }
}
