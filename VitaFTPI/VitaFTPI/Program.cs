using System;
using System.IO;
using System.IO.Compression;
using WinSCP;
using PeXploit;
using System.Threading;

namespace VitaFTPI
{
    class Program
    {
        static string driveLetter = "";
        static bool useUSB = false;
        static string VitaIP = "";
        static string VPKPath = "";
        static string BaseVPK;
        static string Command = "PROM ";
        static int port = 1337;
        static SessionOptions sessionOptions;
        static string SendPath = "ux0:/data/sent.vpk";
        static string AppID;

        static string BaseDirectory = "C:\\VitaFTPI\\";
        static string ExtractDirectory = BaseDirectory + "Extracted\\";
        static string BaseFiles = BaseDirectory + "Base Files\\";
        static string MediaFiles = BaseDirectory + "Media Files\\";

        static PARAM_SFO param;

        static void Main(string[] args)
        {

            if(args.Length == 0)
            {
                Console.WriteLine("No input specified Aboring!");
                return;
            }

            for(int x = 0; x < args.Length; x += 2)
            {
                if (args[x] == "--vpk")
                {
                    VPKPath = args[x + 1];
                }
                if (args[x] == "--ip")
                {
                    VitaIP = args[x + 1];
                }
                if (args[x] == "--usb")
                {
                    useUSB = (args[x + 1] == "true");
                }
                if (args[x] == "--drive-letter")
                {
                    driveLetter = args[x + 1];
                }
                if(args[x] == "--standalone-install")
                {
                    StandAloneInstall();
                    return;
                }
            }


            if(VitaIP == "" || VPKPath == "")
            {
                Console.WriteLine("Invalid Arguments Aborting!");
            }
               

            if (!File.Exists(VPKPath))
            {
                //Checking if the input file specified exists
                Console.WriteLine("No file found. Check your input path and make sure to include the file extension.\nFor Example:\napp.vpk");
                Console.WriteLine(VPKPath);
                Thread.Sleep(5000);
                return;
            }

            ConfigureOptions();
            CreateVPK();
            if (!useUSB) UploadVPK();
            else CopyInstall();
            ClearDirectories();
        }

        static void StandAloneInstall()
        {
            ConfigureOptions();
            using(Session session = new Session())
            {
                session.Open(sessionOptions);
                session.Timeout = TimeSpan.FromSeconds(120000.0);
                TransferOptions toptions = new TransferOptions();
                toptions.TransferMode = TransferMode.Binary;
                Console.WriteLine("Installing VPK");
                session.ExecuteCommand("PROM ux0:/data/sent.vpk");
            }
        }

        static void CopyInstall()
        {
            Console.WriteLine("Now copying the Base VPK over USB");
            File.Copy(BaseVPK, driveLetter + "\\data\\sent.vpk",true);
            using(Session session = new Session())
            {
                session.Open(sessionOptions);
                session.Timeout = TimeSpan.FromSeconds(120000.0);
                TransferOptions options = new TransferOptions();
                options.TransferMode = TransferMode.Binary;
                Console.WriteLine("Now installing base VPK");
                while(!File.Exists(driveLetter + "\\app\\" + param.TITLEID + "\\eboot.bin"))
                {
                    Console.WriteLine("Installing VPK");
                    session.ExecuteCommand("PROM ux0:/data/sent.vpk");
                    Thread.Sleep(5000);
                }
                Console.WriteLine("Now copying the rest of the VPK files...");
                CopyAll(new DirectoryInfo(MediaFiles), new DirectoryInfo(driveLetter + "\\app\\" + param.TITLEID));
                session.Close();
            }
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        static void ClearDirectories()
        {
            Directory.Delete(BaseDirectory, true);
            Console.WriteLine("All operations completed successfully");
            Console.WriteLine("Closing in 5 seconds...");
            //sleeping in milliseconds
            Thread.Sleep(5000);
        }

        static void CreateVPK()
        {
            //This function is the workaround. It lets us install the VPK via 1 command "PROM"

            //We create all the necessary directories
            Console.WriteLine("Creating Directories...");
            recuresivelyCreateDirectory(BaseDirectory);
            recuresivelyCreateDirectory(ExtractDirectory);
            recuresivelyCreateDirectory(MediaFiles);
            recuresivelyCreateDirectory(BaseFiles);

            //first we extratct the contents of the VPK to a directory
            Console.WriteLine("Extracting VPK");
            UnZipFile(VPKPath, ExtractDirectory);

            //First we check if the VPK has the necessary files
            Console.WriteLine("Extracting VPK");
            if (!Directory.Exists(ExtractDirectory + "sce_sys") || !File.Exists(ExtractDirectory + "eboot.bin"))
            {
                Console.WriteLine("Invalid VPK closing in 5 seconds...");
                //Sleeping for 5000 milliseconds (so 5 seconds)
                Thread.Sleep(5000);
                //Closing with exit code 0 so windows won't give and error and start it's troubleshooter or whatever
                Environment.Exit(0);
                return;
            }

            //Then we move the base VPK files to the Base Files directory
            Console.WriteLine("Moving base VPK files");
            Directory.Move(ExtractDirectory + "sce_sys", BaseFiles + "sce_sys");
            File.Move(ExtractDirectory + "eboot.bin", BaseFiles + "eboot.bin");

            //Now we move the rest of the files to the Media Files directory for later
            Console.WriteLine("Moving the rest of the files for later...");
            foreach (string dirName in Directory.GetDirectories(ExtractDirectory))
            {
                string MoveToPath = System.Text.RegularExpressions.Regex.Replace(dirName, "Extracted", "Media Files");
                Directory.Move(dirName, MoveToPath);
            }

            foreach (string fileName in Directory.GetFiles(ExtractDirectory))
            {
                string MoveToPath = System.Text.RegularExpressions.Regex.Replace(fileName, "Extracted", "Media Files");
                File.Move(ExtractDirectory + fileName, MoveToPath);
            }

            //Now we remake the VPK with just the base files
            Console.WriteLine("Remaking VPK with just base files..");
            ZipFile.CreateFromDirectory(BaseFiles, BaseDirectory + "Base.vpk", CompressionLevel.Optimal, false);

            BaseVPK = BaseDirectory + "Base.vpk";

            //We set the param.sfo file for later use.
            param = new PARAM_SFO(BaseFiles + "sce_sys\\param.sfo");
        }

        static void ConfigureOptions()
        {
            //Configure the options for the FTP transfer
            sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = VitaIP,
                PortNumber = port,
                UserName = "anonymous",
                Password = ""
            };
        }


        static void UploadVPK()
        {
            using (Session session = new Session())
            {
                //We add an event handler
                session.FileTransferProgress += new FileTransferProgressEventHandler(ProgressChanged);
                //We start the FTP session
                session.Open(sessionOptions);
                //We set a timeout value and configure other options
                session.Timeout = TimeSpan.FromSeconds(120000.0);
                TransferOptions options = new TransferOptions();
                options.TransferMode = TransferMode.Binary;
                //We start the transfer and store the result in a variable
                Console.WriteLine("Now uploading the base VPK");
                TransferOperationResult result = session.PutFiles(BaseVPK, SendPath, false, options);
                //We check if the transfer is successful (I think)
                result.Check();
                //We Write it in the log
                foreach (FileOperationEventArgs transfer in result.Transfers)
                {
                    Console.WriteLine("Upload of {0} successful!", (object)transfer.FileName);
                }

                //We install the VPK
                Console.WriteLine("Installing VPK");
                session.ExecuteCommand(Command + SendPath);

                //Now we find where the VPK installed to by looking for it's appid in the param.sfo file that we set earilier
                AppID = param.TITLEID;

                //Now we upload the rest of the VPK files
                Console.WriteLine("Uploading rest of the necessary files for the VPK to it's app directory in ux0:/app/" + AppID);
                TransferOperationResult res = session.PutFilesToDirectory(MediaFiles, "ux0:/app/" + AppID);
                foreach (FileOperationEventArgs file in res.Transfers)
                {
                    Console.WriteLine("Upload of {0} successful!", (object)file.FileName);
                }
                //Now we close the FTP session
                Console.WriteLine("Closing session");
                session.Close();
            }
        }

        static void ProgressChanged(object sender, FileTransferProgressEventArgs e)
        {
            Console.WriteLine(e.OverallProgress * 100 + "%");
        }

        static void UnZipFile(string path, string outputDir)
        {
            //Checking if input exists
            if (!File.Exists(path)) return;
            //Checking if output directory exists otherwise we create it.
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);


            if (Directory.GetFiles(outputDir).Length != 0)
            {
                Directory.Delete(outputDir);
                Directory.CreateDirectory(outputDir);
            }

            ZipFile.ExtractToDirectory(path, outputDir);
        }

        static void recuresivelyCreateDirectory(string Path)
        {
            //We check if the directories already exist if they do we will delete them first
            if (Directory.Exists(Path)) Directory.Delete(Path, true);

            Directory.CreateDirectory(Path);
        }
    }
}
