using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateCorrectness : MonoBehaviour {

    public static StateCorrectness Instance;
    public Sprite correctTexture, incorrectTexture;
    private Image currentImage;

    public List<string> optionsRight;
    public List<string> optionsWrong;

    // Use this for initialization
    void Start () {
        currentImage = this.GetComponent<Image>();
        Change(true);
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

    public void Change(bool correct)
    {
        if (correct)
        {
            GetComponentInChildren<DialogInfo>().options = optionsRight;
            currentImage.sprite = correctTexture;
        }
        else
        {
            GetComponentInChildren<DialogInfo>().options = optionsWrong;
            currentImage.sprite = incorrectTexture;
        }
    }

}
