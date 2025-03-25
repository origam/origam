<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<xsl:apply-templates select="AllDataTypes"/>
		</ROOT>
	</xsl:template>

	<xsl:template match="AllDataTypes">
		<xsl:copy>
			<xsl:copy-of select="@*[name() != 'Text1']"/>
			<xsl:attribute name="Text1"><xsl:value-of select="concat(@Text1, 'hi')"/></xsl:attribute>
			<xsl:attribute name="Long1"><xsl:value-of select="123456"/></xsl:attribute>
			<xsl:copy-of select="*"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>
