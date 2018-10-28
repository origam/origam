<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:functx="http://www.functx.com"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
     xmlns:exsl= "http://exslt.org/common"
    version="1.0">
  <xsl:output method="text" />
  <xsl:template match="/">
    <exsl:document href="doc.xml" indent="yes"> 
    <html>
      <head>
        <title>MD files</title>
      </head>
      <body>
        <xsl:for-each select="/Menu/Menuitem">
              <H2>
              <xsl:value-of select="@DisplayName"/>
            </H2>
            <p>
              <xsl:value-of select="description"/>
            </p>
            <xsl:for-each select="Section">
              <H4>
                <xsl:value-of select="@DisplayName"/>
              </H4>
              <p>
                <xsl:value-of select="description"/>
              </p>
              <xsl:for-each select="Field">
                <p>
                  <xsl:value-of select="@DisplayName"/>
                </p>
                <p>
                  <i>
                    <xsl:value-of select="description"/>
                  </i>
                </p>
              </xsl:for-each>
            </xsl:for-each>
          
        </xsl:for-each>
      </body>
    </html>
      </exsl:document>
  </xsl:template>
</xsl:stylesheet>