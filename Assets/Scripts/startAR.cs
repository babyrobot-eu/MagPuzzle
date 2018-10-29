using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class startAR : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON.
        // Check if additional displays are available and activate each.
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
            

    }


    public void startAugmented() {
        ARController aRController = FindObjectOfType<ARController>();
        aRController.StartAR();
        //float sds = gameObject.transform.position.x;
        
    }
	// Update is called once per frame
	void Update () {
		
	}
}
