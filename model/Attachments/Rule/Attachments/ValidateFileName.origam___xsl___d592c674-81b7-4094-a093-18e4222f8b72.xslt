<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:include href="model://e1c65fcd-118d-4eb3-9c2f-aa27fec132ba"/>

	<xsl:template match="ROOT">
		<RuleExceptionDataCollection xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
			<xsl:apply-templates select="AttachmentParent/Attachment"/>
		</RuleExceptionDataCollection>
	</xsl:template>

	<xsl:template match="Attachment">
		<xsl:if test="(normalize-space(FileName)='') or (normalize-space(substring-before(FileName, '.'))='')"> 
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName"><xsl:value-of select="''"/></xsl:with-param>
				<xsl:with-param name="EntityName"><xsl:value-of select="''"/></xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="'Filename cannot be empty.'"/></xsl:with-param>
				<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>