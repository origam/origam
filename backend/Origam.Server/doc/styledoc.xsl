<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:html="http://www.w3.org/1999/xhtml"
    exclude-result-prefixes="html xsl"
    version="1.0">
 <xsl:output omit-xml-declaration="yes" indent="yes"/>
<xsl:template match="/">
<xsl:for-each select="/Menu/Section">
   <html:h2>
    Section <xsl:value-of select="@DisplayName"/>
    </html:h2>
  <xsl:if test="entity/entityid != ''" >
    <html:p>
      Entity
      <html:a href="doc.aspx?section=content&amp;filterType=elementId&amp;filterValue={entity/entityid}&amp;objectType=entity">
       <html:img src="assets/doc/model0.png" /><xsl:value-of select="entity/entityname"/>
        </html:a>
    </html:p>
  </xsl:if>
  <xsl:for-each select="Field">
    <html:div class="screenField">
      <html:div class="title">
        <xsl:value-of select="@DisplayName"/>
      </html:div>
      <html:div class="description" >
        <xsl:value-of select="description"/>
      </html:div>
     </html:div>
 </xsl:for-each>
</xsl:for-each>
</xsl:template> 
</xsl:stylesheet>