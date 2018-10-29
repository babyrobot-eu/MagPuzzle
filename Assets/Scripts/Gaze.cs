using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Gaze : MonoBehaviour {

    //car to store mouse position on the screen
    private Vector3 mousePos;
    //assign a material to the Line Renderer in the Inspector
    public Material material;
    public float thickness = 0.05f;

    public LineRenderer lineLeftEye;
    public LineRenderer lineRightEye;

    private Vector3 gazeTarget = new Vector3();
    private string activeQuadrant = "";

    private bool disabled = true;

    public void ClearGaze()
    {
        lineLeftEye.enabled = false;
        lineRightEye.enabled = false;
    }

    public void SetGaze(Vector3 target)
    {
        if (disabled) return;
        gazeTarget = target;
        lineLeftEye.SetPosition(1, new Vector3(target.x, target.y));
        lineRightEye.SetPosition(1, new Vector3 (target.x,target.y));
        lineLeftEye.enabled = true;
        lineRightEye.enabled = true;
    }

    public void SetGazeToPiece(string piece)
    {
        if (disabled) return;
        Vector3 position = GameObject.Find(piece).transform.position;
        SetGaze(position);
    }

    public void SetGazeToQuadrant(string piece)
    {
        if (disabled) return;
        DisableQuandrantImages();
        if (piece != "")
        {
            GameObject pieceObject = GameObject.Find(piece);
            SetGaze(pieceObject.transform.position);
            pieceObject.GetComponent<Image>().enabled = true;
        }
        if (piece != "Robot")
        {
            activeQuadrant = piece;

        }
        else activeQuadrant = "";

    }

    public static void DisableQuandrantImages()
    {
        GameObject.Find("Q1").GetComponent<Image>().enabled = false;
        GameObject.Find("Q2").GetComponent<Image>().enabled = false;
        GameObject.Find("Q3").GetComponent<Image>().enabled = false;
        GameObject.Find("Q4").GetComponent<Image>().enabled = false;
        GameObject.Find("Robot").GetComponent<Image>().enabled = false;
    }

    internal bool isDisabled()
    {
        return disabled;
    }

    public void SetupGazeLines(bool disabled)
    {
        this.disabled = disabled;
        Vector3 pos = lineRightEye.transform.position;
        lineRightEye.SetPosition(0, new Vector3(pos.x, pos.y, 0));
        lineRightEye.material = material;
        lineRightEye.startWidth = thickness;
        lineRightEye.endWidth = thickness;
        lineRightEye.useWorldSpace = true;
        lineRightEye.positionCount = 2;
        //lineRightEye.SetVertexCount(2);
        //Color color = new Color(255, 255, 255, 100);
        pos = lineLeftEye.transform.position;
        lineLeftEye.SetPosition(0, new Vector3(pos.x, pos.y, 0));
        lineLeftEye.material = material;
        lineLeftEye.startWidth = thickness;
        lineLeftEye.endWidth = thickness;
        lineLeftEye.useWorldSpace = true;
        lineLeftEye.positionCount = 2;
        //lineLeftEye.SetVertexCount(2);
        ClearGaze();
    }

    public Vector3 GetGazeTarget()
    {
        return gazeTarget;
    }

    public String GetActiveQuadrant()
    {
        return activeQuadrant;
    }
}
