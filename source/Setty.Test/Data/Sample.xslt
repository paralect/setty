<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:c="http://core.com/config" exclude-result-prefixes="c">

  <xsl:template match="/">
    <configuration>

      <configSections>
        <!-- log4net section -->
        <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
        <section name="MsmqTransportConfig" type="NServiceBus.Config.MsmqTransportConfig, NServiceBus.Core" />
        <section name="UnicastBusConfig" type="NServiceBus.Config.UnicastBusConfig, NServiceBus.Core" />
        <section name="membase" type="Membase.Configuration.MembaseClientSection, Membase" />
      </configSections>

      <appSettings file="bin\Web.user.config">
        <add key="ConfigurationId" value="Acropolis.Local" />
        <add key="Name" value="{c:Value('Name')}" />
        <add key="Email" value="{c:Value('Email')}" />

        <xsl:if test="c:Value('Company') = 'IBM'">
          <add key="Company" value="{c:Value('Company')}" />
        </xsl:if>

      </appSettings>

      <MsmqTransportConfig InputQueue="Acropolis.Client.AjevaApplication" ErrorQueue="Acropolis.Client.AjevaApplication.Errors" NumberOfWorkerThreads="1" MaxRetries="5" />

      <UnicastBusConfig>
        <MessageEndpointMappings>
          <add Messages="Acropolis.CommandServer.Commands" Endpoint="Acropolis.CommandServer" />
        </MessageEndpointMappings>
      </UnicastBusConfig>

      <membase>
        <servers bucket="default" userName="AppFabricUser" password="qwerty">
          <add uri="http://127.0.0.1:8091/pools/default" />
        </servers>
      </membase>

      <!-- ~~~~~~~~~~~~~~~~~~~~~ -->
      <!-- log4net configuration -->
      <!-- ~~~~~~~~~~~~~~~~~~~~~ -->

      <log4net>
        <appender name="InfoAppender" type="log4net.Appender.RollingFileAppender">
          <file value="c:\Logs\Ajeva.Application\Info\Info.log" />
          <filter type="log4net.Filter.LevelRangeFilter">
            <levelMin value="DEBUG" />
            <levelMax value="WARN" />
          </filter>
          <appendToFile value="true" />
          <rollingStyle value="Date" />
          <datePattern value=".yyyy-MM-dd" />
          <layout type="log4net.Layout.PatternLayout">
            <conversionPattern value="%d %-5p %c - %m%n" />
          </layout>
        </appender>

        <appender name="SqlAppender" type="log4net.Appender.RollingFileAppender">
          <file value="c:\Logs\Ajeva.Application\SQL\Info.log.sql" />
          <appendToFile value="true" />
          <rollingStyle value="Date" />
          <datePattern value=".yyyy-MM-dd" />
          <layout type="log4net.Layout.PatternLayout">
            <conversionPattern value="%d %-5p %c - %m%n" />
          </layout>
        </appender>

        <appender name="ErrorAppender" type="log4net.Appender.RollingFileAppender">
          <file value="c:\Logs\Ajeva.Application\Errors\Error.log" />
          <filter type="log4net.Filter.LevelRangeFilter">
            <levelMin value="ERROR" />
            <levelMax value="FATAL" />
          </filter>
          <appendToFile value="true" />
          <rollingStyle value="Date" />
          <datePattern value=".yyyy-MM-dd" />
          <layout type="log4net.Layout.PatternLayout">
            <conversionPattern value="%d %-5p %c - %m%n" />
          </layout>
        </appender>

        <root>
          <level value="ALL" />
          <appender-ref ref="InfoAppender" />
          <appender-ref ref="ErrorAppender" />
        </root>
        <logger name="Acropolis.Data.ExtendedAcropolisDataContext">
          <level value="ALL" />
          <appender-ref ref="SqlAppender" />
        </logger>
      </log4net>


      <connectionStrings>
        <add name="ApplicationServices" connectionString="data source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|aspnetdb.mdf;User Instance=true" providerName="System.Data.SqlClient" />
        <add name="AcropolisDataContext" connectionString="metadata=res://*/Acropolis.csdl|res://*/Acropolis.ssdl|res://*/Acropolis.msl;provider=System.Data.SqlClient;provider connection string=&quot;Data Source=PC9-PC\SQLEXPRESS;Initial Catalog=Acropolis;Persist Security Info=True;User ID=sa;Password=geetaA1;MultipleActiveResultSets=True&quot;" providerName="System.Data.EntityClient" />
      </connectionStrings>

      <system.web>

        <!--
    <sessionState mode="Custom" customProvider="AppFabricCacheSessionStoreProvider">
      <providers>
        <add
          name="AppFabricCacheSessionStoreProvider"
          type="Microsoft.ApplicationServer.Caching.DataCacheSessionStoreProvider"
          cacheName="AjevaSession"
          sharedId="Ajeva"/>
      </providers>
    </sessionState> -->


        <!-- Local server session state -->
        <!--<sessionState mode="SQLServer" useHostingIdentity="true" sqlConnectionString="data source=PC9-PC\SQLEXPRESS;user Id=sa;pwd=geetaA1;Application Name=Ajeva" />-->
        <sessionState mode="SQLServer" useHostingIdentity="true" sqlConnectionString="Data Source=ORSICH-PC;user Id=sa;pwd=123;Application Name=Ajeva" />

        <!-- Stage server session state -->
        <!--
    <sessionState mode="SQLServer" useHostingIdentity="true" sqlConnectionString="data source=173.203.226.250,1433;user Id=sa;pwd=devstagewD6r24NbN;Application Name=Ajeva" />
    -->

        <httpRuntime maxRequestLength="1550000" maxQueryStringLength="10000" requestValidationMode="2.0" requestPathInvalidCharacters="" />
        <compilation debug="true" targetFramework="4.0">
          <assemblies>
            <add assembly="System.Web.Abstractions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" />
            <add assembly="System.Web.Routing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" />
            <add assembly="System.Web.Mvc, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" />
            <add assembly="System.Data.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
            <add assembly="Microsoft.ApplicationServer.Caching.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" />
            <add assembly="Microsoft.ApplicationServer.Caching.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" />
          </assemblies>
        </compilation>
        <authentication mode="Forms">
          <forms loginUrl="~/Account/LogOn" timeout="2880" />
        </authentication>
        <membership>
          <providers>
            <clear />
            <add name="AspNetSqlMembershipProvider" type="System.Web.Security.SqlMembershipProvider" connectionStringName="ApplicationServices" enablePasswordRetrieval="false" enablePasswordReset="true" requiresQuestionAndAnswer="false" requiresUniqueEmail="false" maxInvalidPasswordAttempts="5" minRequiredPasswordLength="6" minRequiredNonalphanumericCharacters="0" passwordAttemptWindow="10" applicationName="/" />
          </providers>
        </membership>
        <profile>
          <providers>
            <clear />
            <add name="AspNetSqlProfileProvider" type="System.Web.Profile.SqlProfileProvider" connectionStringName="ApplicationServices" applicationName="/" />
          </providers>
        </profile>
        <roleManager enabled="false">
          <providers>
            <clear />
            <add name="AspNetSqlRoleProvider" type="System.Web.Security.SqlRoleProvider" connectionStringName="ApplicationServices" applicationName="/" />
            <add name="AspNetWindowsTokenRoleProvider" type="System.Web.Security.WindowsTokenRoleProvider" applicationName="/" />
          </providers>
        </roleManager>
        <pages>
          <namespaces>
            <add namespace="System.Web.Mvc" />
            <add namespace="System.Web.Mvc.Ajax" />
            <add namespace="System.Web.Mvc.Html" />
            <add namespace="System.Web.Routing" />
          </namespaces>
        </pages>

        <!--
		<customErrors mode="Off">
			<error statusCode="403" redirect="/403" />
			<error statusCode="404" redirect="/404" />
			<error statusCode="500" redirect="/500" />
		</customErrors> -->


      </system.web>
      <system.webServer>
        <security>
          <requestFiltering allowDoubleEscaping="true">
            <requestLimits maxAllowedContentLength="100000000" />
          </requestFiltering>
        </security>
        <validation validateIntegratedModeConfiguration="false" />

        <rewrite>
          <rules>

            <!--<rule name="my" stopProcessing="true">
					<match url="^my/billing(.*)$" />
					<action type="Rewrite" url="mybilling{R:1}" />
				</rule>-->

          </rules>
        </rewrite>

        <modules runAllManagedModulesForAllRequests="true">
          <add name="ErrorLoggingModule" type="Ajeva.Application.Infrastructure.Modules.ErrorLoggingModule, Ajeva.Application" />
          <add name="ErrorHandlerModule" type="Ajeva.Application.Infrastructure.HttpModules.ErrorHandlerModule, Ajeva.Application" />
          <add name="SwfUploadSupportModule" type="Ajeva.Application.Infrastructure.Modules.SwfUploadSupportModule, Ajeva.Application" />
          <add name="SessionModule" type="Ajeva.Application.Infrastructure.Modules.SessionModule, Ajeva.Application" />
          <!--<add name="CompressionModule" type="Ajeva.Application.Infrastructure.HttpModules.CompressionModule, Ajeva.Application" />-->
        </modules>

        <handlers>
          <remove name="MvcHttpHandler" />
          <add name="MvcHttpHandler" preCondition="integratedMode" verb="*" path="*.mvc" type="System.Web.Mvc.MvcHttpHandler" />
        </handlers>

        <httpCompression>
          <scheme name="gzip" dll="%Windir%\system32\inetsrv\gzip.dll" />
          <dynamicTypes>
            <add mimeType="text/*" enabled="true" />
            <add mimeType="message/*" enabled="true" />
            <add mimeType="application/*" enabled="true" />
            <add mimeType="*/*" enabled="true" />
            <add mimeType="application/json" enabled="true" />
          </dynamicTypes>
          <staticTypes>
            <add mimeType="text/*" enabled="true" />
            <add mimeType="message/*" enabled="true" />
            <add mimeType="application/*" enabled="true" />
            <add mimeType="*/*" enabled="false" />
          </staticTypes>

        </httpCompression>

        <urlCompression doStaticCompression="true" doDynamicCompression="true" />
        <caching enabled="true" enableKernelCache="true">
          <profiles>
            <add extension=".gif" policy="DontCache" kernelCachePolicy="CacheUntilChange" duration="0.00:01:00" location="Any" />
            <add extension=".png" policy="DontCache" kernelCachePolicy="CacheUntilChange" duration="0.00:01:00" location="Any" />
            <add extension=".js" policy="DontCache" kernelCachePolicy="CacheUntilChange" duration="0.00:01:00" location="Any" />
            <add extension=".css" policy="DontCache" kernelCachePolicy="CacheUntilChange" duration="0.00:01:00" location="Any" />
            <add extension=".jpg" policy="DontCache" kernelCachePolicy="CacheUntilChange" duration="0.00:01:00" location="Any" />
            <add extension=".jpeg" policy="DontCache" kernelCachePolicy="CacheUntilChange" duration="0.00:01:00" location="Any" />
          </profiles>
        </caching>

        <staticContent>
          <!--        <remove fileExtension=".js" />
        <mimeMap fileExtension=".js" mimeType="text/javascript" /> -->
          <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="30.00:00:00" />
        </staticContent>

      </system.webServer>
      <runtime>
        <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
          <dependentAssembly>
            <assemblyIdentity name="System.Web.Mvc" publicKeyToken="31bf3856ad364e35" />
            <bindingRedirect oldVersion="1.0.0.0" newVersion="2.0.0.0" />
          </dependentAssembly>
        </assemblyBinding>
      </runtime>
      <system.data>
        <DbProviderFactories>
          <add name="EF Caching Data Provider" invariant="EFCachingProvider" description="Caching Provider Wrapper" type="EFCachingProvider.EFCachingProviderFactory, EFCachingProvider, Version=1.0.0.0, Culture=neutral, PublicKeyToken=def642f226e0e59b" />
          <add name="EF Tracing Data Provider" invariant="EFTracingProvider" description="Tracing Provider Wrapper" type="EFTracingProvider.EFTracingProviderFactory, EFTracingProvider, Version=1.0.0.0, Culture=neutral, PublicKeyToken=def642f226e0e59b" />
          <add name="EF Generic Provider Wrapper" invariant="EFProviderWrapper" description="Generic Provider Wrapper" type="EFProviderWrapperToolkit.EFProviderWrapperFactory, EFProviderWrapperToolkit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=def642f226e0e59b" />
        </DbProviderFactories>
      </system.data>

    </configuration>

  </xsl:template>

</xsl:stylesheet>
