using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PieceColor { Blue, Green, Purple, Black, Yellow, Orange, None }
public enum DialogType { Piece, Player, Hint }

public enum PieceSpeechActions { GOOD_MOVE, BAD_MOVE, SHOULD_CHANGE}
public enum PieceGazeActions { GAZE, SHARE_GAZE }

public class DialogInfo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    private bool dialogInfoEnabled = false;

    Material BlueMaterial, BlackMaterial, OrangeMaterial, PurpleMaterial, GreenMaterial, YellowMaterial, GreyMaterial;

    private PieceColor color;

    public PieceColor MyColor
    {
        get { return color; }
        set {
            color = value;
            updateColor(color);
        }
    }

    private Material ReturnMaterial(PieceColor pieceColor)
    {
        switch (pieceColor)
        {
            case PieceColor.Blue:
                return BlueMaterial;
            case PieceColor.Green:
                return GreenMaterial;
            case PieceColor.Purple:
                return PurpleMaterial;
            case PieceColor.Black:
                return BlackMaterial;
            case PieceColor.Yellow:
                return YellowMaterial;
            case PieceColor.Orange:
                return OrangeMaterial;
            case PieceColor.None:
                return GreyMaterial;
            default:
                return GreyMaterial;
        }
    }

    public DialogType Type;
    public List<string> options;
    public List<Vector3> buttonPositions;

    private List<GameObject> buttonObjects = new List<GameObject>();
    internal static string LastColorUsed;

    private void Awake()
    {
        BlueMaterial = (Material)Resources.Load("BLUE", typeof(Material));
        BlackMaterial = (Material)Resources.Load("BLACK", typeof(Material));
        OrangeMaterial = (Material)Resources.Load("ORANGE", typeof(Material));
        PurpleMaterial = (Material)Resources.Load("PURPLE", typeof(Material));
        GreenMaterial = (Material)Resources.Load("GREEN", typeof(Material));
        YellowMaterial = (Material)Resources.Load("YELLOW", typeof(Material));
        GreyMaterial = (Material)Resources.Load("Grey", typeof(Material));
    }


    // Use this for initialization
    void Start()
    {
    }

    public void InstantiateWizardButtons()
    {
        if (options.Count > 0 && !dialogInfoEnabled)
        {   
            foreach(var item in GameObject.FindObjectsOfType<DialogInfo>())
            {
                item.CloseDialogOptions();
            }
            dialogInfoEnabled = true;
            //InstantiateWizardButton(-1);
            for (int i = 0; i < options.Count; i++)
            {
                
                InstantiateWizardButton(i);
            }
        }
    }

    private void InstantiateWizardButton(int positionInArray)
    {
        GameObject buttonObject = (GameObject)Instantiate(Resources.Load("WizardButton"));
        buttonObject.GetComponent<WizardButtonPressed>().Source = Type;
        if (Type == DialogType.Piece)
        {
            buttonObject.GetComponent<WizardButtonPressed>().pieceName = name;
            LastColorUsed = color.ToString();
        }
        if(name == "Hint")
            buttonObject.GetComponent<WizardButtonPressed>().pieceName = transform.parent.name;

        //Debug.Log("!!!!!!!!!!!!!!!!!!" + buttonObject.GetComponent<WizardButtonPressed>().pieceName + "  ??????" + Type);
        buttonObject.transform.SetParent(gameObject.transform);
        buttonObject.transform.position = transform.position - new Vector3(0,0,-100);

        Text text = buttonObject.GetComponentInChildren<Text>();
        if (positionInArray != -1)
        {
            buttonObject.transform.localPosition = buttonPositions[positionInArray];
            buttonObject.transform.localScale = new Vector3(1, 1, 2);
            text.text = options[positionInArray].ToString();
        }
        //else //TODO: Hacky way to allow the gaze, change this!
        //{
        //    buttonObject.transform.localPosition = new Vector3(0, 0, 0);
        //    buttonObject.transform.localScale = new Vector3(1, 1, 1);
        //    if (!(Source == DialogSource.Robot))
        //        text.text = "Gaze";
        //    else text.text = "Clear Gaze";
        //}

        buttonObject.name = transform.name;
        buttonObject.transform.SetParent(transform.root);
        buttonObjects.Add(buttonObject);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void updateColor(PieceColor color)
    {
        //if(Source!= DialogGazeTarget.Robot)
        this.color = color;
        GetComponent<Image>().material = ReturnMaterial(color);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if(dialogInfoEnabled)
                CloseDialogOptions();
            //ScreenMouseRay();
            else InstantiateWizardButtons();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }

    /// <summary>
        /// Cast a ray from the mouse to the target object
        /// Then sets the target position of the ability to that object.
        /// </summary>
    //public void ScreenMouseRay()
    //{
    //    var pointer = new PointerEventData(EventSystem.current);
    //    if (Display.displays.Length > 1)
    //        pointer.position = new Vector2(Input.mousePosition.x + 1920, Input.mousePosition.y);
    //    else
    //        pointer.position = Input.mousePosition;
    //    var raycastResults = new List<RaycastResult>();
    //    EventSystem.current.RaycastAll(pointer, raycastResults);

    //    if (raycastResults.Count > 0)
    //    {
    //        Debug.Log("!!!!!!!!" + name);
    //        GameObject touchedObject = raycastResults[0].gameObject;
    //        WizardButtonPressed wizardButtonPressed = touchedObject.GetComponent<WizardButtonPressed>();
    //        if (wizardButtonPressed != null)
    //        {
    //            if (Type == DialogType.Piece)
    //            {
    //                LastColorUsed = this.color.ToString();
    //            }
    //            string text = touchedObject.GetComponentInChildren<Text>().text;
    //            if (name == "Hint")
    //                wizardButtonPressed.ExecuteAction(transform.parent.name);
    //            else wizardButtonPressed.ExecuteAction(transform.name);


    //            //WizardManager wm = GameObject.Find("WizardManagement").GetComponent<WizardManager>();
    //            //switch (Source)
    //            //{
    //            //    case DialogSource.Piece:
    //            //        wm.DialogChoicePiece(name, text);
    //            //        break;
    //            //    case DialogSource.Player:
    //            //        wm.DialogChoicePlayer(text);
    //            //        break;
    //            //    case DialogSource.Robot:
    //            //        wm.DialogChoiceRobot(text);
    //            //        break;
    //            //}
    //        }
    //    }

    //    CloseDialogOptions();
    //}

    public void CloseDialogOptions()
    {
        foreach (var item in buttonObjects)
        {
            Destroy(item);
        }
        dialogInfoEnabled = false;
    }
}
