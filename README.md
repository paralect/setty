Paralect.Config
====================
<hr />
<p>
  Paralect.Config component solves two common problems - management of key-value settings and management of .NET config files (App.config and Web.config).
</p>

 Settings Folders
---------------------

<table>
  <tr>
    <td> 
      <img src="https://sites.google.com/a/paralect.com/team/paralect-library/paralect-config/Settings.png" alt="example">
    </td>
    <td>
    Settings Folder is a simple concept of hierarchical configuration system well understood by many ASP.NET developers. The more deeper your configuration file is located the more precedence your configuration file has comparing to outer configuration files. This is a simple way to overwrite outer configuration.
    </td>
  </tr>
</table>

<p>
Settings Folder has only one configuration file named App.config. Content of this file is a plain .NET appSettings section. Here is a simple App.config file:
</p>


``` xml
  <?xml version="1.0" encoding="utf-8"?>
  <configuration>
    <appSettings>
      <add key="Acropolis.RackSpaceUserName" value="Seventhman" />
      <add key="Acropolis.RackSpaceKey" value="b84ce1136903f53bcaded6d06c86dcb2" />
      <add key="Acropolis.CacheServer" value="localhost" />
      <add key="Ajeva.Application.RootUri" value="http://localhost:8020" />
      <add key="Ajeva.Application.ContentUri" value="http://localhost:8020/content/static" />
      <add key="Ajeva.Application.LocalFileContainerId" value="4" />
    </appSettings>
   </configuration> 
```
<p>
Because there is no new format of storing application settings, existing project's settings can be simply converted to Settings Folder.
You should organize your Settings Folder by your project needs, but usually you need dedicated profiles for all team members and for all deployment configurations (Stage, Production etc.).
<p>
<p>
Once you created Settings Folder you can reference to concrete settings folder by specifying path to this folder, i.e.:
`C:\Project\Settings\Profiles\DmitrySchetnikovich` (see "Configuring of Settings Folder" below)
Settings Folders are well suited to be a part of your project source files and can be easily placed under your source control system (because this is an ordinary file-system based resource, and not registry-based or server metadata-based). 
</p>
<hr />
 Config Files
---------------------
<p>
.NET config files differ for different deployment configurations. There should be a way to manage config files for different deployment configurations. Paralect.Config chose centralized approach by generating *.config files based on some template language. XSLT is chose for this purpose.
</p>
Config files produced by XSLT templates which don't transform anything but actually used as trivial xml generating language. Reading of this templates is as easy as reading of plain config file. XSLT chose because it is also XML file and thus you will receive the same highlighting and error checking goodness of many xml editors.
Paralect.Config transforms two files - App.config.xslt and Web.config.xslt. Result of transformation will be places to App.config and Web.config (overwriting all existent content!)
Paralect.Config was designed to be used without requirement to reference any Paralect.Config assembles by your project. Thats mean that Paralect.Config can be used as built-time and management-time component.
Paralect.Config was designed to be used both by web applications and desktop applications.
Paralect.Config was designed to work well with any existing config management solutions, like Visual Studio Config Transformation, Web Deployment Project etc.. It's normal to use both Paralect.Config and Visual Studio Config Transformation. However, with Paralect.Config it is possible to change .NET config files without the needs to build or deploy project (thus it is not only build- or deploy-time component, it is also management-time component)


Example
---------------------

This is how your config template can look:

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
<hr />

All application settings in one line
---------------------

To print all application settings in the common `<appSettings />` format, use this line:


``` xml
<configuration>
  <appSettings>
    <add key="AdditionaProperty1" value="Value1" />
    <add key="AdditionaProperty2" value="Value2" />
    ...
    <xsl:value-of select="c:ApplicationSettings()" disable-output-escaping="yes" />
  </appSettings>
  ...
</configuration>
</code>
```
<p>

Configuring of Settings Folder via Setting Path file (`.setty.config` or `.core.config`)
---------------------

To run Paralect.Config.exe without any arguments you need to configure location to your Settings Folder for particular App.config.xslt or Web.config.xslt in Path File.
</p>
 1.One path file for the all solution projects
 
 Without any arguments Paralect.Config will start search for Path File at the current directory (where *.config.xslt located) if Path File will not found it will look into the parent folder, therefore if you want to have one Path File for the all projects and you have project structure like below, you can just put Path File under source folder and it will be used to transform all solution configs :

``` text 
D:\\MyProject\
     settings
      App.config
         Stage
             App.config
         Production
             App.config

     source                
         Project1Folder
             web.config.xslt
         Project2Folder
             app.config.xslt
      .setty.config
```

 2.Separate path file for particular project

To use separate settings for particular project you can just put Path File near *.config.xslt and it will be used to locate settings.

 3.Content of the path file

Path file can contains FULL or RELATIVE path to the settings folder.
Examples of path file content(based on above project structure):

  * `D:\\MyProject\settings\Stage` 
  * ../settings (relative path should not starts from slash, because it will be treat as absolute path)

 4.Source control and Path File:
The file path should not be under your source control system, because in most situations developers has different paths and you will continuously merge this file.


Quick Start
---------------------

In your existing project add corresponding configuration template file just near your normal configuration file (App.config or Web.config). Name this template by adding .xslt extension to the file (App.config.xslt or Web.config.xslt).
Start with the following template:


``` xml
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="c" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:c="http://paralect.com/config">

  <xsl:template match="/">
      <!-- Place your configuration here -->
  </xsl:template>

</xsl:stylesheet>
```


Now just copy full content of App.config to this template. By doing this you will receive the same App.config file after transformation.
Integration with build process (via MSBuild)
If your project should produce configuration file - then add the following lines to the end of *.csproj file:
  
   ``` xml
<Target Name="ParalectConfig" BeforeTargets="PreBuildEvent">
  <Exec Command="&quot;$(MSBuildProjectDirectory)\..\Paralect.Config.exe&quot; /silent" />
</Target>
   ```
With each build your configuration file will be produced by Paralect.Config.exe. You should place Paralect.Config.exe file in the folder were your *.sln file exists (or choose any location you like and reflect this in MSBuild Exec task definition). You even can register Paralect.Config.exe in your PATH environment variable - but in this case your project will depend on system configuration.


Usage
---------------------
You can run it without any parameters. In this case you need to configure location of Settings Folder in the `.setty.config` files. 

Command line arguments:  
 <table>
   <tr>
    <td>    
      Argument
    </td>
     <td>    
       Description
    </td>
    </tr>
  <tr>
    <td>    
      `/context:path`   
    </td>
     <td>    
       Transform all configs in this path. By default this is a system current folder.
    </td>
    </tr>
      <tr>      
    <td>    
      `/settings:path`
    </td>
     <td>    
      Do not use .setty.config files to determine Settings Folder location. Use direct path to Settings Folder for all transformations.
    </td>
    </tr>
          <tr>
    <td>    
       `/silent`
    </td>
     <td>    
   Do not block UI (useful for running this component in background)
    </td>
    </tr>
 </table>
  