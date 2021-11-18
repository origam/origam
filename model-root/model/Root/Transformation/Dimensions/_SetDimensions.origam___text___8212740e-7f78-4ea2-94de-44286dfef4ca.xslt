<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template name="SetDimensions">
		<xsl:param name="feature1"/>
		<xsl:param name="feature2"/>
		<xsl:param name="feature3"/>
		<xsl:param name="feature4"/>
		<xsl:param name="dimension1"/>
		<xsl:param name="dimension2"/>
		<xsl:param name="dimension3"/>
		<xsl:param name="dimension4"/>

		<xsl:if test="AS:IsFeatureOn($feature1)">
			<xsl:attribute name="refDimension1Id"><xsl:value-of select="$dimension1"/></xsl:attribute>
		</xsl:if>
		<xsl:if test="AS:IsFeatureOn($feature2)">
			<xsl:attribute name="refDimension2Id"><xsl:value-of select="$dimension2"/></xsl:attribute>
		</xsl:if>
		<xsl:if test="AS:IsFeatureOn($feature3)">
			<xsl:attribute name="refDimension3Id"><xsl:value-of select="$dimension3"/></xsl:attribute>
		</xsl:if>
		<xsl:if test="AS:IsFeatureOn($feature4)">
			<xsl:attribute name="refDimension4Id"><xsl:value-of select="$dimension4"/></xsl:attribute>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>