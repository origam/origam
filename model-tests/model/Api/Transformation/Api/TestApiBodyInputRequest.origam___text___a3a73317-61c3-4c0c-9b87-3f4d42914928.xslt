<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<value>
{
"ROOT":{
	"Test":[
			{
			"Id":"cd5dc0e5-0bc2-44e9-ad87-3674130f8ffb",
			"dateAttribute":"2020-09-21T11:58:22",
			"stringAttribute":"ORIGAM attribute",
			"stringElement":"ORIGAM element"
			}
		]
	}
}
			</value>
		</ROOT>
	</xsl:template>
</xsl:stylesheet>