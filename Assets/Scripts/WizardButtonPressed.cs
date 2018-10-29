using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WizardButtonPressed : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private const int NumBlinkingSec = 5;
    private Image image;
    public DialogType Source;
    public Material blinkingMaterial;
    public Material originalMaterial;
    private Renderer rend;
    internal string pieceName;

    // Use this for initialization
    void Start()
    {
        rend = this.GetComponentInChildren<Renderer>();
        image = GetComponent<Image>();
        image.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = Color.white;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ExecuteAction();
    }

    public void ExecuteAction()
    {
        string text = GetComponentInChildren<Text>().text;
        WizardManager wm = GameObject.Find("WizardManagement").GetComponent<WizardManager>();
        MagPuzzleManager.Instance.ButtonPressed(this.gameObject);
        if (text == "Repeat")
        {
            wm.RepeatLastUtterance();
        }
        else
        switch (Source)
        {
            case DialogType.Piece:
                wm.DialogChoicePiece(pieceName, text);
                break;
            default:
                wm.DialogChoicePlayer(text);
                break;
        }
    }

    public void Blink()
    {
        StartCoroutine(BlinkCoroutine(NumBlinkingSec));
    }

    private IEnumerator BlinkCoroutine(int numSec)
    {
        var endTime = Time.time + numSec;
        while(Time.time < endTime)
        {
            image.material = blinkingMaterial;
            yield return new WaitForSeconds(0.15f);
            image.material = originalMaterial;
            yield return new WaitForSeconds(0.15f);
        }
    }
}
