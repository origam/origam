<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:include href="model://e1c65fcd-118d-4eb3-9c2f-aa27fec132ba"/>

	<xsl:template match="ROOT">
		<RuleExceptionDataCollection xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
			<xsl:apply-templates select="BusinessPartner"/>
		</RuleExceptionDataCollection>
	</xsl:template>

	<xsl:template match="BusinessPartner">
		<xsl:if test="string(@OldPassword) = ''">
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName">OldPassword</xsl:with-param>
				<xsl:with-param name="EntityName">BusinessPartner</xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="AS:GetString('OldPasswordIsMandatory')"/></xsl:with-param>
				<xsl:with-param name="Severity">High</xsl:with-param>
			</xsl:call-template>		
		</xsl:if>
		<xsl:if test="string(@UserPassword) = ''">
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName">Password</xsl:with-param>
				<xsl:with-param name="EntityName">BusinessPartner</xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="AS:GetString('PasswordIsMandatory')"/></xsl:with-param>
				<xsl:with-param name="Severity">High</xsl:with-param>
			</xsl:call-template>		
		</xsl:if>
		<xsl:if test="string(@UserPassword) != string(@ConfirmUserPassword)">
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName">ConfirmUserPassword</xsl:with-param>
				<xsl:with-param name="EntityName">BusinessPartner</xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="AS:GetString('PasswordsDoesntMatch')"/></xsl:with-param>
				<xsl:with-param name="Severity">High</xsl:with-param>
			</xsl:call-template>
		</xsl:if>						
	</xsl:template>

</xsl:stylesheet>