<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date">

	<xsl:param name="attachment"/>
	<xsl:param name="attachmentName"/>
	<xsl:param name="attachmentList"/>
	<xsl:param name="subject"/>
	<xsl:param name="body"/>
	<xsl:param name="senderEmail" />
	<xsl:param name="senderName" />
	<xsl:param name="recipientEmail"/>
	<xsl:param name="recipientCCEmail"/>
	<xsl:param name="recipientBCCEmail"/>

	<xsl:template match="ROOT">
		<xsl:variable name="sender">
			<xsl:choose>
				<xsl:when test="string($senderName)">
					<xsl:value-of select="concat($senderName, ' &lt;', $senderEmail, '&gt;')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$senderEmail"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		
		<xsl:if test="not(string($recipientEmail))">
			<xsl:message terminate="yes"><xsl:value-of select="AS:GetString('RecipientEmailIsEmpty')"/></xsl:message>
		</xsl:if>
		
		<ROOT>
			<Mail 
				Id="{AS:GenerateId()}"
				Subject="{$subject}"
				DateSent="{date:date-time()}"
				CC="{$recipientCCEmail}"
				BCC="{$recipientBCCEmail}"
				>
				
				<Sender><xsl:value-of select="$sender"/></Sender>
				<Recipient><xsl:value-of select="$recipientEmail"/></Recipient>
				<MessageBody><xsl:value-of select="$body"/></MessageBody>
				
				<xsl:if test="string($attachment)">
					<MailAttachment>
						<Id><xsl:value-of select="AS:GenerateId()"/></Id>
						<Data><xsl:value-of select="$attachment"/></Data>
						<FileName><xsl:value-of select="$attachmentName"/></FileName>
					</MailAttachment>
				</xsl:if>
				
				<xsl:for-each select="$attachmentList/ROOT/Attachment">
					<MailAttachment>
						<Id><xsl:value-of select="AS:GenerateId()"/></Id>
						<Data><xsl:value-of select="Data"/></Data>
						<FileName><xsl:value-of select="FileName"/></FileName>
					</MailAttachment>
				</xsl:for-each>				
			</Mail>
		</ROOT>
	</xsl:template>
</xsl:stylesheet>