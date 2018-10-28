<?xml version="1.0" encoding="utf-8"?>
<html xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
      xsl:version="1.0">
  <head>
    <title>MD files</title>
  </head>
  <body>
    <xsl:for-each select="/Menu/Menuitem">
	 <H2> <xsl:value-of select="@DisplayName"/></H2>
		<p><xsl:value-of select="description"/></p>
			<xsl:for-each select="Section">
				<H4><xsl:value-of select="@DisplayName"/></H4>
					<p><xsl:value-of select="description"/></p>
						<xsl:for-each select="Field">
							<p><xsl:value-of select="@DisplayName"/></p>
								<p><i><xsl:value-of select="description"/></i></p>
						</xsl:for-each>
			</xsl:for-each>
    </xsl:for-each>
  </body>
</html>