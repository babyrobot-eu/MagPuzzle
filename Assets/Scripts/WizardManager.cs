using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public enum PieceSquares { A1 = 0, A2 = 1, A3 = 2, A4 = 3, B1 = 4, B2 = 5, B3 = 6, B4 = 7, C1 = 8, C2 = 9, C3 = 10, C4 = 11, D1 = 12, D2 = 13, D3 = 14, D4 = 15}


public class PieceInfo
{
    public DateTime dateTime;
    public PieceColor pieceColor;
    public float distance;

    public PieceInfo(DateTime dateTime, PieceColor pieceColor, float distance)
    {
        this.dateTime = dateTime;
        this.pieceColor = pieceColor;
        this.distance = distance;
    }
}

public class WizardManager : MonoBehaviour {
    public static WizardManager Instance;
    private const int NUMBER_OF_SQUARES = 16;
    public double MaxSlackDistance = 0.05;
    private GameObject BlueCube, PurpleCube, YellowCube, BlackCube, GreenCube, OrangeCube;

    public int TimeToLoseAPieceMS = 1000;
    List<Image> hintImages = new List<Image>();
    List<List<int>> neighbours = new List<List<int>>();
    List<Vector3> piecePositionsFromCamera = new List<Vector3>();
    List<PieceInfo> positions = new List<PieceInfo>();
    List<Tuple<GameObject, PieceColor>> possiblePieces = new List<Tuple<GameObject, PieceColor>>();
    private List<Tuple<Image, DialogInfo>> pieces = new List<Tuple<Image, DialogInfo>>();
    public static string LastUtteranceVocalized = "";

    private void Start()
    {
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();

    }

    void Awake() {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }



        //markers = FindObjectsOfType(typeof(ARMarker)) as ARMarker[];
        LoadCalibration();
        FillNeighbors();
        FillInitialPositions();
        BlueCube = GameObject.Find("BlueCube");
        PurpleCube = GameObject.Find("PurpleCube");
        YellowCube = GameObject.Find("YellowCube");
        BlackCube = GameObject.Find("BlackCube");
        GreenCube = GameObject.Find("GreenCube");
        OrangeCube = GameObject.Find("OrangeCube");
        possiblePieces.Add(new Tuple<GameObject, PieceColor>(BlueCube, PieceColor.Blue));
        possiblePieces.Add(new Tuple<GameObject, PieceColor>(PurpleCube, PieceColor.Purple));
        possiblePieces.Add(new Tuple<GameObject, PieceColor>(YellowCube, PieceColor.Yellow));
        possiblePieces.Add(new Tuple<GameObject, PieceColor>(BlackCube, PieceColor.Black));
        possiblePieces.Add(new Tuple<GameObject, PieceColor>(GreenCube, PieceColor.Green));
        possiblePieces.Add(new Tuple<GameObject, PieceColor>(OrangeCube, PieceColor.Orange));

        for (int i = 0; i < NUMBER_OF_SQUARES; i++)
        {
            GameObject piece = GameObject.Find(((PieceSquares)i).ToString());
            Image image = piece.GetComponent<Image>();
            hintImages.Add(piece.transform.Find("Hint").GetComponent<Image>());
            DialogInfo info = piece.GetComponent<DialogInfo>();
            info.MyColor = PieceColor.None;
            pieces.Add(new Tuple<Image, DialogInfo>(image, info));
            image.enabled = false;
        }
    }


    private void FillInitialPositions()
    {
        for (int i = 0; i < NUMBER_OF_SQUARES; i++)
        {
            var now = DateTime.Now;
            positions.Add(new PieceInfo(now, PieceColor.None, float.MaxValue));
        }
    }

    private void LoadCalibration()
    {
        using (StreamReader sr = new StreamReader("calibration.csv"))
        {
            string line = "";
            piecePositionsFromCamera.Clear();
            while ((line = sr.ReadLine()) != null)
            {
                var positions = line.Split(',');
                piecePositionsFromCamera.Add(new Vector3(float.Parse(positions[0]), float.Parse(positions[1]), float.Parse(positions[2])));
            }
        }
    }

    private void SaveCalibration()
    {
        using (StreamWriter sw = new StreamWriter("calibration.csv",false))
        {
            foreach (var item in piecePositionsFromCamera)
            {
                sw.WriteLine(item.x + "," + item.y + "," + item.z);
            }
        }
    }

    public void CalibratePiecePositions()
    {
        //The piece with the greater x and lower y is going to be the one on the bottom left corner

        List<Tuple<GameObject, Vector3>> piecesPositions = new List<Tuple<GameObject, Vector3>>();

        foreach (var item in possiblePieces)
        {
            if (item.Item1.activeInHierarchy)
            {
                ARMarker myMarker = item.Item1.transform.parent.GetComponent<ARTrackedObject>().GetMarker();
                Matrix4x4 pose = myMarker.TransformationMatrix;
                Vector3 position = ARUtilityFunctions.PositionFromMatrix(pose);
                piecesPositions.Add(new Tuple<GameObject, Vector3>(item.Item1, position));
            }
        }
        if (piecesPositions.Count == 6)
        {
            //Sort the positions by x
            piecesPositions.Sort((x, y) => y.Item2.x.CompareTo(x.Item2.x));
            Vector3 bottomLeft, bottomRight, topLeft, topRight, middleLeft, middleRight;
            if (piecesPositions[0].Item2.y < piecesPositions[1].Item2.y)
            {
                bottomLeft = piecesPositions[0].Item2;
                bottomRight = piecesPositions[1].Item2;
            }
            else
            {
                bottomLeft = piecesPositions[1].Item2;
                bottomRight = piecesPositions[0].Item2;
            }
            if (piecesPositions[4].Item2.y < piecesPositions[5].Item2.y)
            {
                topLeft = piecesPositions[4].Item2;
                topRight = piecesPositions[5].Item2;
            }
            else
            {
                topLeft = piecesPositions[5].Item2;
                topRight = piecesPositions[4].Item2;
            }
            if (piecesPositions[2].Item2.y < piecesPositions[3].Item2.y)
            {
                middleLeft = piecesPositions[2].Item2;
                middleRight = piecesPositions[3].Item2;
            }
            else
            {
                middleLeft = piecesPositions[3].Item2;
                middleRight = piecesPositions[2].Item2;
            }
            piecePositionsFromCamera.Clear();
            piecePositionsFromCamera.Add(bottomLeft);
            piecePositionsFromCamera.Add(new Vector3(middleLeft.x, bottomLeft.y, (bottomLeft.z + middleLeft.z)/2 ));
            piecePositionsFromCamera.Add(new Vector3(middleRight.x, topLeft.y,(topLeft.z + middleRight.z) / 2));
            piecePositionsFromCamera.Add(topLeft);
            piecePositionsFromCamera.Add(new Vector3(bottomLeft.x, middleLeft.y, (middleLeft.z + bottomLeft.z) / 2));
            piecePositionsFromCamera.Add(middleLeft);
            piecePositionsFromCamera.Add(new Vector3(middleRight.x, middleLeft.y, (middleLeft.z + middleRight.z) / 2));
            piecePositionsFromCamera.Add(new Vector3(topLeft.x, middleLeft.y, (middleLeft.z + topLeft.z) / 2));
            piecePositionsFromCamera.Add(new Vector3(bottomRight.x, middleRight.y, (middleRight.z + bottomRight.z) / 2));
            piecePositionsFromCamera.Add(new Vector3(middleLeft.x, middleRight.y, (middleRight.z + middleLeft.z) / 2));
            piecePositionsFromCamera.Add(middleRight);
            piecePositionsFromCamera.Add(new Vector3(topRight.x, middleRight.y, (middleRight.z + topRight.z) / 2));
            piecePositionsFromCamera.Add(bottomRight);
            piecePositionsFromCamera.Add(new Vector3(middleLeft.x, bottomRight.y, (bottomRight.z + middleLeft.z) / 2));
            piecePositionsFromCamera.Add(new Vector3(middleRight.x, topRight.y, (topRight.z + middleRight.z) / 2));
            piecePositionsFromCamera.Add(topRight);

            SaveCalibration();
        }
        else Debug.Log("Did not find 6 pieces to calibrate!");
    }

    private void FillNeighbors()
    {
        neighbours.Add(new List<int>() { 1, 4 }); //A1
        neighbours.Add(new List<int>() { 0, 2, 5}); //A2
        neighbours.Add(new List<int>() { 1, 3, 6 }); //A3
        neighbours.Add(new List<int>() { 2, 7 }); //A4
        neighbours.Add(new List<int>() { 0, 5, 8}); //B1
        neighbours.Add(new List<int>() {1, 4, 6, 9 }); //B2
        neighbours.Add(new List<int>() {2, 5, 7, 10 }); //B3
        neighbours.Add(new List<int>() {3, 6, 11 }); //B4
        neighbours.Add(new List<int>() {4, 9, 12 }); //C1
        neighbours.Add(new List<int>() {5, 8, 10, 13 }); //C2
        neighbours.Add(new List<int>() {6, 9, 11, 14 }); //C3
        neighbours.Add(new List<int>() { 7, 10, 15 }); //C4
        neighbours.Add(new List<int>() { 8, 13 }); //D1
        neighbours.Add(new List<int>() { 9, 12, 14}); //D2
        neighbours.Add(new List<int>() { 10, 13, 15 }); //D3
        neighbours.Add(new List<int>() { 11, 14 }); //D4
    }

    internal void ClearHints()
    {
        foreach (var item in hintImages)
        {
            item.enabled = false;
        }
    }

    private void OnApplicationQuit()
    {
        //dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
        //dictationRecognizer.Dispose();
    }

    // Update is called once per frame
    void Update () {

        foreach (var item in possiblePieces)
        {

//            Debug.Log("x: " + (Origin.transform.position.x - item.Item1.transform.position.x) +
//" y: " + (Origin.transform.position.y - item.Item1.transform.position.y) +
//" z: " + (Origin.transform.position.z - item.Item1.transform.position.z));
            if (item.Item1.activeInHierarchy)
            {
                ARMarker myMarker = item.Item1.transform.parent.GetComponent<ARTrackedObject>().GetMarker();
                Matrix4x4 pose = myMarker.TransformationMatrix;
                Vector3 position = ARUtilityFunctions.PositionFromMatrix(pose);
                //Quaternion orientation = ARUtilityFunctions.QuaternionFromMatrix(pose);
                //float x = position.x - distanceToOrigin[5].x;
                //float y = position.y - distanceToOrigin[5].y;
                //float z = position.z - distanceToOrigin[5].z;
                //Debug.Log("x: " + x +
                //" y: " + y +
                //" z: " + z);

                var minDistance = float.MaxValue;
                int minIndex = -1;
                for (int i = 0; i < NUMBER_OF_SQUARES; i++)
                {
                    var distance = Vector3.Distance(position, piecePositionsFromCamera[i]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minIndex = i;
                    }
                }

                if (minDistance < MaxSlackDistance)
                {
                    if (positions[minIndex].pieceColor != item.Item2)
                    {
                        if (minDistance < positions[minIndex].distance)
                        {
                            UpdatePosition(item.Item2, minDistance, minIndex);
                        }
                    }
                    else positions[minIndex].dateTime = DateTime.UtcNow;
                }
            }
        }
        for (int i = 0; i < positions.Count; i++)
        {
            if (positions[i].pieceColor != PieceColor.None)
            {
                if ((DateTime.UtcNow - positions[i].dateTime).TotalMilliseconds > TimeToLoseAPieceMS)
                {
                    UpdatePosition(PieceColor.None, float.MaxValue, i);
                }
            }
        }

        //for (int i = 0; i < NUMBER_OF_SQUARES; i++)
        //{
        //    if (pieces[i].Item2.MyColor == PieceColor.None)
        //    {
        //        bool hintExists = false;
        //        foreach (var item in neighbours[i])
        //        {
                    
        //            if (pieces[item].Item2.MyColor != PieceColor.None)
        //            {
        //                hintExists = true;
        //                hintImages[i].enabled = true;
        //            }
        //        }
        //        if (!hintExists)
        //        {
        //            hintImages[i].enabled = false;
        //        }
        //    } else if(hintImages[i].enabled == true)
        //    {
        //        hintImages[i].enabled = false;
        //    }
        //}

    }

    public void SetHint(int position, bool active)
    {
        hintImages[position].enabled = active;
    }

    private bool containsPiece(int positionNumber)
    {
        if (positionNumber > positions.Count || positionNumber < 0)
            return false;
        if (positions[positionNumber].pieceColor != PieceColor.None)
            return true;
        return false;
    }

    private void UpdatePosition(PieceColor color, float distance, int index)
    {
        string nameOfSquare = ((PieceSquares)index).ToString();
        PieceColor oldColor = positions[index].pieceColor;
        positions[index] = new PieceInfo(DateTime.UtcNow, color, distance);
        pieces[index].Item2.MyColor = color;
        if (color != PieceColor.None)
        {
            pieces[index].Item1.enabled = true;
            AutoGazeBehavior.Instance?.EventPieceMoved(nameOfSquare);
        }
        else pieces[index].Item1.enabled = false;
        if (color == PieceColor.None)
            Dbg.Log(LogMessageType.PIECE_LOST, new List<string>() { nameOfSquare, oldColor.ToString() });
        else
            Dbg.Log(LogMessageType.PIECE_FOUND, new List<string>() { nameOfSquare, color.ToString() });


        ////Transform the board positions into a matrix that is more efficient to perform calculations on
        //Vector4 fourthColumn = new Vector4(containsPiece(15), containsPiece(14), containsPiece(13), containsPiece(12));
        //Vector4 thirdColumn = new Vector4(containsPiece(11), containsPiece(10), containsPiece(9), containsPiece(8));
        //Vector4 secondColumn = new Vector4(containsPiece(7), containsPiece(6), containsPiece(5), containsPiece(4));
        //Vector4 firstColumn = new Vector4(containsPiece(3), containsPiece(2), containsPiece(1), containsPiece(0));
        //Matrix4x4 matrix4X4 = new Matrix4x4(firstColumn, secondColumn, thirdColumn, fourthColumn);

        ////Send it to the class responsible for managing buttons regarding the pieces
        MagPuzzleManager.Instance.BoardStateUpdated(GetBoardState());
    }

    public bool [] GetBoardState()
    {
        bool[] boardState = new bool[16];

        for (int i = 0; i < 16; i++)
        {
            boardState[i] = containsPiece(i);
        }
        return boardState;
    }

    internal void DialogChoicePiece(string name, string text)
    {
        if (!Furhat.Instance.robotInDialogAct)
        {
            AutoGazeBehavior.Instance.BlockedFromLookingAtTheBoard = true;
            Furhat.Instance.robotInDialogAct = true;
            AutoGazeBehavior.Instance.DialogChoicePiece(name);

            string dialogActText = Behaviors.Instance.SayRandomString(text);
            Dbg.Log(LogMessageType.DIALOG_ACT, new List<string>() {DialogType.Piece.ToString(), name, text, dialogActText});

        }
    }

    internal void DialogChoicePlayer(string text)
    {
        if (Furhat.Instance == null)
            return;
        if (!Furhat.Instance.robotInDialogAct)
        {
            AutoGazeBehavior.Instance.BlockedFromLookingAtTheBoard = false;
            Furhat.Instance.robotInDialogAct = true;
            string dialogActText = Behaviors.Instance.SayRandomString(text);
            if (dialogActText.Contains("gaze("))
                AutoGazeBehavior.Instance.DialogChoicePlayer(false);
            else AutoGazeBehavior.Instance.DialogChoicePlayer(true);
            Dbg.Log(LogMessageType.DIALOG_ACT, new List<string>() { DialogType.Player.ToString(), Settings.Instance.ParticipantName.text, text, dialogActText });
        }
    }

    public void RepeatLastUtterance()
    {
        AutoGazeBehavior.Instance.BlockedFromLookingAtTheBoard = false;
        Furhat.Instance.robotInDialogAct = true;
        Furhat.Instance.say(LastUtteranceVocalized, "");
        if (LastUtteranceVocalized.Contains("gaze("))
            AutoGazeBehavior.Instance.DialogChoicePlayer(false);
        else AutoGazeBehavior.Instance.DialogChoicePlayer(true);
        Dbg.Log(LogMessageType.DIALOG_ACT, new List<string>() { "REPEAT_LAST_UTTERANCE", LastUtteranceVocalized });
    }

}
