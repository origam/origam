<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<xsl:apply-templates select="OrigamOnlineUser"/>
		</ROOT>
	</xsl:template>

	<xsl:template match="OrigamOnlineUser">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:attribute name="ClearSessionRequestTimestamp"><xsl:value-of select="date:date-time()"/></xsl:attribute>
			<xsl:copy-of select="*"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>