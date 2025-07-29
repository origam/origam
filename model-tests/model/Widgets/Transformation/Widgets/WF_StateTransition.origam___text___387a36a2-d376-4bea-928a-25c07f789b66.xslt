<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<xsl:apply-templates select="StateTransition[@refWorkflowStatesId='bcd46e3a-638d-4d96-ae1b-49005cf9754a']"/>
		</ROOT>
	</xsl:template>

	<xsl:template match="StateTransition">
		<xsl:copy>
			<xsl:copy-of select="@*[name() != 'refWorkflowStateId']"/>
			<xsl:attribute name="refWorkflowStateId"><xsl:value-of select="'82792f50-5232-4096-a953-4ccd66711352'"/></xsl:attribute>
			<xsl:copy-of select="*"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>