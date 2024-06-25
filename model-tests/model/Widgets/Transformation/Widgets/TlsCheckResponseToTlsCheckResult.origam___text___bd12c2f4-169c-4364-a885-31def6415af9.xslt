<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" 
	exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<TlsCheckResult Id="{AS:GenerateId()}">
				<xsl:attribute name="Message">
					<xsl:for-each select="./*">
						<xsl:value-of select="AS:NodeToString(.)"/>
						<xsl:value-of select="'&#xa;&#xd;'"/>
					</xsl:for-each>
				</xsl:attribute>
			</TlsCheckResult>
		</ROOT>
	</xsl:template>
</xsl:stylesheet>