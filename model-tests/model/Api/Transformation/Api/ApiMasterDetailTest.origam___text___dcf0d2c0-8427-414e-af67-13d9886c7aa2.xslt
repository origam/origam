<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<ApiMainEntity Id="2fa57b1c-314f-4595-b556-f5e1848fcd76" s1="1s1" s2="1s2">
				<ApiDetailEntity refApiMainEntityId="2fa57b1c-314f-4595-b556-f5e1848fcd76" i1="1" i2="2"/>
			</ApiMainEntity>
			<ApiMainEntity Id="761bf13c-a0f9-4f51-ba19-02cbe2af3d96" s1="2s1" s2="2s2">
				<ApiDetailEntity refApiMainEntityId="761bf13c-a0f9-4f51-ba19-02cbe2af3d96" i1="21" i2="22"/>
			</ApiMainEntity>
			
		</ROOT>
	</xsl:template>
</xsl:stylesheet>