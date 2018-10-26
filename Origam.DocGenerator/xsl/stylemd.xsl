<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:functx="http://www.functx.com"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    version="2.0">
<xsl:output method="text" />
<xsl:template match="/">
<xsl:for-each select="/Menu/Menuitem">
___
## <xsl:value-of select="@DisplayName"/>

<xsl:if test="description != ''" >
	- *<xsl:value-of select="description"/>*
	</xsl:if>
#### <xsl:for-each select="Section">

<xsl:value-of select="@DisplayName"/>
<xsl:if test="description != ''" >
	- *<xsl:value-of select="description"/>*
</xsl:if>
|||
|-------|--------|
<xsl:for-each select="Field">| <xsl:value-of select="@DisplayName"/> | <xsl:if test="description != ''" ><xsl:value-of select="description"/> </xsl:if> |
</xsl:for-each>
</xsl:for-each>
</xsl:for-each>

  </xsl:template> 
</xsl:stylesheet>