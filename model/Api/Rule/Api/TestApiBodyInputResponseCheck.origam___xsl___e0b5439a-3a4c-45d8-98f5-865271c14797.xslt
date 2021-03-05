<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:include href="model://e1c65fcd-118d-4eb3-9c2f-aa27fec132ba"/>

	<xsl:template match="/">
		<RuleExceptionDataCollection xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
			<xsl:if test="not(ROOT/Test/@Id = 'cd5dc0e5-0bc2-44e9-ad87-3674130f8ffb')">
				<xsl:call-template name="Exception">
					<xsl:with-param name="FieldName"><xsl:value-of select="''"/></xsl:with-param>
					<xsl:with-param name="EntityName"><xsl:value-of select="''"/></xsl:with-param>
					<xsl:with-param name="Message"><xsl:value-of select="'/ROOT/Test/@Id was expected to be cd5dc0e5-0bc2-44e9-ad87-3674130f8ffb'"/></xsl:with-param>
					<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
				</xsl:call-template>
			</xsl:if>
			<xsl:if test="not(ROOT/Test/@dateAttribute = '2020-09-21T11:58:22+00:00')">
				<xsl:call-template name="Exception">
					<xsl:with-param name="FieldName"><xsl:value-of select="''"/></xsl:with-param>
					<xsl:with-param name="EntityName"><xsl:value-of select="''"/></xsl:with-param>
					<xsl:with-param name="Message"><xsl:value-of select="'/ROOT/Test/@dateAttribute was expected to be 2020-09-21T11:58:22+00:00'"/></xsl:with-param>
					<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
				</xsl:call-template>
			</xsl:if>
			<xsl:if test="not(ROOT/Test/@stringAttribute = 'ORIGAM attribute')">
				<xsl:call-template name="Exception">
					<xsl:with-param name="FieldName"><xsl:value-of select="''"/></xsl:with-param>
					<xsl:with-param name="EntityName"><xsl:value-of select="''"/></xsl:with-param>
					<xsl:with-param name="Message"><xsl:value-of select="'/ROOT/Test/@stringAttribute was expected to be ORIGAM attribute'"/></xsl:with-param>
					<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
				</xsl:call-template>
			</xsl:if>
			<xsl:if test="not(ROOT/Test/stringElement = 'ORIGAM element')">
				<xsl:call-template name="Exception">
					<xsl:with-param name="FieldName"><xsl:value-of select="''"/></xsl:with-param>
					<xsl:with-param name="EntityName"><xsl:value-of select="''"/></xsl:with-param>
					<xsl:with-param name="Message"><xsl:value-of select="'/ROOT/Test/stringElement was expected to be ORIGAM element'"/></xsl:with-param>
					<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
				</xsl:call-template>
			</xsl:if>
		</RuleExceptionDataCollection>
	</xsl:template>
</xsl:stylesheet>