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
	<xsl:if test="string(@UserEmail) = ''">
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName"><xsl:value-of select="'UserEmail'"/></xsl:with-param>
				<xsl:with-param name="EntityName"><xsl:value-of select="'BusinessPartner'"/></xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="AS:GetString('EmailIsMandatory')"/></xsl:with-param>
				<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
			</xsl:call-template>		
		</xsl:if>
		<xsl:if test="string(UserName) = ''">
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName"><xsl:value-of select="'UserMame'"/></xsl:with-param>
				<xsl:with-param name="EntityName"><xsl:value-of select="'BusinessPartner'"/></xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="AS:GetString('UsernameIsMandatory')"/></xsl:with-param>
				<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
			</xsl:call-template>		
		</xsl:if>
		<xsl:if test="string(@UserPassword) = ''">
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName"><xsl:value-of select="'Password'"/></xsl:with-param>
				<xsl:with-param name="EntityName"><xsl:value-of select="'BusinessPartner'"/></xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="AS:GetString('PasswordIsMandatory')"/></xsl:with-param>
				<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
			</xsl:call-template>		
		</xsl:if>
		<xsl:if test="string(UserName) != '' and string(AS:LookupValue('a427e92f-943e-4ba0-9db7-ca5d42190aaf', UserName)) != ''">
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName"><xsl:value-of select="'UserName'"/></xsl:with-param>
				<xsl:with-param name="EntityName"><xsl:value-of select="'BusinessPartner'"/></xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="AS:GetString('UsernameAlreadyExists')"/></xsl:with-param>
				<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
			</xsl:call-template>
		</xsl:if>
		<xsl:if test="string(@UserPassword) != string(@ConfirmUserPassword)">
			<xsl:call-template name="Exception">
				<xsl:with-param name="FieldName"><xsl:value-of select="'ConfirmUserPassword'"/></xsl:with-param>
				<xsl:with-param name="EntityName"><xsl:value-of select="'BusinessPartner'"/></xsl:with-param>
				<xsl:with-param name="Message"><xsl:value-of select="AS:GetString('PasswordsDoesntMatch')"/></xsl:with-param>
				<xsl:with-param name="Severity"><xsl:value-of select="'High'"/></xsl:with-param>
			</xsl:call-template>
		</xsl:if>						
	</xsl:template>

</xsl:stylesheet>