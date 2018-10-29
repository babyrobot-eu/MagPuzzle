using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Behaviors : MonoBehaviour {

    public static Behaviors Instance;
    public string behaviorFileName;
    System.Random randomGenerator;
    Dictionary<string, List<Behavior>> utteranceList;

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

    // Use this for initialization
    void Start()
    {
        randomGenerator = new System.Random();
        utteranceList = new Dictionary<string, List<Behavior>>();
        LoadBehaviors(behaviorFileName, utteranceList);
    }

    private int random(int minValue, int maxValue)
    {
        return randomGenerator.Next(minValue, maxValue);
    }

    private bool random(double probability)
    {
        var generated = randomGenerator.NextDouble();
        if (generated < probability)
            return true;
        else return false;
    }

    internal string SayRandomString(string str)
    {
        if (utteranceList.ContainsKey(str))
            return SayRandomString(utteranceList[str]);
        else return "";
    }

    internal string SayRandomString(List<Behavior> utteranceListAll)
    {
        double randomNumber = randomGenerator.NextDouble();

        int minNumTimesSaid = utteranceListAll.Min(x => x.numTimesSaid);
        List<Behavior> lessUsedUtteranceList = utteranceListAll.Where(x => x.numTimesSaid == minNumTimesSaid).ToList();

        double sumTupleWeights = 0;
        foreach (var item in lessUsedUtteranceList)
        {
            sumTupleWeights += item.weight;
        }
        randomNumber = randomNumber * sumTupleWeights;
        double valueSoFar = 0;
        foreach (var item in lessUsedUtteranceList)
        {
            valueSoFar += item.weight;
            if (valueSoFar >= randomNumber)
            {
                item.numTimesSaid++;
                string stringToSay = string.Format(item.text, Settings.Instance.ParticipantName.text, DialogInfo.LastColorUsed);
                Furhat.Instance.say(stringToSay, item.emotion);
                WizardManager.LastUtteranceVocalized = stringToSay;
                stringToSay += "-!-Emotion: " + item.emotion;
                return stringToSay;
            }
        }
        lessUsedUtteranceList[0].numTimesSaid++;
        string stringToSay2 = string.Format(lessUsedUtteranceList[0].text, Settings.Instance.ParticipantName.text, DialogInfo.LastColorUsed);
        Furhat.Instance.say(stringToSay2, lessUsedUtteranceList[0].emotion);
        WizardManager.LastUtteranceVocalized = stringToSay2;
        stringToSay2 += "-!-Emotion: " + lessUsedUtteranceList[0].emotion;
        return stringToSay2;
    }

    internal void LoadBehaviors(string filepath, Dictionary<string, List<Behavior>> utterances)
    {
        using (StreamReader sr = new StreamReader(filepath))
        {
            string line = sr.ReadLine(); //Discard the first line
            while ((line = sr.ReadLine()) != null)
            {
                string[] data = line.Split('\t');
                Behavior behavior = new Behavior(data[1], data[2], data[3], double.Parse(data[4]), 0);
                if (utterances.ContainsKey(data[0]))
                    utterances[data[0]].Add(behavior);
                else
                {
                    utterances[data[0]] = new List<Behavior>();
                    utterances[data[0]].Add(behavior);
                }
            }
        }
    }

    internal List<string> GetBlockOfStrings(string category, Dictionary<string, List<Behavior>> utteranceList)
    {
        List<string> toReturn = new List<string>();
        foreach (var item in utteranceList[category])
        {
            toReturn.Add(item.text);
        }
        return toReturn;
    }

}

public class Behavior
{
    public string text;
    public string emotion;
    public string gazeTarget;
    public double weight;
    public int numTimesSaid;

    public Behavior(string text, string gazeTarget, string emotion, double weight, int numTimesSaid)
    {
        this.text = text;
        this.emotion = emotion;
        this.gazeTarget = gazeTarget;
        this.weight = weight;
        this.numTimesSaid = numTimesSaid;
    }
}
