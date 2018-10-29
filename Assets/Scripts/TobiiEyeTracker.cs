using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TobiiEyeTracker : MonoBehaviour {
    private int currentX;
    private int currentY;

    // Use this for initialization
    void Start () {
        InvokeRepeating("LogEyeTracker",0, 0.1f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void LogEyeTracker()
    {
        Dbg.Log(LogMessageType.TOBII, new List<string>() { currentX.ToString(), currentY.ToString()});
    }
}
