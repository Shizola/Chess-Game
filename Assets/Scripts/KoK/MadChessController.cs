
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Chess.Core;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public int centipawnScore;
    public int currentOpponentSkillElo;

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
                onSearchComplete?.Invoke(bestMove);
            }
            else if (data.StartsWith("info depth"))
            {
                // The engine has provided an "info depth" response
                // Extract and display the evaluation score if available
                ExtractAndDisplayEvaluation(data);
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

    public void NewGame(int skillElo)
    {
        currentOpponentSkillElo = skillElo;

        UnityEngine.Debug.Log(currentOpponentSkillElo.ToString());

       // SendCommand("debug on");
        SendCommand("setoption name uci_limitstrength value true");
        SendCommand("setoption name uci_elo value " + currentOpponentSkillElo.ToString());
        
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
        SendCommand("position fen " + fen);
        SendCommand("go nodes 10");
    }

    //temp
    // Method to extract and display the evaluation score
    private void ExtractAndDisplayEvaluation(string infoDepthResponse)
    {
        // Check if the response contains the "score cp" information
        if (infoDepthResponse.Contains("score cp"))
        {
            // Split the response into tokens
            string[] tokens = infoDepthResponse.Split(' ');

            // Find the index of the "score" token
            int scoreIndex = Array.IndexOf(tokens, "score");

            if (scoreIndex != -1 && scoreIndex + 2 < tokens.Length)
            {
                // Extract the score value (in centipawns)
                centipawnScore = int.Parse(tokens[scoreIndex + 2]);

                // Convert the centipawn score to a traditional evaluation score
                // float evaluationScore = cpScore / 100.0f;

                // Display the traditional evaluation score
                // string formattedScore = evaluationScore.ToString("+#0.0;-#0.0;0");
                //UnityEngine.Debug.Log("Evaluation Score: " + formattedScore);
            }
        }
    }

}