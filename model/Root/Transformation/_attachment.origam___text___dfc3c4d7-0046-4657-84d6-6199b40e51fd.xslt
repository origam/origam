<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">
	
	<xsl:param name="attachment"/>
	<xsl:param name="entityId"/>
	<xsl:param name="fileName"/>
	<xsl:param name="recordId"/>
	<xsl:param name="remark"/>

	<xsl:template match="ROOT">
		<ROOT>
			<Attachment>
				<Id><xsl:value-of select="AS:GenerateId()"/></Id>
				<refParentRecordEntityId><xsl:value-of select="$entityId"/></refParentRecordEntityId>
				<refParentRecordId><xsl:value-of select="$recordId"/></refParentRecordId>
				<FileName><xsl:value-of select="$fileName"/></FileName>
				<Note><xsl:value-of select="$remark"/></Note>
				<Data><xsl:value-of select="$attachment"/></Data>
			</Attachment>
		</ROOT>
	</xsl:template>
</xsl:stylesheet>