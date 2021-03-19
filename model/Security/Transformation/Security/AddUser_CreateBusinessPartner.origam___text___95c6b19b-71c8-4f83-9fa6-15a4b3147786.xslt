<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:param name="UserRoleId"/>
	<xsl:param name="FirstName"/>
	<xsl:param name="Name"/>
	<xsl:param name="UserName"/>

	<xsl:template match="ROOT">
		<ROOT>
			<BusinessPartner>
				<Id><xsl:value-of select="AS:GenerateId()"/></Id>
				<FirstName><xsl:value-of select="$FirstName"/></FirstName>
				<Name><xsl:value-of select="$Name"/></Name>
				<UserName><xsl:value-of select="$UserName"/></UserName>
				
				<BusinessPartnerOrigamRole Id="{AS:GenerateId()}" refOrigamRoleId="{$UserRoleId}"/>
				
				<Resource>
					<xsl:if test="AS:GetConstant('Authorization_NewUser_DefaultOrganizationId') != AS:GetConstant('_guid_empty')">
						<xsl:attribute name="refOrganizationId"><xsl:value-of select="AS:GetConstant('Authorization_NewUser_DefaultOrganizationId')"/></xsl:attribute>
					</xsl:if>
					<Id><xsl:value-of select="AS:GenerateId()"/></Id>
					<Name><xsl:value-of select="concat($FirstName, ' ', $Name)"/></Name>
					<xsl:if test="AS:GetConstant('Authorization_NewUser_DefaultBusinessUnitId') != AS:GetConstant('_guid_empty')">
						<refBusinessUnitId><xsl:value-of select="AS:GetConstant('Authorization_NewUser_DefaultBusinessUnitId')"/></refBusinessUnitId>
					</xsl:if>
					<refResourceTypeId><xsl:value-of select="AS:GetConstant('ResourceType_Human')"/></refResourceTypeId>
				</Resource>
			</BusinessPartner>
		</ROOT>
	</xsl:template>
</xsl:stylesheet>