<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
	xmlns:AS="http://schema.advantages.cz/AsapFunctions"
	xmlns:str="http://exslt.org/strings"
    xmlns:math="http://exslt.org/math"
	xmlns:date="http://exslt.org/dates-and-times" exclude-result-prefixes="AS date str math">

	<xsl:variable name="text300" select="'abcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghi'"/>
	<xsl:variable name="text3000" select="'abcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghiabcdefghi'"/>
	
	<xsl:template match="ROOT">
		<ROOT>
			<xsl:for-each select="str:split('a b c d e f',' ')">
				<xsl:variable name="l1" select="."/> 
				<xsl:for-each select="str:split('a b c d e f',' ')">
					<xsl:variable name="l2" select="."/> 
					<xsl:for-each select="str:split('0 1 2 a b c d e f',' ')">
						<xsl:variable name="l3" select="."/> 
							<xsl:for-each select="str:split('9 0 a b c d e f',' ')">
							<xsl:variable name="l4" select="."/> 							
								<xsl:call-template name="WideTable">
									<xsl:with-param name="l1"><xsl:value-of select="$l1"/></xsl:with-param>
									<xsl:with-param name="l2"><xsl:value-of select="$l2"/></xsl:with-param>
									<xsl:with-param name="l3"><xsl:value-of select="$l3"/></xsl:with-param>
									<xsl:with-param name="l4"><xsl:value-of select="$l4"/></xsl:with-param>
								</xsl:call-template>
						</xsl:for-each>
					</xsl:for-each>
				</xsl:for-each>
			</xsl:for-each>
		</ROOT>	
	</xsl:template>

	<xsl:template name="WideTable">
		<xsl:param name="l1"/>
		<xsl:param name="l2"/>
		<xsl:param name="l3"/>
		<xsl:param name="l4"/>
		<WideTable
			Id="{concat($l1,$l2,$l3,$l4,'0000-0000-0000-0000-000000000000')}"
		>
			<xsl:variable name="wideTableColumns">
				<xsl:call-template name="WideTableColumns">
					<xsl:with-param name="l1"><xsl:value-of select="$l1"/></xsl:with-param>
					<xsl:with-param name="l2"><xsl:value-of select="$l2"/></xsl:with-param>
					<xsl:with-param name="l3"><xsl:value-of select="$l3"/></xsl:with-param>
					<xsl:with-param name="l4"><xsl:value-of select="$l4"/></xsl:with-param>
					<xsl:with-param name="extra"><xsl:value-of select="'main'"/></xsl:with-param>
				</xsl:call-template>
			</xsl:variable>
			<xsl:copy-of select="AS:NodeSet($wideTableColumns)/out/@*"/>
		
			<WideTableChild1
				Id="{concat($l1,$l2,$l3,$l4,'0000-0000-0000-0000-000000000001')}"
				refWideTableId="{concat($l1,$l2,$l3,$l4,'0000-0000-0000-0000-000000000000')}"				
			>
				<xsl:variable name="wideTableColumns1">
					<xsl:call-template name="WideTableColumns">
						<xsl:with-param name="l1"><xsl:value-of select="$l1"/></xsl:with-param>
						<xsl:with-param name="l2"><xsl:value-of select="$l2"/></xsl:with-param>
						<xsl:with-param name="l3"><xsl:value-of select="$l3"/></xsl:with-param>
						<xsl:with-param name="l4"><xsl:value-of select="$l4"/></xsl:with-param>
						<xsl:with-param name="extra"><xsl:value-of select="'child1'"/></xsl:with-param>
					</xsl:call-template>
				</xsl:variable>
				<xsl:copy-of select="AS:NodeSet($wideTableColumns)/out/@*"/>
			</WideTableChild1>
			<WideTableChild2
				Id="{concat($l1,$l2,$l3,$l4,'0000-0000-0000-0000-000000000002')}"
				refWideTableId="{concat($l1,$l2,$l3,$l4,'0000-0000-0000-0000-000000000000')}"				
			>
				<xsl:variable name="wideTableColumns2">
					<xsl:call-template name="WideTableColumns">
						<xsl:with-param name="l1"><xsl:value-of select="$l1"/></xsl:with-param>
						<xsl:with-param name="l2"><xsl:value-of select="$l2"/></xsl:with-param>
						<xsl:with-param name="l3"><xsl:value-of select="$l3"/></xsl:with-param>
						<xsl:with-param name="l4"><xsl:value-of select="$l4"/></xsl:with-param>
						<xsl:with-param name="extra"><xsl:value-of select="'child1'"/></xsl:with-param>
					</xsl:call-template>
				</xsl:variable>
				<xsl:copy-of select="AS:NodeSet($wideTableColumns)/out/@*"/>
			</WideTableChild2>
			
		</WideTable>
	</xsl:template>


	<xsl:template name="WideTableColumns">
		<xsl:param name="l1"/>
		<xsl:param name="l2"/>
		<xsl:param name="l3"/>
		<xsl:param name="l4"/>
		<xsl:param name="extra"/>
		
		<out>
			<xsl:choose>
				<xsl:when test="$l4='a'">
					<xsl:attribute name="refWideTableStatusId"><xsl:value-of select="AS:GetConstant('WideTableStatus_Processed')"/></xsl:attribute>
				</xsl:when>
				<xsl:when test="$l3='b'">
					<xsl:attribute name="refWideTableStatusId"><xsl:value-of select="AS:GetConstant('WideTableStatus_New')"/></xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="refWideTableStatusId"><xsl:value-of select="AS:GetConstant('WideTableStatus_Running')"/></xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:attribute name="Int1"><xsl:value-of select="round(math:random()*100000)"/></xsl:attribute>
			<xsl:attribute name="Text3"><xsl:value-of select="concat($extra,'-',$l1,$l2,$l3,$l4,'-text1-', $text300)"/></xsl:attribute>
			<xsl:attribute name="Text4"><xsl:value-of select="concat($extra,'-',$l1,$l2,$l3,$l4,'-text2-', $text300)"/></xsl:attribute>
			<xsl:attribute name="Text5"><xsl:value-of select="concat($extra,'-',$l1,$l2,$l3,$l4,'-text3-', $text300)"/></xsl:attribute>
			<xsl:attribute name="Text6"><xsl:value-of select="concat($extra,'-',$l1,$l2,$l3,$l4,'-text4-', $text300)"/></xsl:attribute>
			<xsl:attribute name="Memo1"><xsl:value-of select="concat($extra,'-memo1-', $text3000)"/></xsl:attribute>
			<xsl:attribute name="Memo2"><xsl:value-of select="concat($extra,'-memo2-', $text3000)"/></xsl:attribute>
			<xsl:attribute name="Memo3"><xsl:value-of select="concat($extra,'-memo3-', $text3000)"/></xsl:attribute>
			<xsl:attribute name="Memo4"><xsl:value-of select="concat($extra,'-memo3-', $text3000)"/></xsl:attribute>
			<xsl:attribute name="Memo5"><xsl:value-of select="concat($extra,'-memo3-', $text3000)"/></xsl:attribute>
			<xsl:attribute name="Memo6"><xsl:value-of select="concat($extra,'-memo3-', $text3000)"/></xsl:attribute>
			<xsl:attribute name="Selected">false</xsl:attribute>
			
			
		</out>		
	</xsl:template>
	
</xsl:stylesheet>