
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ftp_commands_generator.ftp.models
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [Serializable]
    public partial class FtpServerConfiguration
    {
        private string idField;
        private string ipField;
        private string userField;
        private string passwordField;
        private string localDirectoryPathField;
        private string ftpCommandFileName;
        private string ftpFileOutputDirectory;

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string id
        {
            get => idField;
            set => idField = value;
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string IP
        {
            get => ipField;
            set => ipField = value;
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string User
        {
            get => userField;
            set => userField = value;
        }

        /// <remarks/>
        /// The password must be decoded, it is a base 64
        [XmlAttributeAttribute()]
        public string Password
        {
            get
            {
                byte[] data = Convert.FromBase64String(passwordField);
                return Encoding.ASCII.GetString(data); ;
            }
            set => passwordField = value;
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string LocalDirectoryPath
        {
            get => localDirectoryPathField;
            set => localDirectoryPathField = value;
        }


        /// <remarks/>
        [XmlAttributeAttribute()]
        public string FtpCommandFileName
        {
            get => ftpCommandFileName;
            set => ftpCommandFileName = value;
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string FtpFileOutputDirectory
        {
            get => ftpFileOutputDirectory;
            set => ftpFileOutputDirectory = value;
        }
        
    }
}

