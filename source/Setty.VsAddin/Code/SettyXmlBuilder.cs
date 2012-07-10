using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Setty.Settings;
using System.IO;

namespace Setty.VsAddin.Code
{
    public class SettyXmlBuilder
    {
        private const string VsProjectXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";


        //<Target Name="Setty" BeforeTargets="PreBuildEvent">
        //  <Exec Command="&amp;quot;$(MSBuildProjectDirectory)\..\setty.exe&amp;quot; /silent" />
        //</Target>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="relativePathToSetty"></param>
        /// <returns></returns>
        public XmlElement GetSettyProjectXml(XmlDocument xmlDoc, string relativePathToSetty)
        {
            var targetElem = xmlDoc.CreateElement("Target", VsProjectXmlNamespace);
            var nameAttr = xmlDoc.CreateAttribute("Name");
            nameAttr.Value = "Setty";
            targetElem.Attributes.Append(nameAttr);
            var beforeAttr = xmlDoc.CreateAttribute("BeforeTargets");
            beforeAttr.Value = "PreBuildEvent";
            targetElem.Attributes.Append(beforeAttr);

            var execElem = xmlDoc.CreateElement("Exec", VsProjectXmlNamespace);
            var commandAttr = xmlDoc.CreateAttribute("Command");
            commandAttr.InnerXml = String.Format(@"&quot;$(MSBuildProjectDirectory)\{0}&quot; /silent", relativePathToSetty);
            execElem.Attributes.Append(commandAttr);
            targetElem.AppendChild(execElem);

            return targetElem;
        }

        /// <summary>
        /// Add xml (to the project file) that automatically run setty.exe after each build
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="pathToSetty"></param>
        /// <param name="pathToSettySettings"></param>
        public void AddSettyPostBuildPartIntoProjectFile(string projectName, string pathToSetty, string pathToSettySettings)
        {
            var relativePathToSetty = PathHelper.MakeRelativePath(projectName, pathToSetty);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(projectName);

            var firstChild = xmlDoc.DocumentElement.FirstChild;
            var nameAttribute = firstChild.Attributes["Name"];
            var isSettyAdded = nameAttribute != null && nameAttribute.Value == "Setty";
            if (!isSettyAdded)
            {
                xmlDoc.DocumentElement.PrependChild(GetSettyProjectXml(xmlDoc, relativePathToSetty));
                xmlDoc.Save(projectName);
            }
        }

        /// <summary>
        /// Include generated config into project, also make *.config 'DependentUpon' setty config
        /// </summary>
        /// <param name="activeProjectPath">path to the project file</param>
        /// <param name="originalConfigFileName"></param>
        /// <param name="settyFileName"></param>
        public void IncludeConfigIntoProjFile(string activeProjectPath, string originalConfigFileName, string settyFileName)
        {
            XmlDocument projDoc;
            projDoc = new XmlDocument();
            projDoc.Load(activeProjectPath);
            var nsmgr = new XmlNamespaceManager(projDoc.NameTable);
            nsmgr.AddNamespace("ns", VsProjectXmlNamespace);
            var config = projDoc.DocumentElement.SelectSingleNode(String.Format("//*[@Include='{0}']", originalConfigFileName), nsmgr);
            if (config != null)
            {
                config.ParentNode.RemoveChild(config);
            }

            var xml = CreateConfigXmlOfProject(projDoc, originalConfigFileName, settyFileName);
            projDoc.DocumentElement.AppendChild(xml);
            projDoc.Save(activeProjectPath);
        }


        /// <summary>
        /// Generate xml to include generated config into project, also make *.config 'DependentUpon' setty config
        /// Produce xml like this:
        /// <ItemGroup>
        ///    <Content Include='App.config.xslt' />
        ///    <Content Include='App.config'>
        ///       <DependentUpon>App.config.xslt</DependentUpon>
        ///    </Content>
        ///  </ItemGroup>
        /// </summary>
        /// <param name="xmlDoc">Project file XmlDocument</param>
        /// <param name="originalConfigFileName">Original config file name</param>
        /// <param name="settyFileName">Setty config file name</param>
        /// <returns></returns>
        public XmlElement CreateConfigXmlOfProject(XmlDocument xmlDoc, string originalConfigFileName, string settyFileName)
        {
            var itemGroup = xmlDoc.CreateElement("ItemGroup", VsProjectXmlNamespace);
            var contentItem = xmlDoc.CreateElement("Content", VsProjectXmlNamespace);
            var includeAttr = xmlDoc.CreateAttribute("Include");
            includeAttr.Value = settyFileName;
            contentItem.Attributes.Append(includeAttr);
            itemGroup.AppendChild(contentItem);


            contentItem = xmlDoc.CreateElement("Content", VsProjectXmlNamespace);
            includeAttr = xmlDoc.CreateAttribute("Include");
            includeAttr.Value = originalConfigFileName;
            contentItem.Attributes.Append(includeAttr);
            itemGroup.AppendChild(contentItem);

            var dependent = xmlDoc.CreateElement("DependentUpon", VsProjectXmlNamespace);
            dependent.InnerText = settyFileName;
            contentItem.AppendChild(dependent);

            return itemGroup;
        }

        /// <summary>
        /// Wrap config xml with xslt when generate setty xslt config from *.config
        /// </summary>
        /// <param name="originalConfig"></param>
        /// <param name="settyConfig"></param>
        public void WrapSettyConfigWithXslt(string originalConfig, string settyConfig)
        {
            var xslt =
                @"<?xml version='1.0' encoding='utf-8'?>
                                        <xsl:stylesheet version='1.0' exclude-result-prefixes='c'
                                           xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
                                           xmlns:c='http://setty.org/config'>
                                            <xsl:template match='/'>
                                            </xsl:template>
                                        </xsl:stylesheet>";
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(originalConfig);

            var xsltDoc = new XmlDocument();
            xsltDoc.LoadXml(xslt);
            var node = xsltDoc.DocumentElement.ChildNodes[0];

            //necessary for crossing XmlDocument contexts
            XmlNode importNode = xsltDoc.ImportNode(xmlDoc.DocumentElement, true);

            node.AppendChild(importNode);
            xsltDoc.Save(settyConfig);
        }

        public void CreateEmptySettingsFile(string pathToSettings)
        {
            var pathToFile = Path.Combine(pathToSettings, "App.config");

            if (!File.Exists(pathToFile))
            {
                var xml =
                    @"<?xml version='1.0' encoding='utf-8' ?>
                    <configuration>
                        <appSettings>
                        </appSettings>
                    </configuration>";
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                xmlDoc.Save(pathToFile);
            }
        }
    }
}
