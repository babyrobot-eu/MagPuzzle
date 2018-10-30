
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TcpClientUnity : MonoBehaviour {

    public enum TypeOfTCPConnection { Gaze, Speech }

    public TypeOfTCPConnection TypeOfConnection;

    private const int MillisecondsSleepTime = 5000;

    //ip/address of the server, 127.0.0.1 is for your own computer
    public string conHost = "localhost";

    //port for the server, make sure to unblock this in your router firewall if you want to allow external connections
    public int conPort = 8081;

    GazeDrawing gaze;
    TcpClient mySocket;
    NetworkStream theStream;
    StreamWriter theWriter;
    StreamReader theReader;

    public Text ChatBox;
    public Text TextStatusBox;
    public String connectedText;
    public String disConnectedText;

    //a true/false variable for connection status
    private bool socketReady = false;
    private bool justConnected = false;

    private Thread SocketThread;
    private bool quitApplication = false;
    private string newResultString;
    private bool newResult = false;
    private bool justDisConnected = false;
    private string gazeTarget;
    private bool updateGazeTarget = false;
    private string lastMessage;
    private bool lastMessageIsUnknown = false;

    private void Update()
    {
        if (newResult)
        {
            ChatBox.text = newResultString;
            newResult = false;
        }
        if (justConnected)
        {
            changeTextBox(true);
            justConnected = false;
        }
        if (justDisConnected)
        {
            changeTextBox(false);
            justDisConnected = false;
        }
        if (updateGazeTarget)
        {
            gaze.SetGazeToQuadrant(gazeTarget);
            AutonomousGazeBehavior.Instance.EventUserLookingAtQuadrantOrPlayer(gazeTarget);
            updateGazeTarget = false;
        }
    }



    private void Start()
    {
        gaze = GetComponent<GazeDrawing>();
        changeTextBox(false);
        SocketThread = new Thread(setupSocket);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }

    private void changeTextBox(bool isConnected)
    {
        if (isConnected)
        {
            TextStatusBox.text = connectedText;
            TextStatusBox.color = Color.green;
        }
        else
        {
            TextStatusBox.text = disConnectedText;
            TextStatusBox.color = Color.red;
        }
    }

    public string removeEmptySpaces(string text, char stopAt = '\0')
    {
        if (!string.IsNullOrEmpty(text))
        {
            int charLocation = text.IndexOf(stopAt);

            if (charLocation > 0)
            {
                return text.Substring(0, charLocation);
            }
        }

        return String.Empty;
    }

    private void setupSocket()
    {
        while (!quitApplication)
        {

            if (socketReady == false || !theStream.CanRead)
            {
                socketReady = false;
                startSocket();
            }
            else
            {

                //if (mySocket.Client.Poll(1, SelectMode.SelectRead) && !theStream.DataAvailable)
                //{
                //    Debug.Log("Detected disconnect, closing server!");
                //    closeSocket();
                //}
                string serverSays = readSocket();
                if (serverSays != "")
                {
                    if (TypeOfConnection == TypeOfTCPConnection.Speech)
                    {
                        newResultString = serverSays;
                        newResult = true;
                    }
                    else
                    {

                        string stringFromServer = removeEmptySpaces(serverSays);
                        string[] messages = stringFromServer.Split('M');

                        string[] message = messages[messages.Length - 1].Split(';');
                        string[] userLocation = message[1].Split(',');
                        float z = float.Parse(userLocation[2]);

                        AutonomousGazeBehavior.Instance.UpdatePlayerPosition(new Vector3(float.Parse(userLocation[0]), float.Parse(userLocation[1]), z));
                        if (message[0] == "Unknown")
                        {
                            if (!lastMessageIsUnknown)
                                Dbg.Log(LogMessageType.PLAYER_GAZE_TARGET, "Unknown");
                            lastMessageIsUnknown = true;
                        }
                        else
                        {
                            switch (message[0])
                            {
                                case "Q1":
                                    SetGazeTarget("Q1");
                                    break;
                                case "Q2":
                                    SetGazeTarget("Q2");
                                    break;
                                case "Q3":
                                    SetGazeTarget("Q3");
                                    break;
                                case "Q4":
                                    SetGazeTarget("Q4");
                                    break;
                                case "Robot":
                                    SetGazeTarget("Robot");
                                    break;

                            }
                            lastMessageIsUnknown = false;
                        }
                    }
                }
            }
        }
    }

    private void SetGazeTarget(string gazeTarget)
    {
        Dbg.Log(LogMessageType.PLAYER_GAZE_TARGET, gazeTarget);
        this.gazeTarget = gazeTarget;
        updateGazeTarget = true;
    }

    private void startSocket()
    {
        try
        {
            mySocket = new TcpClient(conHost, conPort);
            theStream = mySocket.GetStream();
            theWriter = new StreamWriter(theStream);
            theReader = new StreamReader(theStream);
            socketReady = true;
            justConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log("Could not connect, Socket error:" + e);
            Thread.Sleep(MillisecondsSleepTime);
            socketReady = false;
        }
    }

    //send message to server
    public void writeSocket(string str)
    {
        if (!socketReady)
            return;
        String tmpString = str + "\r\n";
        theWriter.Write(tmpString);
        theWriter.Flush();
    }

    //read message from server
    private string readSocket()
    {
        String result = "";
        if (theStream.DataAvailable)
        {
            Byte[] inStream = new Byte[mySocket.SendBufferSize];
            theStream.Read(inStream, 0, inStream.Length);
            result += System.Text.Encoding.UTF8.GetString(inStream);
         }
        return result;
    }

    //disconnect from the socket
    private void closeSocket()
    {
        socketReady = false;
        justDisConnected = true;
        if (!socketReady)   
            return;
        theWriter.Close();
        theReader.Close();
        mySocket.Close();

    }

    private void OnApplicationQuit()
    {
        closeSocket();
        quitApplication = true;
    }



}
