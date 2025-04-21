<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<xsl:apply-templates select="WorkQueue"/>
		</ROOT>
	</xsl:template>

	<xsl:template match="WorkQueue">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:choose>
				<xsl:when test="@EnableThrottling = 'false'">
					<xsl:attribute name="ThrottlingIntervalSeconds">
						<xsl:value-of select="AS:GetConstant('_Number_0')"/>
					</xsl:attribute>
					<xsl:attribute name="ThrottlingItemsPerInterval">
						<xsl:value-of select="AS:GetConstant('_Number_0')"/>
					</xsl:attribute>
				</xsl:when>
				<xsl:when test="@EnableThrottling = 'true'">
					<xsl:attribute name="ThrottlingIntervalSeconds">
						<xsl:value-of select="AS:GetConstant('_Number_60')"/>
					</xsl:attribute>
					<xsl:attribute name="ThrottlingItemsPerInterval">
						<xsl:value-of select="AS:GetConstant('_Number_10')"/>
					</xsl:attribute>
				</xsl:when>
			</xsl:choose>
			<xsl:copy-of select="*"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>