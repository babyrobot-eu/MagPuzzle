using Crosstales.RTVoice;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum JointAttentionCondition { Control = 0, Full = 1, DISABLED = 2 }

public class Settings : MonoBehaviour {
    public InputField ParticipantName;
    public Button LeftCondition;
    public Button RightCondition;
    public Button Accept;
    public static JointAttentionCondition Condition;
    public static string SessionName = "";
    public static Settings Instance;
    public AudioClip wakeUpBeep;

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
        updateCurrentCondition(JointAttentionCondition.Full);
        //EnableWizardButtons(false);
    }

    private void Start()
    {
        // Display.displays[0] is the primary, default display and is always ON.
        // Check if additional displays are available and activate each.
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate(2560,1440,60);
        }
    }

    //public void EnableWizardButtons(bool enable)
    //{
    //    var buttons = FindObjectsOfType<WizardButtonPressed>();
    //    foreach (var item in buttons)
    //    {
    //        item.transform.GetComponent<Image>().enabled = enable;
    //    }
    //}

    public void SetupGazeLines(bool disabled)
    {
        var buttons = FindObjectsOfType<Gaze>();
        foreach (var item in buttons)
        {
            item.SetupGazeLines(disabled);
        }
    }

    public void StartSession()
    {
        if (ParticipantName.text != "")
        {
            Dbg.Instance.CreateLoggingFile(ParticipantName.text, Condition.ToString());
            //EnableWizardButtons(true);
            MagPuzzleManager.Instance.NewSession();
            SetupGazeLines(Condition == JointAttentionCondition.Control ? true : false);
            GetComponent<AudioSource>().Play();
            Furhat.Instance?.WakeUP();
            ParticipantName.interactable = false;
            LeftCondition.interactable = false;
            RightCondition.interactable = false;
            Accept.interactable = false;
            SpeechRecognition.Instance.StartSpeechRecognition();
        }
        else
        {
            Speaker.Speak("Can't start a session with an empty name!", null, null, true, 2, 1, 1, "");
            //Debug.LogError("Can't start a session with an empty name!");
        }
    }

    public void EndSession()
    {
        SpeechRecognition.Instance.StopSpeechRecognition();
        Dbg.Instance.Close();
        //EnableWizardButtons(false);
        Furhat.Instance?.FallAsleep();
        ParticipantName.interactable = true;
        LeftCondition.interactable = true;
        RightCondition.interactable = true;
        Accept.interactable = true;
        Gaze.DisableQuandrantImages();
    }

    public void shiftConditionLeft()
    {
        shiftCondition(true);
    }

    public void shiftConditionRight()
    {
        shiftCondition(false);
    }

    private void shiftCondition(bool left)
    {
        if (left)
        {
            if((int)Condition >0)
                updateCurrentCondition(--Condition);
        }
        else
        {
            if((int)Condition < 1)
                updateCurrentCondition(++Condition);
        }
    }

    private void updateCurrentCondition(JointAttentionCondition growthRate)
    {
        Condition = growthRate;
        Text text = GameObject.Find("Condition").GetComponentInChildren<Text>();
        switch (growthRate)
        {
            case JointAttentionCondition.Control:
                text.text = "1 - Control";
                break;
            case JointAttentionCondition.Full:
                text.text = "2 - Full Joint Attention";
                break;
        }
    }

}
