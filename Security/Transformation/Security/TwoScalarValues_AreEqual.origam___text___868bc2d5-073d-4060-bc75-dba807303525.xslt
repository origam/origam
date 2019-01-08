<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:param name="First"/>
	<xsl:param name="Second"/>

	<xsl:template match="/">
		<ROOT>
			<value>
			<xsl:choose>
				<xsl:when test="string($First) = string($Second)">
					<xsl:value-of select="'true'"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="'false'"/>
				</xsl:otherwise>
			</xsl:choose>
			</value>
		</ROOT>
	</xsl:template>
</xsl:stylesheet>