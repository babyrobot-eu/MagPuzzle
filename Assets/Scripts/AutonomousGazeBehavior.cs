using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum GazeState {ROBOT_DISTRACTED, ROBOT_LOOKING_AT_PLAYER, ROBOT_LOOKING_AT_QUADRANT, EVENT_PLAYER_STARTED_SPEAKING, EVENT_PIECE_MOVED, EVENT_PLAYER_LOOKING_AT_QUADRANT, EVENT_PLAYER_LOOKING_AT_ROBOT, DIALOG_ACT, WAITING_AFTER_SQUARE_DIALOG_ACT };
public enum Priority { Highest = 5, High = 4, Medium = 3, Low = 2, Lowest = 1, None = 0}

class AutonomousGazeBehavior : MonoBehaviour
{
    Furhat furhat;
    GazeDrawing furhatInterfaceGaze;
    private GameObject PlayerGazeTarget;
    private GameObject DistractedGazeTarget;

    [Header("Reacting to Events")]
    Priority DialogActsPriority = Priority.Highest;
    Priority BoardEventsPriority = Priority.High;
    public double ProbabilityToLookAtBoardEvents = 1;
    public Vector2 TimesBoardEvents = new Vector2();
    Priority UserSpeakingPriority = Priority.Medium;
    public double ProbabilityUserSpeaking = 1;
    //public bool AlwaysLookingAtUserDuringSpeech = false;
    public Vector2 TimesLookingAtUserSpeaking = new Vector2(500, 1000);
    Priority UserLookingAtRobotMimickPriority = Priority.Low;
    Priority WaitingAfterSquareDialogActPriority = Priority.Medium;
    //public bool AlwaysMimicLookingAtRobot = false;
    public Vector2 TimesLookingAtRobotMimick= new Vector2();
    Priority UserLookingAtQuandrantPriority = Priority.Low;
    //public bool AlwaysLookingAtQuadrant = false;
    public Vector2 TimesLookingAtQuadrant = new Vector2();
    //Priority UserDistractedMimick;
    //public bool AlwaysMimicDistracted = false;
    //public Vector2 TimesLookingAtDistractedMimick;

    [Header("Condition 1 and 2")]
    public Vector2 TimesAutoPlayer = new Vector2();
    public double ProbabilityAutoPlayer = 0.5;
    public Vector2 TimesAutoQuadrant = new Vector2();
    public double ProbabilityAutoQuadrant = 0.4;
    public List<Vector3> DistractedPointsToLookAt = new List<Vector3>();
    public Vector2 TimesAutoDistracted = new Vector2();
    public Vector2 TimesBlockedAfterSquareDialogAct = new Vector2();

    [Header("Blinking")]
    public bool blinkingActivated;
    public Vector2 TimesBlinking = new Vector2();

    //Look at random quadrant
    //Look at player
    //Look at random distracted point
    private Dictionary<GazeState, Priority> Priorities = new Dictionary<GazeState, Priority>();
    private GazeState currentGazeTarget;

    public static AutonomousGazeBehavior Instance;

    //private IEnumerator blinkingCoroutine;
    private Coroutine blinkCoroutine;
    private Coroutine autoGazeCoroutine;
    private Coroutine keepTrackingPlayerCoroutine;
    private Coroutine keepCompleteMimickry;
    private Vector3 playerPosition;
    private bool robotStoppedDialogAct = false;

    private bool blockedFromLookingAtTheBoard = false;
    private string lastMessage;

    public bool BlockedFromLookingAtTheBoard
    {
        get
        {
            return blockedFromLookingAtTheBoard;
        }

        set
        {
            blockedFromLookingAtTheBoard = value;
        }
    }

    //private bool playerSpeaking = false;

    private bool isRobotStarted(GazeState gazeState)
    {
        switch (gazeState)
        {
            case GazeState.ROBOT_DISTRACTED:
                return true;
            case GazeState.ROBOT_LOOKING_AT_PLAYER:
                return true;
            case GazeState.ROBOT_LOOKING_AT_QUADRANT:
                return true;
            case GazeState.EVENT_PLAYER_STARTED_SPEAKING:
                return false;
            case GazeState.EVENT_PIECE_MOVED:
                return false;
            case GazeState.EVENT_PLAYER_LOOKING_AT_QUADRANT:
                return false;
            case GazeState.DIALOG_ACT:
                return false;
            case GazeState.EVENT_PLAYER_LOOKING_AT_ROBOT:
                return false;
            default:
                return false;
        }
    }
    //public void UserStoppedSpeaking()
    //{
    //    playerSpeaking = false;
    //}


    public void EventUserLookingAtQuadrantOrPlayer(string target)
    {
        if (Settings.Condition != JointAttentionCondition.Full) return;
        if (furhat.Asleeep) return;
        if (target == lastMessage) return;
        if (target == "") return;
        GazeState targetGaze = target == "Robot" ? GazeState.EVENT_PLAYER_LOOKING_AT_ROBOT : GazeState.EVENT_PLAYER_LOOKING_AT_QUADRANT;
        if (Priorities[targetGaze] >= Priorities[currentGazeTarget])
        {
            if (targetGaze == GazeState.EVENT_PLAYER_LOOKING_AT_ROBOT)
            {
                BlockedFromLookingAtTheBoard = false;
                performGaze(targetGaze, "");
            }
            else
            {
                performGaze(targetGaze, target);
            }
        }
        lastMessage = target;
    }

    public void EventUserStartedSpeaking()
    {
        if (Settings.Condition != JointAttentionCondition.Full) return;
        if (furhat.Asleeep) return;
        BlockedFromLookingAtTheBoard = false;
        if (Priorities[GazeState.EVENT_PLAYER_STARTED_SPEAKING] >= Priorities[currentGazeTarget])
        {
            if (ProbabilityUserSpeaking >= UnityEngine.Random.value)
            {
                
                performGaze(GazeState.EVENT_PLAYER_STARTED_SPEAKING, "");
            }

        }
    }

    public void EventPieceMoved(string pieceName)
    {
        if (Settings.Condition != JointAttentionCondition.Full) return;
        if (furhat.Asleeep) return;
        BlockedFromLookingAtTheBoard = false;
        if (Priorities[GazeState.EVENT_PIECE_MOVED] >= Priorities[currentGazeTarget])
        {
            if (ProbabilityToLookAtBoardEvents >= UnityEngine.Random.value)
            {
                performGaze(GazeState.EVENT_PIECE_MOVED, pieceName);
            }
        }
    }

    public void performGaze(GazeState gazeState, string target)
    {
        if (furhat.Asleeep) return;
        if (keepTrackingPlayerCoroutine != null) StopCoroutine(keepTrackingPlayerCoroutine);
        if (autoGazeCoroutine != null) StopCoroutine(autoGazeCoroutine);
        currentGazeTarget = gazeState;
        Dbg.Log(LogMessageType.ROBOT_GAZE_TARGET, new List<string>() { gazeState.ToString(), target });
        switch (gazeState)
        {
            case GazeState.ROBOT_DISTRACTED:
                furhatInterfaceGaze.SetGaze(DistractedGazeTarget.transform.position);
                furhat.Gaze(DistractedPointsToLookAt[UnityEngine.Random.Range(0, DistractedPointsToLookAt.Count)]);
                autoGazeCoroutine = StartCoroutine(WaitForAutoGazeTest(TimesAutoDistracted));
                break;
            case GazeState.ROBOT_LOOKING_AT_PLAYER:
                furhatInterfaceGaze.SetGaze(PlayerGazeTarget.transform.position);
                keepTrackingPlayerCoroutine = StartCoroutine(keepTrackingPlayer());
                autoGazeCoroutine = StartCoroutine(WaitForAutoGazeTest(TimesAutoPlayer));
                break;
            case GazeState.ROBOT_LOOKING_AT_QUADRANT:
                furhatInterfaceGaze.SetGazeToQuadrant(target);
                furhat.gaze(target);
                autoGazeCoroutine = StartCoroutine(WaitForAutoGazeTest(TimesAutoQuadrant));
                break;
            case GazeState.EVENT_PLAYER_STARTED_SPEAKING:
                furhatInterfaceGaze.SetGaze(PlayerGazeTarget.transform.position);
                keepTrackingPlayerCoroutine = StartCoroutine(keepTrackingPlayer());
                autoGazeCoroutine = StartCoroutine(WaitForAutoGazeTest(TimesLookingAtUserSpeaking));
                break;
            case GazeState.EVENT_PIECE_MOVED:
                furhatInterfaceGaze.SetGazeToPiece(target);
                furhat.gaze(target);
                autoGazeCoroutine = StartCoroutine(WaitForAutoGazeTest(TimesBoardEvents));
                break;
            case GazeState.EVENT_PLAYER_LOOKING_AT_QUADRANT:
                furhatInterfaceGaze.SetGazeToPiece(target);
                furhat.gaze(target);
                autoGazeCoroutine = StartCoroutine(WaitForAutoGazeTest(TimesLookingAtQuadrant));
                break;
            case GazeState.EVENT_PLAYER_LOOKING_AT_ROBOT:
                furhatInterfaceGaze.SetGaze(PlayerGazeTarget.transform.position);
                keepTrackingPlayerCoroutine = StartCoroutine(keepTrackingPlayer());
                autoGazeCoroutine = StartCoroutine(WaitForAutoGazeTest(TimesLookingAtRobotMimick));
                break;
            default:
                break;
        }

    }

    private IEnumerator keepTrackingPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            Vector3 newPlayerPosition = playerPosition;
            newPlayerPosition.y = newPlayerPosition.y - 0.1f;
            furhat.Gaze(newPlayerPosition);
        }
    }

    public void UpdatePlayerPosition(Vector3 vector3)
    {
        //Dbg.Log(LogMessageType.PLAYER_POSITION, vector3.ToString());
        playerPosition = vector3;
    }

    private void AutomaticGazeTest()
    {
        if(currentGazeTarget == GazeState.EVENT_PIECE_MOVED)
        {
            if(lastMessage == "Robot")
            {
                performGaze(GazeState.ROBOT_LOOKING_AT_PLAYER, "");
                return;
            }
        }
        if (blockedFromLookingAtTheBoard)
        {
            autoGazeCoroutine = StartCoroutine(WaitForAutoGazeTest(TimesBlockedAfterSquareDialogAct));
            currentGazeTarget = GazeState.WAITING_AFTER_SQUARE_DIALOG_ACT;
            blockedFromLookingAtTheBoard = false;
            return;
        }
        float randomValue = UnityEngine.Random.value;
        if(currentGazeTarget == GazeState.DIALOG_ACT)
            performGaze(GazeState.ROBOT_LOOKING_AT_PLAYER, "");
        else if (ProbabilityAutoQuadrant >= randomValue)
        {
            string[] quadrants = { "Q1Options", "Q2Options", "Q3Options", "Q4Options" };
            string randomQuadrant = quadrants[UnityEngine.Random.Range(0, quadrants.Length)];
            performGaze(GazeState.ROBOT_LOOKING_AT_QUADRANT, randomQuadrant);
        }
        else if (ProbabilityAutoPlayer + ProbabilityAutoQuadrant >= randomValue)
            performGaze(GazeState.ROBOT_LOOKING_AT_PLAYER, "");
        else performGaze(GazeState.ROBOT_DISTRACTED, "");
    }

    public void RobotStoppedDialogAct()
    {
        robotStoppedDialogAct = true;
    }

    private void Update()
    {
        if (robotStoppedDialogAct)
        {
            robotStoppedDialogAct = false;
            AutomaticGazeTest();
        }
    }

    IEnumerator WaitForAutoGazeTest(Vector2 timeToWait)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(timeToWait.x, timeToWait.y + 1) / 1000f);
        AutomaticGazeTest();
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

    private void Start()
    {
        playerPosition = new Vector3(0, 0, 1);
        furhatInterfaceGaze = transform.GetComponent<GazeDrawing>();
        furhat = Furhat.Instance;
        PlayerGazeTarget = GameObject.Find("Player");
        DistractedGazeTarget = GameObject.Find("DistractedImageRobot");
        blinkCoroutine = StartCoroutine(Blink());
        //StartCoroutine(blinkingCoroutine);
        Priorities.Add(GazeState.DIALOG_ACT, DialogActsPriority);
        Priorities.Add(GazeState.EVENT_PIECE_MOVED, BoardEventsPriority);
        Priorities.Add(GazeState.EVENT_PLAYER_STARTED_SPEAKING, UserSpeakingPriority);
        Priorities.Add(GazeState.EVENT_PLAYER_LOOKING_AT_QUADRANT, UserLookingAtQuandrantPriority);
        Priorities.Add(GazeState.EVENT_PLAYER_LOOKING_AT_ROBOT, UserLookingAtRobotMimickPriority);
        Priorities.Add(GazeState.WAITING_AFTER_SQUARE_DIALOG_ACT, WaitingAfterSquareDialogActPriority);
        //Priorities.Add(GazeState.EVENTUSERDISTRACTED, Priority.Lowest);
        Priorities.Add(GazeState.ROBOT_LOOKING_AT_QUADRANT, Priority.None);
        Priorities.Add(GazeState.ROBOT_LOOKING_AT_PLAYER, Priority.None);
        Priorities.Add(GazeState.ROBOT_DISTRACTED, Priority.None);
    }

    public void GazeAtUserStartInteraction()
    {
        performGaze(GazeState.ROBOT_LOOKING_AT_PLAYER, "");
    }

    internal void DialogChoicePiece(string name)
    {
        currentGazeTarget = GazeState.DIALOG_ACT;
        if (keepTrackingPlayerCoroutine != null) StopCoroutine(keepTrackingPlayerCoroutine);
        if (autoGazeCoroutine != null) StopCoroutine(autoGazeCoroutine);
        furhatInterfaceGaze.SetGazeToPiece(name);
        Dbg.Log(LogMessageType.ROBOT_GAZE_TARGET, new List<string>() { GazeState.DIALOG_ACT.ToString(), DialogType.Piece.ToString() });
        furhat.gaze(name);
    }

    internal void DialogChoicePlayer(bool withAutoTracking)
    {
        currentGazeTarget = GazeState.DIALOG_ACT;
        if (keepTrackingPlayerCoroutine != null) StopCoroutine(keepTrackingPlayerCoroutine);
        if (autoGazeCoroutine != null) StopCoroutine(autoGazeCoroutine);
        furhatInterfaceGaze.SetGaze(PlayerGazeTarget.transform.position);
        Dbg.Log(LogMessageType.ROBOT_GAZE_TARGET, new List<string>() { GazeState.DIALOG_ACT.ToString(), DialogType.Player.ToString() });
        if(withAutoTracking)
            keepTrackingPlayerCoroutine = StartCoroutine(keepTrackingPlayer());
        else Furhat.Instance.Gaze(0, 0, 1);
    }

    IEnumerator Blink()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(TimesBlinking.x, TimesBlinking.y + 1) / 1000f);
            if (!furhat.Asleeep && blinkingActivated)
                furhat.blink();
        }
    }

    public void DeactivateBlinking()
    {
        StopCoroutine(blinkCoroutine);
    }
}
