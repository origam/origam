<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<xsl:variable name="params">
				<parameter value="test1" quantity="50"/>
				<parameter value="test2" quantity="50"/>
				<parameter value="nul" quantity="500"/>
			</xsl:variable>
			
			<xsl:for-each select="AS:RandomlyDistributeValues($params)/values/value">
				<value><xsl:value-of select="."/></value>
			</xsl:for-each>
		</ROOT>
	</xsl:template>
</xsl:stylesheet>