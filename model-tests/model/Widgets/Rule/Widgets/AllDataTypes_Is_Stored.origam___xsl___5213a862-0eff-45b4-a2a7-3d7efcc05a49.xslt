<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:include href="model://e1c65fcd-118d-4eb3-9c2f-aa27fec132ba"/>

	<xsl:template match="ROOT">
		<RuleExceptionDataCollection xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
			<xsl:if test="not(string(AS:LookupValue('4b72f11a-4513-4e19-a8f5-addf40a277b1', AllDataTypes/@Id)))">
				<xsl:call-template name="Exception">
					<xsl:with-param name="FieldName"/>
					<xsl:with-param name="EntityName"/>
					<xsl:with-param name="Message">Not Valid</xsl:with-param>
					<xsl:with-param name="Severity">High</xsl:with-param>
				</xsl:call-template>
			</xsl:if>
		</RuleExceptionDataCollection>
	</xsl:template>
</xsl:stylesheet>