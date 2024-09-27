#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace PlayVibe
{
    public class BuildAssistant
    {
        [PostProcessBuild]
        private static void OnBuildFinished(BuildTarget target, string pathToBuiltProject)
        {
            return;
            CreateRarArchive();
        }
        
        private static void CreateRarArchive()
        {
            try
            {
                string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string projectFolder = Path.Combine(desktopFolder, "Build");
                
                if (!Directory.Exists(projectFolder))
                {
                    UnityEngine.Debug.LogError("Project folder does not exist.");
                    return;
                }

                string[] files = Directory.GetFiles(projectFolder, "*.*", SearchOption.AllDirectories);
                string rarFilePath = Path.Combine(desktopFolder, "Build.rar");
                string arguments = $"a -r \"{rarFilePath}\" \"{string.Join("\" \"", files)}\"";
                Process rarProcess = new Process();
                rarProcess.StartInfo.FileName = "C:/Program Files/WinRAR/WinRAR.exe";
                rarProcess.StartInfo.Arguments = arguments;
                rarProcess.Start();
                rarProcess.WaitForExit();
                
                SendBuild();

                UnityEngine.Debug.Log("Archive created successfully: " + rarFilePath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error creating rar archive: " + e.Message);
            }
        }

        private static void SendBuild()
        {
            string message = DateTime.Now.ToString();
            string secondAppPath = @"C:/Users/User/source/repos/PlayVibe_BuildAssistant/PlayVibe_BuildAssistant/bin/Debug/netcoreapp3.1/PlayVibe_BuildAssistant.exe";
            
            Process externalAppProcess = new Process();
            externalAppProcess.StartInfo.FileName = secondAppPath;
            externalAppProcess.StartInfo.UseShellExecute = false;
            externalAppProcess.StartInfo.RedirectStandardInput = true;
            externalAppProcess.StartInfo.Arguments = message; 
            externalAppProcess.Start();
        }
    }
}
#endif