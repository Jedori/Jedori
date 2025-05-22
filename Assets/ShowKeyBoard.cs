using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class ShowKeyBoard : MonoBehaviour
{
    private TMP_InputField InputField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputField = GetComponent<TMP_InputField>();
        InputField.onSelect.AddListener(x => OpenKeyBoard());
    }

    public void OpenKeyBoard()
    {
        Debug.Log("OpenKeyBoard");
        NonNativeKeyboard.Instance.InputField = InputField;
        NonNativeKeyboard.Instance.PresentKeyboard(InputField.text);
    }
    
}
