<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:c="http://core.com/config" exclude-result-prefixes="c">

  <xsl:template match="/">
    <configuration>
      
      <appSettings>
        <xsl:value-of select="c:ApplicationSettings()" disable-output-escaping="yes" />        
      </appSettings>
      
    </configuration>
  </xsl:template>
  
</xsl:stylesheet>
