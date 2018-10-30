using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCPFurhatComm;
using UnityEngine.UI;
using System;

public class Furhat : MonoBehaviour
{

    FurhatInterface furhat;
    public string IPAddress;
    public Text robotTextBox;
    private bool clearRobotText = false;
    public static Furhat Instance;
    public Vector3 boardBottomLeftPosition;
    public float boardPieceSize;
    public List<Vector3> pieceLocations;
    public string lookAt;
    private bool asleeep = true;
    private Vector3 gazePositionQ1;
    private Vector3 gazePositionQ2;
    private Vector3 gazePositionQ3;
    private Vector3 gazePositionQ4;
    public bool robotInDialogAct;

    public bool Asleeep
    {
        get
        {
            return asleeep;
        }

        set
        {
            asleeep = value;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void LookAtPiece()
    {
        Gaze(returnLocation(lookAt));
    }

    internal void WakeUP()
    {
        Asleeep = false;
        furhat.Gesture(GESTURES.MOVING.WAKE_UP);
        AutonomousGazeBehavior.Instance.GazeAtUserStartInteraction();
    }

    internal void FallAsleep()
    {
        Asleeep = true;
        furhat.Gesture(GESTURES.MOVING.SLEEP);
        Gaze(0, -1, 1);
    }

    internal void Gaze(Location gazePoint)
    {
        Dbg.Log(LogMessageType.SETUP_AND_PROBLEMS, new List<string>() { "FURHAT_GAZE_TCP", gazePoint.x.ToString(), gazePoint.y.ToString(), gazePoint.z.ToString() });
        furhat?.Gaze(gazePoint);
    }

    internal void Gaze(Vector3 gazePoint)
    {
        Gaze(new Location(gazePoint.x, gazePoint.y, gazePoint.z));
    }

    internal void Gaze(float x, float y, float z)
    {
        Gaze(new Location(x,y,z));
    }

    public Location returnLocation(string piece)
    {
        switch (piece)
        {
            case "A1":
                return new Location(pieceLocations[0].x, pieceLocations[0].y, pieceLocations[0].z);
            case "B1":
                return new Location(pieceLocations[1].x, pieceLocations[1].y, pieceLocations[1].z);
            case "C1":
                return new Location(pieceLocations[2].x, pieceLocations[2].y, pieceLocations[2].z);
            case "D1":
                return new Location(pieceLocations[3].x, pieceLocations[3].y, pieceLocations[3].z);
            case "A2":
                return new Location(pieceLocations[4].x, pieceLocations[4].y, pieceLocations[4].z);
            case "B2":
                return new Location(pieceLocations[5].x, pieceLocations[5].y, pieceLocations[5].z);
            case "C2":
                return new Location(pieceLocations[6].x, pieceLocations[6].y, pieceLocations[6].z);
            case "D2":
                return new Location(pieceLocations[7].x, pieceLocations[7].y, pieceLocations[7].z);
            case "A3":
                return new Location(pieceLocations[8].x, pieceLocations[8].y, pieceLocations[8].z);
            case "B3":
                return new Location(pieceLocations[9].x, pieceLocations[9].y, pieceLocations[9].z);
            case "C3":
                return new Location(pieceLocations[10].x, pieceLocations[10].y, pieceLocations[10].z);
            case "D3":
                return new Location(pieceLocations[11].x, pieceLocations[11].y, pieceLocations[11].z);
            case "A4":
                return new Location(pieceLocations[12].x, pieceLocations[12].y, pieceLocations[12].z);
            case "B4":
                return new Location(pieceLocations[13].x, pieceLocations[13].y, pieceLocations[13].z);
            case "C4":
                return new Location(pieceLocations[14].x, pieceLocations[14].y, pieceLocations[14].z);
            case "D4":
                return new Location(pieceLocations[15].x, pieceLocations[15].y, pieceLocations[15].z);
            case "Q1":
                return new Location(gazePositionQ1.x, gazePositionQ1.y, gazePositionQ1.z);
            case "Q2":
                return new Location(gazePositionQ2.x, gazePositionQ2.y, gazePositionQ2.z);
            case "Q3":
                return new Location(gazePositionQ3.x, gazePositionQ3.y, gazePositionQ3.z);
            case "Q4":
                return new Location(gazePositionQ4.x, gazePositionQ4.y, gazePositionQ4.z);
            case "Q1Options":
                return new Location(gazePositionQ1.x, gazePositionQ1.y, gazePositionQ1.z);
            case "Q2Options":
                return new Location(gazePositionQ2.x, gazePositionQ2.y, gazePositionQ2.z);
            case "Q3Options":
                return new Location(gazePositionQ3.x, gazePositionQ3.y, gazePositionQ3.z);
            case "Q4Options":
                return new Location(gazePositionQ4.x, gazePositionQ4.y, gazePositionQ4.z);
            default:
                return new Location(0, 0, 0);
        }
    }

    private Location getPlayerLocation()
    {
        return new Location(0, 0, 1);
    }

    internal void gaze(string targetName)
    {
        if(targetName == "Player")
        {
            Gaze(new Location(0, 0, 1));
        }
        else
        {
            Location location = returnLocation(targetName);
            Gaze(location);
        }
  
    }

    // Use this for initialization
    void Start()
    {
        ConnectToFurhat();
        Gaze(new Location(0, -1, 1));
        furhat.Gesture(GESTURES.MOVING.SLEEP);

        gazePositionQ1 = new Vector3((pieceLocations[8].x + pieceLocations[9].x) / 2, pieceLocations[8].y, (pieceLocations[9].z + pieceLocations[12].z) / 2);
        gazePositionQ2 = new Vector3((pieceLocations[0].x + pieceLocations[1].x) / 2, pieceLocations[0].y, (pieceLocations[1].z + pieceLocations[4].z) / 2);
        gazePositionQ3 = new Vector3((pieceLocations[10].x + pieceLocations[11].x) / 2, pieceLocations[10].y, (pieceLocations[11].z + pieceLocations[14].z) / 2);
        gazePositionQ4 = new Vector3((pieceLocations[2].x + pieceLocations[3].x) / 2, pieceLocations[2].y, (pieceLocations[3].z + pieceLocations[6].z) / 2);
    }

    public void ConnectToFurhat()
    {
        Dbg.Log(LogMessageType.SETUP_AND_PROBLEMS, "Connecting to Furhat");
        //furhat?.CloseConnection();
        furhat = new FurhatInterface(IPAddress);
        furhat.EndSpeechAction += () =>
        {
            EndSpeechActionMethod();
        };
    }

    public void EndSpeechActionMethod()
    {
        SpeechRecognition.Instance.StartSpeechRecognition();
        clearRobotText = true;
        AutonomousGazeBehavior.Instance.RobotStoppedDialogAct();
        robotInDialogAct = false;
        Dbg.Log(LogMessageType.DIALOG_ACT, "END_OF_UTTERANCE");
    }


    // Update is called once per frame
    void Update()
    {
        if (clearRobotText)
        {
            clearRobotText = false;
            robotTextBox.text = "";
        }
    }

    public void say(string text, string emotion)
    {
        robotTextBox.text = text;
        SpeechRecognition.Instance.StopSpeechRecognition();
        if (emotion == "")
        {
            furhat?.Say(text);
            Dbg.Log(LogMessageType.SETUP_AND_PROBLEMS, new List<string>() { "FURHAT_SAY_TCP", text });
        }
        else
        {
            furhat?.Say(text, emotion);
            Dbg.Log(LogMessageType.SETUP_AND_PROBLEMS, new List<string>() { "FURHAT_SAY_EMOTION_TCP", text, emotion });
        }
    }

    public void blink()
    {
        furhat?.Gesture("blink");
    }

    private void OnApplicationQuit()
    {
        furhat?.CloseConnection();
    }

}
