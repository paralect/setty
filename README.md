Overview
---------------------
Setty was designed to help manage project key/value settings. .NET config files differ for different deployment configurations. There should be a way to manage config files for different deployment configurations. Setty chose centralized approach by generating *.config files based on some template language. Currently there is support for razor and xslt transform engines. 

Setty with razor syntax require .net 4.0+. .net 2.0 version currently support only xslt transformation engine and has to be installed manually. See documentation below for details

Installation guide
---------------------
The best way to getting started with Setty is download plugin for visual studio 2010. And install setty in one click.
Plugin will configure everything for you.

1. Open Visual Studio and navigate to Tools -> Extentions Manager. 
2. Search in Online Gallery for work 'Setty' and double click to install (reload Visual Studio after this). Or alternatively download extension [Setty VS 2010 addin](https://github.com/downloads/paralect/setty/Setty.VsAddin.vsix) 
![add vs extension](http://paralect.github.com/setty/images/add_vs_extention.png)
3. Create new project (or open existing one), right click on it. In context menu navigate to Add -> Add Setty... 
![add vs extension](http://paralect.github.com/setty/images/images/add_setty.png)
4. Plugin ask you where to create global settings folder. Usually this folder is on top of solution folder. 
![add vs extension](http://paralect.github.com/setty/images/browse_settings_folder.png)
5. Congratulations! You are done. Now you can start use Setty.

Quick start
---------------------
By default setty use <a href="http://www.microsoft.com/web/category/razor">Razor syntax</a> to transform setty config (Web.config.cshtml on above screen). 
  Settings Folder is a simple concept of hierarchical configuration system well understood by many ASP.NET developers. The more deeper your configuration file is located the more precedence your configuration file has comparing to outer configuration files. This is a simple way to overwrite outer configuration. Here is example from real world:

![example](http://paralect.github.com/setty/images/settings_folder.png)

To try it out navigate to the settings folder which you've specified on 4th step. There you'll find App.config file. This file is a simple storage of key/value settings. Settings Folder has only one configuration file named App.config. Content of this file is a plain .NET appSettings section. 

Open it and add new setting:
``` xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
  	<add key="hello" value="Hello World"/>
  </appSettings>
</configuration>
```
After this open your setty config file (In my case it's Web.config.cshtml) and use setting as follow:
``` xml
<configuration>
  <appSettings>
    <add key="MyFirstSetting" value="@Model["hello"]" />
    ...
```
Rebuild project and open Web.config file, there should be following result:
``` xml
<configuration>
  <appSettings>
    <add key="MyFirstSetting" value="Hello World" />
    ...
```

Real world Setty config files
---------------------

Below example show not only how to use Setty, but also flexibility that Setty provide by adding support
of variables, conditions, whatever transformation language support.

Razor transformation engine:

``` xml
<?xml version="1.0" encoding="utf-8"?>
    <configuration>    

      <appSettings>
        <add key="Name" value="@Model["Name"]" />
        <add key="Email" value="@Model["Email"]" />
      </appSettings> 

      <system.web>
        <sessionState mode="SQLServer" sqlConnectionString="@Model["StateServer"]" />
      </system.web> 

      <compilation debug="@Model["Debug"]" targetFramework="4.0" /> 

      @if(Model["Email"] == "Compress")
       {
        <httpCompression>
          <scheme name="gzip" dll="%Windir%\system32\inetsrv\gzip.dll" />
          <dynamicTypes>
            <add mimeType="text/*" enabled="true" />
            ...
          </dynamicTypes>
        </httpCompression> 
       }     
          ...

    </configuration>

      
```

Xslt transformation engine:


``` xml
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:c="http://core.com/config"> 

  <xsl:template match="/">
    <configuration>    

      <appSettings>
        <add key="Name" value="{c:Value('Name')}" />
        <add key="Email" value="{c:Value('Email')}" />
      </appSettings> 

      <system.web>
        <sessionState mode="SQLServer" sqlConnectionString="{c:Value('StateServer')}" />
      </system.web> 

      <compilation debug="{c:Value('Debug')}" targetFramework="4.0" /> 

      <xsl:if test="c:Value('Compress') = 'true'">

        <httpCompression>
          <scheme name="gzip" dll="%Windir%\system32\inetsrv\gzip.dll" />
          <dynamicTypes>
            <add mimeType="text/*" enabled="true" />
            ...
          </dynamicTypes>
        </httpCompression>

      </xsl:if> 

      ...

    </configuration>

  </xsl:template> 

</xsl:stylesheet>

```

Read <a href="http://paralect.github.com/setty">more</a> documentation on a Setty site.

  