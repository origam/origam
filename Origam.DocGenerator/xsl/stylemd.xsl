<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:exsl= "http://exslt.org/common"
    version="1.0">
<xsl:output method="text" />
<xsl:template match="/">
<xsl:for-each select="/Menu/Menuitem">
  <exsl:document href="doc_{@DisplayName}.md">
___
## <xsl:value-of select="@DisplayName"/>

<xsl:if test="description != ''" >
	- *<xsl:value-of select="description"/>*
	</xsl:if>
#### <xsl:for-each select="Section">

  |<xsl:value-of select="@DisplayName"/>|<xsl:if test="description != ''" ><xsl:value-of select="description"/></xsl:if>|
  |-------|--------|
  <xsl:for-each select="Field">| <xsl:value-of select="@DisplayName"/> | <xsl:if test="description != ''" ><xsl:value-of select="description"/> </xsl:if> |
</xsl:for-each>
</xsl:for-each>
</exsl:document>
</xsl:for-each>
</xsl:template> 
</xsl:stylesheet>