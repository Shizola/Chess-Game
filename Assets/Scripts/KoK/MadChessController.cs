
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Chess.Core;
using UnityEngine;

public class MadChessController : MonoBehaviour
{
    private Process uciProcess;
    private StreamWriter engineStreamWriter;
    private StreamReader engineStreamReader;
    private Thread readThread;

    public string madChessExePath; // Set this in the Inspector to the name of your .exe file

    public event Action onUCIok;
    public event Action onIsReady;

    public Action<string> onSearchComplete;

    void Start()
    {
        // Get the full path to the .exe file in the StreamingAssets folder
        string exeFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, madChessExePath);

        UnityEngine.Debug.Log(madChessExePath);

        // Check if the process with the given exeFileName is already running
        Process[] processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(madChessExePath));
        if (processes.Length > 0)
        {
            // The process is already running, you may want to handle this case accordingly
            UnityEngine.Debug.Log("MadChess is already running.");
        }
        else
        {
            UnityEngine.Debug.Log("Launching MadChess");

            // Launch the .exe file externally            
            LaunchUCIEngine(exeFilePath);
        }
    }


    private void LaunchUCIEngine(string enginePath)
    {
        uciProcess = new Process();
        uciProcess.StartInfo.FileName = enginePath;
        uciProcess.StartInfo.UseShellExecute = false;
        uciProcess.StartInfo.RedirectStandardInput = true;
        uciProcess.StartInfo.RedirectStandardOutput = true;
        uciProcess.StartInfo.CreateNoWindow = true;
        uciProcess.EnableRaisingEvents = true;
        uciProcess.OutputDataReceived += OnEngineOutputReceived;

        if (uciProcess.Start())
        {
            engineStreamReader = uciProcess.StandardOutput;
            engineStreamWriter = uciProcess.StandardInput;

            // Start reading asynchronously from the engine's output using a separate thread
            readThread = new Thread(ReadEngineOutput);
            readThread.Start();


            // CalculateBestMove();
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to launch UCI engine.");
        }
    }

    private void ReadEngineOutput()
    {
        while (!engineStreamReader.EndOfStream)
        {
            string data = engineStreamReader.ReadLine();
            // Handle the engine's response here           

            if (data != "")
            {
                UnityEngine.Debug.Log("Engine Response: " + data);
            }

            // For example, the engine will respond with "uciok" to the "uci" command
            if (data == "uciok")
            {
                // The engine has identified itself, now we can send it commands
                //SendCommand("isready");
                onUCIok?.Invoke();
            }
            else if (data == "readyok")
            {
                // // The engine has responded to the isready command, now we can send it commands
                // SendCommand("ucinewgame");
                // SendCommand("position startpos");
                // SendCommand("go depth 10"); // Change the search depth as needed

                onIsReady?.Invoke();

            }
            else if (data.StartsWith("bestmove"))
            {
                // The engine has responded with the best move it has found
                // Parse the response to get the best move
                string[] bestMoveSplit = data.Split(' ');
                string bestMove = bestMoveSplit[1];
                UnityEngine.Debug.Log("Best move: " + bestMove);
                onSearchComplete?.Invoke(bestMove);
            }
        }
    }

    private void OnEngineOutputReceived(object sender, DataReceivedEventArgs e)
    {
        // The event handler will be empty, as we handle the engine output in the separate thread
    }

    public void SendCommand(string command)
    {
        if (engineStreamWriter != null)
        {
            engineStreamWriter.WriteLine(command);
            engineStreamWriter.Flush();
        }
    }

    public void CheckUCI()
    {
        // Send the 'uci' command to identify the engine
        SendCommand("uci");
        UnityEngine.Debug.Log("check uci");
    }

    public void NewGame()
    {
        SendCommand("setoption name uci_limitstrength value true");
        SendCommand("setoption name uci_elo value 600");
        
        SendCommand("ucinewgame");

          
    }

    public void CheckIsReady()
    {
        SendCommand("isready");
    }

    // Make sure to clean up when the application closes or when you don't need the engine anymore
    private void OnApplicationQuit()
    {
        if (uciProcess != null && !uciProcess.HasExited)
        {
            uciProcess.CloseMainWindow();
            uciProcess.Close();
        }

        // Make sure to stop the read thread before quitting the application
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }
    }

    public void SendPosition(string fen)
    {
         SendCommand("setoption name uci_limitstrength value true");
        SendCommand("setoption name uci_elo value 600");
        SendCommand("position fen " + fen);
        SendCommand("go depth 4");
    }
}
