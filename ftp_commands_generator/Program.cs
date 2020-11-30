using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ftp_commands_generator.ftp;
using ftp_commands_generator.ftp.models;
using ftp_commands_generator.ftpconfig;
using System.Linq;

namespace ftp_commands_generator
{
    /// <summary>
    /// The program generates an ftp command file (.ftp extension) that when executed by an ftp server
    /// it copies recursively the specified directory to the ftp server.
    /// </summary>
    internal class Program
    {
        private static readonly string ftpConfigurationXmlFilePath = @"ftpconfig/ftpServerConfig.xml";
        private static readonly string ftpConfigurationXmlNodesPath = "/ftpserverconfigurations/ftpserverconfiguration";
        private static XmlDocument ftpConfigurationXml = new XmlDocument();

        private static void Main(string[] args)
        {
            Console.WriteLine($"Reading ftp server parameters");

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ftpConfigurationXmlFilePath);
            ftpConfigurationXml.Load(path);
            var ftpconfigurationnode = fetchFtpConfiguration(ftpConfigurationXml, args);
            if (ftpconfigurationnode != null)
            {
                var additionalfolders = readAdditionalFolders(ftpconfigurationnode).ToList();

                var ftpserverdeserializedconfiguration = XmlSerializerMethods.DeserializeObject<FtpServerConfiguration>(ftpconfigurationnode);
                Console.WriteLine($"Configuration: {ftpserverdeserializedconfiguration.id} with IP Address: {ftpserverdeserializedconfiguration.IP}");

                var ftpserver = new FtpServerDTO(new DirectoryInfo(ftpserverdeserializedconfiguration.LocalDirectoryPath))
                {
                    IpAddress = ftpserverdeserializedconfiguration.IP,
                    User = ftpserverdeserializedconfiguration.User,
                    Password = ftpserverdeserializedconfiguration.Password,
                    FtpCommandsFileName = ftpserverdeserializedconfiguration.FtpCommandFileName,
                    FtpFileOutputDirectory = ftpserverdeserializedconfiguration.FtpFileOutputDirectory,
                    AdditionalfoldersField = additionalfolders,
                };

                FtpCommandsGenerator ftpCommandsGenerator = new FtpCommandsGenerator(ftpserver);

                Console.WriteLine($"Generating ftp command file: {ftpserver.FtpCommandsFileName}");

                ftpCommandsGenerator.GenerateFtpCommandsFile();
                Console.WriteLine(Environment.NewLine);
            }
            else
            {
                var ftpconfigutationid = args?.FirstOrDefault() ?? "No id provided";
                Console.WriteLine($"Could not find Ftp configuration with id: {ftpconfigutationid}, check the ftpServerConfig.xml");
            }
        }

        private static IEnumerable<string> readAdditionalFolders(XmlNode ftpxmlconfigurationNode)
        {
            var additionalfoldernodes = ftpxmlconfigurationNode?.FirstChild;
            foreach (XmlNode node in additionalfoldernodes)
            {
                yield return node.Attributes["Path"].Value;
            }
        }

        private static XmlNode fetchFtpConfiguration(XmlDocument ftpxmlconfiguration, string[] args)
        {
            XmlNode fptconfigurationnode;

            if (args?.Length > 0)
            {
                fptconfigurationnode = ftpxmlconfiguration.GetElementById(args[0]);
            }
            else
            {
                fptconfigurationnode = ftpConfigurationXml.SelectSingleNode(ftpConfigurationXmlNodesPath);
            }

            return fptconfigurationnode;
        }
    }
}