using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
public class TurnLight : MonoBehaviour
{
    [SerializeField] GameObject flashLight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.activated.AddListener(x => TurnLightSwitch());
    }

    public void TurnLightSwitch()
    {
        if (flashLight.activeSelf) flashLight.SetActive(false);
        else flashLight.SetActive(true);
    }


}
