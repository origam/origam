<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:param name="tagInputSourceId"/>
	<xsl:param name="rowId"/>
	
	<xsl:template match="ROOT">
		<ROOT>
			<xsl:apply-templates select="AllDataTypes"/>
		</ROOT>
	</xsl:template>

	<xsl:template match="AllDataTypes">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:attribute name="Boolean1">
				<xsl:value-of select="AS:LookupValue('1e4b33fc-57c5-44d9-b1b8-06fbc88b5ceb', $rowId)"/>
			</xsl:attribute>
			<xsl:attribute name="refTagInputSourceId">
				<xsl:value-of select="$tagInputSourceId"/>
			</xsl:attribute>
			<xsl:copy-of select="*"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>