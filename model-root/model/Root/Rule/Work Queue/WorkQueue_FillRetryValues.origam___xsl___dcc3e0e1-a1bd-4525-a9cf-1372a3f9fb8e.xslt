<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<xsl:apply-templates select="WorkQueue"/>
		</ROOT>
	</xsl:template>

	<xsl:template match="WorkQueue">
		<xsl:choose>
			<xsl:when test="@refWorkQueueRetryTypeId = AS:GetConstant('WorkQueueRetryType_NoRetry')">
				<xsl:attribute name="MaxRetries">
					<xsl:value-of select="AS:GetConstant('_Number_0')"/>
				</xsl:attribute>
				<xsl:attribute name="RetryIntervalSeconds">
					<xsl:value-of select="AS:GetConstant('_Number_0')"/>
				</xsl:attribute>
				<xsl:attribute name="ExponentialRetryBase">
					<xsl:value-of select="AS:GetConstant('_Number_0')"/>
				</xsl:attribute>
			</xsl:when>
			<xsl:when test="@refWorkQueueRetryTypeId = AS:GetConstant('WorkQueueRetryType_LinearRetry')">
				<xsl:attribute name="MaxRetries">
					<xsl:value-of select="AS:iif(@MaxRetries != AS:GetConstant('_Number_0'), @MaxRetries,
					AS:GetConstant('_Number_3'))"/>
				</xsl:attribute>
				<xsl:attribute name="RetryIntervalSeconds">
					<xsl:value-of select="AS:iif(@RetryIntervalSeconds != AS:GetConstant('_Number_0'),
					@RetryIntervalSeconds,
					AS:GetConstant('_Number_30'))"/>
				</xsl:attribute>
				<xsl:attribute name="ExponentialRetryBase">
					<xsl:value-of select="AS:GetConstant('_Number_0')"/>
				</xsl:attribute>
			</xsl:when>
			<xsl:when test="@refWorkQueueRetryTypeId = AS:GetConstant('WorkQueueRetryType_ExponentialRetry')">
				<xsl:attribute name="MaxRetries">
					<xsl:value-of select="AS:iif(@MaxRetries != AS:GetConstant('_Number_0'), @MaxRetries,
					AS:GetConstant('_Number_3'))"/>
				</xsl:attribute>
				<xsl:attribute name="RetryIntervalSeconds">
					<xsl:value-of select="AS:iif(@RetryIntervalSeconds != AS:GetConstant('_Number_0'),
					@RetryIntervalSeconds,
					AS:GetConstant('_Number_30'))"/>
				</xsl:attribute>
				<xsl:attribute name="ExponentialRetryBase">
					<xsl:value-of select="AS:iif(@ExponentialRetryBase != AS:GetConstant('_Number_0'),
					@ExponentialRetryBase,
					AS:GetConstant('_Number_2'))"/>
				</xsl:attribute>
			</xsl:when>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>