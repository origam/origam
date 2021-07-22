<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<div>
			<xsl:apply-templates select="BusinessPartner"/>
		</div>
	</xsl:template>

	<xsl:template match="BusinessPartner">
		<div>
			<p>Hello <xsl:value-of select="concat(FirstName, ' ', Name)"/></p>
			<p><xsl:value-of select="date:date-time()"/></p>
		</div>
	</xsl:template>
</xsl:stylesheet>