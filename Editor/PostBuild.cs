using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Diagnostics;


[ExecuteInEditMode]
public class PostBuild {

    public static UploadData data = new UploadData();
    public static string UploaderPath;

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) 
    {
	    UploaderPath = System.Text.RegularExpressions.Regex.Replace(Application.dataPath,"Assets","Uploader");

        if(!Directory.Exists(UploaderPath))
            return;

        string Args = "-i \"" + pathToBuiltProject + "\" -o \"" + UploaderPath + "\\" + data.File_Name + "\" -f -u -r -p -d";
        UnityEngine.Debug.Log(Args);
        Process UnityTools = new Process();
        UnityTools.StartInfo.FileName = UploaderPath + "\\UnityTools.exe";
        UnityTools.StartInfo.Arguments = Args;
        UnityTools.EnableRaisingEvents = true;
        UnityTools.Exited += new System.EventHandler(ProcessExit);
        UnityTools.Start();
    }

    private static void ProcessExit(object sender, System.EventArgs e)
    {
        Process VitaFTPI = new Process();
        VitaFTPI.StartInfo.FileName = UploaderPath + "\\VitaFTPI.exe";
        string Args = "--ip " + data.IP + " --vpk " + data.File_Name + ".vpk" + " --usb " + boolToString(data.UseUSB) + " --drive-letter " + data.DriveLetter;
        UnityEngine.Debug.Log(Args);
        VitaFTPI.StartInfo.Arguments = Args;
        VitaFTPI.Start();
    }

    static string boolToString(bool input)
    {
        if (input)
        {
            return "true";
        }
        else
            return "false";
    }

    public class UploadData
    {
        //The only reason I made this class was so you can change the values easily without breaking the other code :)
        public string IP = "192.168.18.8";
        public string File_Name = "Build";

        //Only use this when UseUSB in set to true. This will transfer the VPK over usb but still install it via ftp so the ftpanywhere plugin is required.
        public string DriveLetter = "E:";
        // Set this to false for now it doesn't work properly
        public bool UseUSB = false;
    }
}
