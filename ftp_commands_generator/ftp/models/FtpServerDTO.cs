using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ftp_commands_generator.ftp.models
{
    /// <summary>
    /// Represents an instance of a ftp server configuration (provided from a config file for example) for the ftp commands file generator.
    /// </summary>
    public class FtpServerDTO
    {
        public DirectoryInfo LocalDirectoryInfo { get; set; }
        public string FtpCommandsFileName { get; set; }
        public string FtpFileOutputDirectory { get; set; }
        public string FtpRootName { get; set; }
        public string IpAddress { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public List<string> AdditionalfoldersField { get; set; }

        public FtpServerDTO(DirectoryInfo directoryinfo)
        {
            LocalDirectoryInfo = directoryinfo;
            FtpRootName = LocalDirectoryInfo.Name;
        }
    }
}