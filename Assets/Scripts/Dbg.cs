using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum LogMessageType { DIALOG_ACT, PIECE_FOUND, PIECE_LOST, SPEECH_REC_FINAL, SPEECH_REC_HYPOTHESIS, PLAYER_GAZE_TARGET, ROBOT_GAZE_TARGET, SESSION_START, SESSION_END, TOBII,
    PLAYER_POSITION, SETUP_AND_PROBLEMS
}

public class Dbg : MonoBehaviour {

    private string LogFileExtension = ".tsv";
    public bool EchoToConsole = true;
    public bool AddTimeStamp = true;

    private StreamWriter OutputStream;

    static Dbg Singleton = null;

    public static Dbg Instance
    {
        get { return Singleton; }
    }


    void Awake()
    {
        if (Singleton != null)
        {
            UnityEngine.Debug.LogError("Multiple Dbg Singletons exist!");
            return;
        }

        Singleton = this;
    }

    public void CreateLoggingFile(string participantName, string sessionType)
    {
        if (OutputStream != null)
        {
            OutputStream.Close();
            OutputStream = null;
        }
        // Open the log file to append the new log to it.
        string LogPath = @"Logs\";
        System.IO.Directory.CreateDirectory(LogPath);
        var files = Directory.GetFiles(LogPath, "*" + LogFileExtension);
        int LastSession = 0;
        foreach (var item in files)
        {
            int currentSession = int.Parse(Path.GetFileName(item).Split('-')[0]);
            LastSession = LastSession < currentSession ? currentSession : LastSession;
        }
        LastSession = LastSession + 1;
        OutputStream = new StreamWriter(LogPath + LastSession + "-" + participantName + "_" + sessionType + LogFileExtension, true);
        Log(LogMessageType.SESSION_START, new List<string> { Settings.Instance.ParticipantName.text, Settings.Condition.ToString()});
    }

    internal static void Log(LogMessageType logMessage, string content)
    {
        Log(logMessage.ToString() + "\t" + content);
    }

    internal static void Log(LogMessageType logMessage, List<string> contentList)
    {
        StringBuilder allContent = new StringBuilder();
        for (int i = 0; i < contentList.Count - 1; i++)
        {
            allContent.Append(contentList[i]);
            allContent.Append('\t');
        }
        allContent.Append(contentList[contentList.Count - 1]);

        Log(logMessage, allContent.ToString());
    }


    void OnDestory()
    {
        Close();
    }

    public void Close()
    {
        Log(LogMessageType.SESSION_END, new List<string> { Settings.Instance.ParticipantName.text, Settings.Condition.ToString()});
        if (OutputStream != null)
        {
            OutputStream.Close();
            OutputStream = null;
        }
    }

    private void TimeStampMessage(string message)
    {

    }

    private void Write(string message)
    {
        if (OutputStream != null)
        {
            OutputStream.WriteLine(message);
            OutputStream.Flush();
        }

        if (EchoToConsole)
        {
            UnityEngine.Debug.Log(message);
        }
    }

    public static void Log(string message)
    {
        //message = string.Format("[{0:H:mm:ss}]\t {1}", DateTime.Now, message);

        message = string.Format("{0:H:mm:ss:ms}\t{1}\t{2}",DateTime.Now , DateTimeOffset.Now.ToUnixTimeMilliseconds(), message);

        if (Dbg.Instance != null)
            Dbg.Instance.Write(message);
        else
            // Fallback if the debugging system hasn't been initialized yet.
            UnityEngine.Debug.Log(message);
    }
}
