using SFB;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading;

public class Process : MonoBehaviour
{
    public InputField outputInfo;

    private Thread conversionThread;
    private string threadOutput = "";
    private bool isProcessing = false;

    public void OpenPdfAndConvert()
    {
        var extensions = new[] {
            new ExtensionFilter("PDF Files", "pdf"),
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select PDF", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string pdfPath = paths[0];
            outputInfo.text = "PDF selected: " + pdfPath;

            if (!isProcessing)
            {
                conversionThread = new Thread(() => ConvertPdfToImages(pdfPath));
                conversionThread.Start();
            }
            else
            {
                outputInfo.text = "Already in progress.";
            }
        }
        else
        {
            outputInfo.text = "No file found.";
        }
    }

    void ConvertPdfToImages(string pdfPath)
    {
        isProcessing = true;

        string popplerRelativePath = Path.Combine("poppler-24.08.0", "Library", "bin", "pdftoppm.exe");
        string exePath = Path.Combine(Application.dataPath, "..", popplerRelativePath);
        exePath = Path.GetFullPath(exePath);

        if (!File.Exists(exePath))
        {
            UpdateOutput($"pdftoppm.exe not found: {exePath}");
            isProcessing = false;
            return;
        }

        string exeFolder = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outputFolder = Path.Combine(exeFolder, "pdfimgs");

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string outputPrefix = Path.Combine(outputFolder, "page");
        string args = $"-png -r 150 \"{pdfPath}\" \"{outputPrefix}\"";//150 dpi

        UpdateOutput("Starting: " + exePath + " " + args);

        try
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = exePath,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.Log(e.Data);
                        UpdateOutput(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.LogError(e.Data);
                        UpdateOutput("[Error] " + e.Data);
                    }
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }

            UpdateOutput($"PDF success: {outputFolder}");
        }
        catch (Exception e)
        {
            UpdateOutput("Error: " + e.Message);
        }

        isProcessing = false;
    }


    private string outputToShow = "";

    void UpdateOutput(string message)
    {
        lock (this)
        {
            outputToShow = message;
        }
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(outputToShow))
        {
            lock (this)
            {
                outputInfo.text = outputToShow;
                outputToShow = "";
            }
        }
    }

    private void OnDestroy()
    {
        if (conversionThread != null && conversionThread.IsAlive)
            conversionThread.Abort();
    }
}



//NO THREADS, v1.0 (working)
/*using SFB;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
public class Process : MonoBehaviour
{
    public InputField outputInfo;

    public void OpenPdfAndConvert()
    {
        var extensions = new[] {
            new ExtensionFilter("PDF Files", "pdf"),
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select PDF", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string pdfPath = paths[0];
            outputInfo.text = "PDF selected: " + pdfPath;

            ConvertPdfToImages(pdfPath);
        }
        else
        {
            outputInfo.text = "No file found.";
        }
    }

    void ConvertPdfToImages(string pdfPath)
    {
        string popplerRelativePath = Path.Combine("poppler-24.08.0", "Library", "bin", "pdftoppm.exe");
        string exePath = Path.Combine(Application.dataPath, "..", popplerRelativePath);
        exePath = Path.GetFullPath(exePath);

        if (!File.Exists(exePath))
        {
            outputInfo.text = "pdftoppm.exe not found: " + exePath;
            return;
        }

        string exeFolder = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outputFolder = Path.Combine(exeFolder, "pdfimgs");

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string outputPrefix = Path.Combine(outputFolder, "page");
        string args = $"-png -r 150 \"{pdfPath}\" \"{outputPrefix}\"";//150 dpi

        outputInfo.text = "Task: " + exePath + " " + args;

        try
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = exePath,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
            {
                process.OutputDataReceived += (sender, e) => { if (e.Data != null) Debug.Log(e.Data); };
                process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Debug.LogError(e.Data); };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }

            outputInfo.text = $"PDF success: {outputFolder}";
        }
        catch (Exception e)
        {
            outputInfo.text = "Error: " + e.Message;
        }
    }
}*/
