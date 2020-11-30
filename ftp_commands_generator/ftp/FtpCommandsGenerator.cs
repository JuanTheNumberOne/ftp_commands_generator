using System;
using System.Text;
using System.ComponentModel;
using System.IO;
using FtpExtensionMethods;
using ftp_commands_generator.ftp.models;
using System.Collections.Generic;

namespace ftp_commands_generator.ftp
{
    public class FtpCommandsGenerator
    {
        /// <summary>
        /// Enum used to write the ftp commands. The description attribute is what
        /// is written to the ftp command file.
        /// </summary>
        private enum FtpCommands
        {
            [Description("open")]
            OpenConnection = 0,

            [Description("mkdir")]
            CreateDirectory = 1,

            [Description("rmdir")]
            DeleteDirectory = 2,

            [Description("mput *")]
            CopyAllFiles = 3,

            [Description("mdelete *")]
            DeleteAllFiles = 4,

            [Description("disconnect")]
            Disconnect = 5,

            [Description("cd")]
            ChangeFtpFolder = 6,

            [Description("lcd")]
            ChangeLocalFolder = 7,

            [Description("bye")]
            ReturnToSystem = 8,
        }

        private readonly FtpServerDTO _ftpServer;

        /// <summary>
        /// Constructor that creates a directory info object from the folder
        /// provided in the path.
        /// </summary>
        /// <param name="directorypath"> The path of the root directory (the one we want to copy)</param>
        public FtpCommandsGenerator(FtpServerDTO ftpserver)
        {
            _ftpServer = ftpserver;
        }

        /// <summary>
        /// Generates a command .ftp file that copies over recursively
        /// a folder via ftp
        /// </summary>
        public void GenerateFtpCommandsFile()
        {
            CreateFtpCommandFile($@"{_ftpServer.FtpFileOutputDirectory}\{_ftpServer.FtpCommandsFileName}");
        }

        /// <summary>
        /// Creates an ftp file in the specified path. If the specified path does not exist,
        /// it attempts once to create the file in the desktop folder of the currently logged user.
        /// if that fails it exits with no file created
        /// </summary>
        /// <param name="filepath">Path where the file will be created</param>
        /// <returns>returns the file name</returns>
        private string CreateFtpCommandFile(string filepath, bool triedtofixoutputpath = false)
        {
            try
            {
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }
                // First command for the ftp should be open
                using (FileStream fs = File.Create(filepath))
                {
                    EstablishFtpConnection(fs, _ftpServer.IpAddress);
                    LoginAndValidate(fs);
                    IncludeAdditionalDirectoriesAsSubdirectories(_ftpServer.LocalDirectoryInfo, _ftpServer.AdditionalfoldersField);
                    DeleteFullDirectory(fs, _ftpServer.LocalDirectoryInfo);
                    CopyFullDirectory(fs, _ftpServer.LocalDirectoryInfo);
                    CloseFtpConnectionAndReturnToSystem(fs);
                    Console.WriteLine($"ftp command file generated at {_ftpServer.FtpFileOutputDirectory}");
                }

                return filepath;
            }
            catch (Exception)
            {
                if (triedtofixoutputpath)
                {
                    Console.WriteLine("Could not find desktop, file generation failed");
                }
                else
                {
                    Console.WriteLine("Specified output path for .ftp file does not exist, setting it to desktop");
                    _ftpServer.FtpFileOutputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    CreateFtpCommandFile($@"{_ftpServer.FtpFileOutputDirectory}\{_ftpServer.FtpCommandsFileName}", true);
                }
            }

            return "";
        }

        /// <summary>
        /// Writes the commands to the .ftp file for copying via ftp a directory, both its files
        /// and subfolders
        /// </summary>
        /// <param name="fs">The filestream that writes/reads to/from the file</param>
        /// <param name="actualdirectory"> The directory info object of the copied directory</param>
        private void CopyFullDirectory(FileStream fs, DirectoryInfo actualdirectory, int ftprootindex = 0)
        {
            NavigateToLocalDirectory(fs, actualdirectory);

            ftprootindex = ftprootindex == 0 ? actualdirectory.FullName.LastIndexOf(_ftpServer.FtpRootName) + _ftpServer.FtpRootName.Length : ftprootindex;
            NavigateToFtpDirectory(fs, actualdirectory.FullName.Substring(ftprootindex));

            var directorysubdirectories = actualdirectory.GetDirectories();
            var directoryfiles = actualdirectory.GetFiles();

            if (directoryfiles.Length > 0)
            {
                CopyAllFiles(fs);
            }

            if (directorysubdirectories.Length > 0)
            {
                CreateDirectories(fs, directorysubdirectories);
            }

            foreach (var subdirectory in directorysubdirectories)
            {
                CopyFullDirectory(fs, subdirectory, ftprootindex);
            }
        }

        /// <summary>
        /// Writes the commands to the .ftp file for deleting recursively a directory, both its files
        /// and subfolders
        /// </summary>
        /// <param name="fs">The filestream that writes/reads to/from the file</param>
        /// <param name="actualdirectory"> The directory info object of the copied directory</param>
        private void DeleteFullDirectory(FileStream fs, DirectoryInfo actualdirectory, int ftprootindex = 0)
        {
            ftprootindex = ftprootindex == 0 ? actualdirectory.FullName.LastIndexOf(_ftpServer.FtpRootName) + _ftpServer.FtpRootName.Length : ftprootindex;
            NavigateToFtpDirectory(fs, actualdirectory.FullName.Substring(ftprootindex));

            var directorysubdirectories = actualdirectory.GetDirectories();
            var directoryfiles = actualdirectory.GetFiles();

            if (directoryfiles.Length > 0)
            {
                DeleteAllFiles(fs);
            }

            foreach (var subdirectory in directorysubdirectories)
            {
                DeleteFullDirectory(fs, subdirectory);
                NavigateToFtpDirectory(fs, actualdirectory.FullName.Substring(ftprootindex));
                DeleteDirectory(fs, subdirectory);
            }
        }

        private void IncludeAdditionalDirectoriesAsSubdirectories(DirectoryInfo maindirectory, List<string> additionaldirectories)
        {
            foreach (var directorypath in additionaldirectories)
            {
                var additionaldirectoryinfo = new DirectoryInfo(directorypath);
                var additionalsubdirectoryinfo = maindirectory.CreateSubdirectory(additionaldirectoryinfo.Name);

                foreach (var file in additionaldirectoryinfo.GetFiles())
                {
                    if (!new FileInfo(additionalsubdirectoryinfo + "\\" + file.Name).Exists)
                    {
                        file.MoveTo(additionalsubdirectoryinfo + "\\" + file.Name);
                    }
                }
            }
        }

        private void EstablishFtpConnection(FileStream fs, string ftpseverip)
        {
            WriteToFile(fs, $"{FtpCommands.OpenConnection.GetDescription()} {ftpseverip}");
            WriteToFile(fs, Environment.NewLine);
        }

        private void LoginAndValidate(FileStream fs)
        {
            WriteToFile(fs, _ftpServer.User);
            WriteToFile(fs, Environment.NewLine);
            WriteToFile(fs, _ftpServer.Password);
            WriteToFile(fs, Environment.NewLine);
        }

        private void CloseFtpConnectionAndReturnToSystem(FileStream fs)
        {
            WriteToFile(fs, $"{FtpCommands.Disconnect.GetDescription()}");
            WriteToFile(fs, Environment.NewLine);
            WriteToFile(fs, $"{FtpCommands.ReturnToSystem.GetDescription()}");
            WriteToFile(fs, Environment.NewLine);
        }

        private void CopyAllFiles(FileStream fs)
        {
            WriteToFile(fs, $"{FtpCommands.CopyAllFiles.GetDescription()}");
            WriteToFile(fs, Environment.NewLine);
        }

        private void DeleteAllFiles(FileStream fs)
        {
            WriteToFile(fs, $"{FtpCommands.DeleteAllFiles.GetDescription()}");
            WriteToFile(fs, Environment.NewLine);
        }

        private void CreateDirectories(FileStream fs, DirectoryInfo[] directories)
        {
            foreach (var directory in directories)
            {
                WriteToFile(fs, $"{FtpCommands.CreateDirectory.GetDescription()} {directory.Name}");
                WriteToFile(fs, Environment.NewLine);
            }
        }

        private void DeleteDirectory(FileStream fs, DirectoryInfo directory)
        {
            WriteToFile(fs, $"{FtpCommands.DeleteDirectory.GetDescription()} {directory.Name}");
            WriteToFile(fs, Environment.NewLine);
        }

        private void NavigateToLocalDirectory(FileStream fs, DirectoryInfo directory)
        {
            WriteToFile(fs, $"{FtpCommands.ChangeLocalFolder.GetDescription()} {directory.FullName}");
            WriteToFile(fs, Environment.NewLine);
        }

        private void NavigateToFtpDirectory(FileStream fs, string ftpdirectorypath)
        {
            ftpdirectorypath = String.IsNullOrEmpty(ftpdirectorypath) == true ? @"\" : ftpdirectorypath;
            WriteToFile(fs, $"{FtpCommands.ChangeFtpFolder.GetDescription()} {ftpdirectorypath}");
            WriteToFile(fs, Environment.NewLine);
        }

        private void WriteToFile(FileStream fs, string command)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(command);
            fs.Write(info, 0, info.Length);
        }
    }
}