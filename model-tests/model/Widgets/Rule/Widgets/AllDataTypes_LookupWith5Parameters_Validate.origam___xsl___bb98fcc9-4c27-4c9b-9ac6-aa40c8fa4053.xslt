<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:lookup="http://xsl.origam.com/lookup"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS lookup date">

	<xsl:include href="model://e1c65fcd-118d-4eb3-9c2f-aa27fec132ba"/>
	<xsl:param name="params"/>
	<xsl:variable name="date1Interval"><xsl:value-of select="AS:LookupValue('2c6e598f-dd4c-4980-8d40-ba5ad015f03f',
		/ROOT/AllDataTypes/@Id)"/></xsl:variable>
	
	<xsl:template match="ROOT">
		<RuleExceptionDataCollection xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
			<xsl:apply-templates select="AllDataTypes"/>
		</RuleExceptionDataCollection>
	</xsl:template>

	<xsl:template match="AllDataTypes">
		<xsl:variable name="dateFrom" select="date:date-time()"/>
		<xsl:variable name="dateTo" select="AS:AddMonths(AS:FirstDayNextMonthDate(date:date-time()),-1)"/>
		
		<xsl:variable name="parameters">
		<param>
			<name>AllDataTypes_parDate1From</name>
			<value><xsl:value-of select="AS:AddMonths(AS:FirstDayNextMonthDate($dateFrom),-1)"/></value>
		</param>
		<param>
			<name>AllDataTypes_parDate1To</name>
			<value><xsl:value-of select="AS:AddSeconds(AS:FirstDayNextMonthDate($dateTo), -1)"/></value>
		</param>
		<param>
			<name>AllDataTypes_parWidgetPlainTextTestId</name>
			<value><xsl:value-of select="@refWidgetPlainTextTestId"/></value>
		</param>
		<param>
			<name>AllDataTypes_parTagInputSourceId</name>
			<value><xsl:value-of select="@refTagInputSourceId"/></value>
		</param>
		<param>
			<name>AllDataTypes_parDate1Interval</name>
			<value><xsl:value-of select="$date1Interval"/></value>
		</param>
		</xsl:variable>

		<xsl:variable name="count">
		<xsl:value-of select="lookup:LookupValue('5a9dcccc-4d9e-4801-b744-52677e129f93', $parameters)"/>
		</xsl:variable>

		<xsl:if test="$count > 1">
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName"><xsl:value-of select="''"/></xsl:with-param>
				<xsl:with-param name="EntityName"><xsl:value-of select="''"/></xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="'Error'"/></xsl:with-param>
				<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>