<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="/">
		<iterations>
			<xsl:for-each select="AS:ListDays('2008-10-01', '2008-10-03')/list/item">
				<iteration index="{position()}"/>
			</xsl:for-each>
		</iterations>
	</xsl:template>
</xsl:stylesheet>