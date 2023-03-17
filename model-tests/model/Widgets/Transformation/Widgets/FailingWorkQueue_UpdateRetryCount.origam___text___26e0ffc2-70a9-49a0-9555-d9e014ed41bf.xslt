<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<xsl:apply-templates select="WorkQueueEntry"/>
		</ROOT>
	</xsl:template>

	<xsl:template match="WorkQueueEntry">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:if test="@refWorkQueueId = '0ab10c2f-386e-4dd1-992e-5e3765a28447'">
				<xsl:attribute name="Number1">
					<xsl:value-of select="AS:iif(not(@Number1), 1, AS:Plus(@Number1, 1))"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@refWorkQueueId = '8527e8c1-d480-4b12-81a6-f3858b37dc73'">
				<xsl:attribute name="Number2">
					<xsl:value-of select="AS:iif(not(@Number2), 1, AS:Plus(@Number2, 1))"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:copy-of select="*"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>