<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="c" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:c="http://core.com/config">
  
  <xsl:template match="/">
    <configuration>
      <appSettings>
        <add key="Test" value="Value2" />
        <add key="SolutionFolder" value="{c:Value('Acropolis.SolutionFolder')}" />
      </appSettings>     
      
    </configuration>
  </xsl:template>
  
</xsl:stylesheet>