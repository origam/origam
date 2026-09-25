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

namespace Origam.Architect.Server.Services.Xslt;

// Mirrors the templates of Origam.Workbench.Editors.XslEditor.
public static class XslTemplates
{
    public const string Transformation = """
        <?xml version="1.0" encoding="UTF-8"?>
        <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
        	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
        	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

        	<xsl:template match="ROOT">
        		<ROOT>
        			<xsl:apply-templates select=""/>
        		</ROOT>
        	</xsl:template>

        	<xsl:template match="">
        		<xsl:copy>
        			<xsl:copy-of select="@*"/>
        			<xsl:attribute name=""><xsl:value-of select=""/></xsl:attribute>
        			<xsl:copy-of select="*"/>
        		</xsl:copy>
        	</xsl:template>
        </xsl:stylesheet>
        """;

    public const string Rule = """
        <?xml version="1.0" encoding="UTF-8"?>
        <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
        	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
        	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

        	<xsl:include href="model://e1c65fcd-118d-4eb3-9c2f-aa27fec132ba"/>

        	<xsl:template match="ROOT">
        		<RuleExceptionDataCollection xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
        			<xsl:apply-templates select=""/>
        		</RuleExceptionDataCollection>
        	</xsl:template>

        	<xsl:template match="">
        		<xsl:if test="">
        			<xsl:call-template name="Exception">
        				<xsl:with-param name="FieldName"><xsl:value-of select="''"/></xsl:with-param>
        				<xsl:with-param name="EntityName"><xsl:value-of select="''"/></xsl:with-param>
        				<xsl:with-param name="Message"><xsl:value-of select="''"/></xsl:with-param>
        				<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
        			</xsl:call-template>
        		</xsl:if>
        	</xsl:template>

        </xsl:stylesheet>
        """;
}
