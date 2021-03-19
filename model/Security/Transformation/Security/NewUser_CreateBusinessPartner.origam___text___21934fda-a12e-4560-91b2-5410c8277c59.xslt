<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:param name="id" select="AS:GenerateId()"/>
	<xsl:param name="email"/>
	<xsl:param name="password"/>
	<xsl:param name="name"/>
	<xsl:param name="firstName"/>
	<xsl:param name="userName"/>
	<xsl:param name="roleId"/>
	<xsl:param name="emailConfirmed"/>

	<xsl:template match="ROOT">
		<ROOT>			
			<BusinessPartner
				UserEmail="{$email}"
				UserPassword="{$password}"
				ConfirmUserPassword="{$password}">
				<Id><xsl:value-of select="$id"/></Id>
				<UserName><xsl:value-of select="$userName"/></UserName>
				<Name><xsl:value-of select="$name"/></Name>
				<FirstName><xsl:value-of select="$firstName"/></FirstName>
				<xsl:if test="string($roleId)">
					<BusinessPartnerOrigamRole
						Id="{AS:GenerateId()}"
						refOrigamRoleId="{$roleId}"
					/>
				</xsl:if>
				<xsl:if test="string($emailConfirmed)">
					<EmailConfirmed><xsl:value-of select="$emailConfirmed"/></EmailConfirmed>
				</xsl:if>
			</BusinessPartner>
		</ROOT>
	</xsl:template>
	
</xsl:stylesheet>