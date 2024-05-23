<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:param name="boolean"/>

	<xsl:template match="ROOT">
		<ROOT>
			<value>
				<xsl:choose>
					<xsl:when test="$boolean = 'false'">test_true</xsl:when>
					<xsl:otherwise>test_false</xsl:otherwise>
				</xsl:choose>
			</value>
		</ROOT>
	</xsl:template>
</xsl:stylesheet>