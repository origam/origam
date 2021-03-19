<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:param name="startDate"/>
	<xsl:param name="endDate"/>

	<xsl:template match="ROOT">
		<ROOT>
			<xsl:apply-templates select="AS:ListDays($startDate, $endDate)"/>
		</ROOT>
	</xsl:template>
	
	<xsl:template match="item">
		<date>
			<xsl:value-of select="."/>
		</date>
	</xsl:template>
</xsl:stylesheet>