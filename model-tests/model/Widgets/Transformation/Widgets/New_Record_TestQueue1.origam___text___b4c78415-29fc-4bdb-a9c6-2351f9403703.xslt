<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:template match="ROOT">
		<ROOT>
			<WorkQueueEntry	Id="{AS:GenerateId()}" 
							refWorkQueueId="751BD582-2604-4259-B560-6AFB8A772FCA"
							
			/>
		</ROOT>
	</xsl:template>
</xsl:stylesheet>