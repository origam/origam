<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template name="Exception">
		<xsl:param name="FieldName"/>
		<xsl:param name="EntityName"/>
		<xsl:param name="Message"/>
		<xsl:param name="Severity"/>
		<xsl:param name="HttpStatusCode" select="400"/>

		<RuleExceptionData>
			<FieldName>
				<xsl:value-of select="$FieldName"/>
			</FieldName>
			<EntityName>
				<xsl:value-of select="$EntityName"/>
			</EntityName>
			<Message>
				<xsl:value-of select="$Message"/>
			</Message>
			<Severity>
				<xsl:value-of select="$Severity"/>
			</Severity>
			<HttpStatusCode>
				<xsl:value-of select="$HttpStatusCode"/>
			</HttpStatusCode>
		</RuleExceptionData>
	</xsl:template>
</xsl:stylesheet>