<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:param name="splitXPath"/>
	<xsl:param name="targetQueue"/>

	<xsl:template match="ROOT">
		<xsl:variable name="target" select="AS:LookupValue('930ae1c9-0267-4c8d-b637-6988745fd44c', $targetQueue)"/>
		
		<xsl:if test="not(string($target))">
			<xsl:message terminate="yes"><xsl:value-of select="concat('Queue not found! ', $targetQueue)"/></xsl:message>
		</xsl:if>
		
		<ROOT>
			<xsl:apply-templates select="WorkQueueEntry">
				<xsl:with-param name="targetQueueId" select="$target"/>
			</xsl:apply-templates>
		</ROOT>
	</xsl:template>
	
	<xsl:template match="WorkQueueEntry">
		<xsl:param name="targetQueueId"/>

		<xsl:variable name="fileName" select="@FileName"/>
		<xsl:variable name="refId" select="@refId"/>
		
		<xsl:for-each select="AS:ToXml(Data, $splitXPath)">
			<WorkQueueEntry
				Id="{AS:GenerateId()}"
				FileName="{concat($fileName, ' [', position(), ']')}"
				refId="{$refId}"
				refWorkQueueId="{$targetQueueId}"
				>
				<Data>
					<xsl:value-of select="AS:NodeToString(.)"/>
				</Data>
			</WorkQueueEntry>
		</xsl:for-each>
	</xsl:template>
</xsl:stylesheet>