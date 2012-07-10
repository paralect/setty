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

.setty file
---------------------
Setty use .setty file to locate settings folder for the current environment.

1.Usually you will need only one .setty for the all solution projects.
 
 Without any special arguments setty.exe will start search for .setty file at the current directory (where *.config.cshtml located). If .setty will not find it will look into the parent folder, therefore if you want to have one .setty for the all projects and you have project structure like below, you can just put .setty under source folder and it will be used to transform all solution configs:

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
             web.config.cshtml
         Project2Folder
             app.config.cshtml
      .setty.config
```

 2.Separate .setty file for particular project

To use separate settings for particular project you can just put .setty near *.config.cshtml and it will be used to locate settings.

 3.Content of the .setty file

Path file can contains FULL or RELATIVE path to the settings folder.
Examples of path file content(based on above project structure):

  * `D:\\MyProject\settings\Stage` 
  * `../settings` (relative path should not starts from slash, because it will be treat as absolute path)


Setty and version of control 
---------------------

The .setty file should not be under your source control system, because in most situations developers and different environments has different paths within .setty and you will continuously merge this file. 
Same apply for any .config file, because setty always regenerate it you need add it to ignore as well.


How to install the Setty project
---------------------

Before run solution you have to install some tools. If you have some of them just skip step.


1. Clone project from github

 `git clone git@github.com:paralect/setty.git`

2. <a href="http://www.microsoft.com/en-us/download/details.aspx?id=17630">Download</a> and install ILMerge. It used to merge some external libraries into single .exe file.

3. <a href="https://github.com/downloads/loresoft/msbuildtasks/MSBuild.Community.Tasks.v1.4.0.42.msi"> Download </a> and install MsBuild community tasks.
Above three steps should be enough to open and compile all solution projects except VS addin project.

4. To open Setty.VsAddin project <a href="http://www.microsoft.com/en-us/download/details.aspx?id=21835">download</a> and install visual studio sdk.
<a href="http://visualstudiogallery.msdn.microsoft.com/e9f40a57-3c9a-4d61-b3ec-1640c59549b3/">Donwload</a> and install VSPackage Builder plugin

5. Restart Visual studio and open solution again. Congratulations! You are done!


Extend Setty with you favorite transformation language
---------------------

Fork setty project, install it first.

To add new transformation language into the project open Setty Project within Setty solution and implement ITransformer interface (put realization and all related files under Engines folder)
This interface has one method `void Transform(String inputFilePath, String outputFilePath, KeyValueConfigurationCollection settings)` 
Transformer accept path to the config file, transform it with key/value settings and save result to the output config file. 
Also in interface you need to specify `ConfigExtention` for a new engine (for example: xslt, cshtml). Setty will automatically choose transformer by ConfigExtention
Once interface implemented add new engine config files names into `SettyConstants.SearchConfigsNames`. 

All externals libs should be merged into the Setty.dll and Setty.exe files. To add them unload Setty project and modify following section at the bottom of .csproj file. After this do the same at Setty.Host project.

``` xml
<Target Name="AfterBuild" Condition="$(Configuration) == 'Publish'">
    <ItemGroup>
      <InputAssemblies Include="$(OutputPath)\Setty.dll" />
      <InputAssemblies Include="$(OutputPath)\Setty.Settings.dll" />
      <InputAssemblies Include="$(OutputPath)\RazorEngine.dll" />
      <InputAssemblies Include="$(OutputPath)\System.Web.Razor.dll" />
    </ItemGroup>
    <Message Text="Merging assemblies..." />
    <MakeDir Directories="$(OutputPath)\Published" />
    <ILMerge TargetPlatformVersion="v4" InputAssemblies="@(InputAssemblies)" OutputFile="$(OutputPath)\Published\Setty.dll" DebugInfo="false" />
</Target>
```

Conratulations! You are done. <a href="https://help.github.com/articles/using-pull-requests/">Send pull request</a> to us and we will merge it in.


Setty and nuget
---------------------

Some nugets automatically can modify your config file, but because of setty regenerate config files you can loose these changes on next build.
So developer should manually copy nuget config changes into setty config file. 

This is not always bad, because some of the nugets can do crazy things with your config and add stuff that you don't like to see in config.


Manual Setty Installation (Based on xslt transformation engine)
---------------------

In your existing project add corresponding configuration template file just near your normal configuration file (App.config or Web.config). Name this template by adding .xslt extension to the file (App.config.xslt or Web.config.xslt). Start with the following template:

``` xml
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="c" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:c="http://setty.org/config">

  <xsl:template match="/">
      <!-- Place your configuration here -->
  </xsl:template>

</xsl:stylesheet>

```

Now just copy full content of App.config to this template. By doing this you will receive the same App.config file after transformation. Integration with build process (via MSBuild) If your project should produce configuration file - then add the following lines to the end of *.csproj file:

``` xml
<Target Name="Setty" BeforeTargets="PreBuildEvent">
  <Exec Command="&quot;$(MSBuildProjectDirectory)\..\setty.exe&quot; /silent" />
</Target>

```

With each build your configuration file will be produced by Setty.exe. 
You should place Setty.exe file in the folder were your *.sln file exists (or choose any location you like and reflect this in MSBuild Exec task definition). You even can register Setty.exe in your PATH environment variable - but in this case your project will depend on system configuration.

BTW: VS 2010 plugin do almost same steps.

  