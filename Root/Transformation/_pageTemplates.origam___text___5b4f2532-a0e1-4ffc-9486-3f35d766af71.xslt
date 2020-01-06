<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:origam="http://schema.advantages.cz/origam"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date origam">

	<xsl:template match="origam:page">
		<html>
			<head>
				<title><xsl:value-of select="@title"/></title>
			</head>
			<body>
				<xsl:apply-templates/>
			</body>
		</html>
	</xsl:template>
	
	<xsl:template match="origam:grid">
		<table>
			<tr>
				<xsl:apply-templates select="origam:gridColumn" mode="header"/>
			</tr>
			<xsl:element name="xsl:for-each">
				<xsl:attribute name="select"><xsl:value-of select="@entity"/></xsl:attribute>
				<tr>
					<xsl:apply-templates select="origam:gridColumn" mode="row"/>
				</tr>
			</xsl:element>
		</table>
	</xsl:template>

	<xsl:template match="origam:gridColumn" mode="header">
		<th>
			<xsl:value-of select="@title"/>
		</th>
	</xsl:template>

	<xsl:template match="origam:gridColumn" mode="row">
		<td>
			<xsl:call-template name="renderField"/>
		</td>
	</xsl:template>

	<xsl:template match="*">
		<xsl:choose>
			<xsl:when test="count(@*) = 0 and count (*) = 0 and string(.) = ''">
				<xsl:copy/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy>
					<xsl:copy-of select="@*"/>
					<xsl:apply-templates/>
				</xsl:copy>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="origam:form">
		<xsl:element name="xsl:for-each">
			<xsl:attribute name="select"><xsl:value-of select="@entity"/></xsl:attribute>
			<table>
				<xsl:apply-templates/>
			</table>
		</xsl:element>
	</xsl:template>

	<xsl:template match="origam:formField">
		<tr>
			<th align="left">
				<xsl:value-of select="@title"/>
			</th>
			<td>
				<xsl:call-template name="renderField"/>
			</td>
		</tr>
	</xsl:template>

	<xsl:template name="renderField">
		<xsl:choose>
			<xsl:when test="string(@customTemplate)">
				<xsl:element name="xsl:call-template">
					<xsl:attribute name="name"><xsl:value-of select="@customTemplate"/></xsl:attribute>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="@mode = 'edit'">
						<xsl:choose>
							<xsl:when test="widget = 'dropDown'">
								
							</xsl:when>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<xsl:element name="xsl:value-of">
							<xsl:attribute name="select"><xsl:value-of select="@value"/></xsl:attribute>
						</xsl:element>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

<!--	
	<xsl:template name="renderDropDown">
		<xsl:element name="xsl:variable">
			<xsl:attribute name="select"><xsl:value-of select="concat('AS:LookupList(', @lookupId, ')/list)"/></xsl:attribute>
		</xsl:element>
		
		<xsl:if test="count($list/item) > 0">
			<select name="@name">
				<xsl:for-each select="$list/item">
					<option value="{@Id}">
						<xsl:if test="@Id = $currentLeadQualityId">
							<xsl:attribute name="selected">selected</xsl:attribute>
						</xsl:if>
						<xsl:value-of select="@Name"/>
					</option>
				</xsl:for-each>
			</select>
		</xsl:if>
	</xsl:template>
-->
</xsl:stylesheet>