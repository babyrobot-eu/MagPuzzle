using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using Crosstales.RTVoice;

public class SpeechRecognition : MonoBehaviour {

    [SerializeField]
    private Text m_speechBox;
    public static SpeechRecognition Instance;

    private bool stopSpeechRecognition = false;
    private bool startSpeechRecognition = false;
    private bool waitingForFinalResult = false;


    private DictationRecognizer m_DictationRecognizer;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        m_DictationRecognizer = new DictationRecognizer();
        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            waitingForFinalResult = false;
            Dbg.Log(LogMessageType.SPEECH_REC_FINAL, text);
            //AutoGazeBehavior.Instance.EventUserStartedSpeaking();
            if (m_speechBox != null)
            {
                m_speechBox.text = text;
            }
            Speaker.Speak(text, null, null,true,2,1,1,"");
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            if (!waitingForFinalResult)
            {
                waitingForFinalResult = true;
                AutonomousGazeBehavior.Instance?.EventUserStartedSpeaking();
            }
            Dbg.Log(LogMessageType.SPEECH_REC_HYPOTHESIS, text);
            if (m_speechBox)
            {
                m_speechBox.text = text + "...";
            }
  
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
            {
            //    Debug.Log("Dictation Stopped");
                
                                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
            }
          //  StartSpeechRecognition();
        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };

        m_DictationRecognizer.AutoSilenceTimeoutSeconds = 600;
        m_DictationRecognizer.InitialSilenceTimeoutSeconds = 600;
        //m_DictationRecognizer.Start();
    }

    private void Update()
    {
        if (startSpeechRecognition)
        {
            startSpeechRecognition = false;
            m_DictationRecognizer.Start();
        }
        if (stopSpeechRecognition)
        {
            stopSpeechRecognition = false;
            m_DictationRecognizer.Stop();
        }
    }



    public void StartSpeechRecognition()
    {
        startSpeechRecognition = true;
    }

    public void StopSpeechRecognition()
    {
        stopSpeechRecognition = true;
    }

    private void OnApplicationQuit()
    {
        if(m_DictationRecognizer.Status == SpeechSystemStatus.Running)
            m_DictationRecognizer.Stop();
    }
}
