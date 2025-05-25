using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DisableGrabbingHandModel : MonoBehaviour
{
    [SerializeField] private GameObject leftHandModel;
    [SerializeField] private GameObject rightHandModel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(HideGrabbingHand);
        grabInteractable.selectExited.AddListener(ShowGrabbingHand);

    }

    public void HideGrabbingHand(SelectEnterEventArgs args)
    {
        Debug.Log("HideGrabbing");
        Debug.Log("IsgrabInteractor is Null? " + args.interactorObject.transform.name);
        Debug.Log("IsgrabInteractor is Null? " + args.interactorObject.transform.tag);
        if (args.interactorObject.transform.tag == "LeftHand")
        {
            Debug.Log("Left Hand HideGrabbing");
            leftHandModel.SetActive(false);
        }
        if (args.interactorObject.transform.tag == "RightHand")
        {
            Debug.Log("Right Hand HideGrabbing");
            rightHandModel.SetActive(false);
        }
    }

    public void ShowGrabbingHand(SelectExitEventArgs args)
    {
        Debug.Log("ShowGrabbing");
        Debug.Log("IsgrabInteractor is Null? " + args.interactorObject.transform.name);
        Debug.Log("IsgrabInteractor is Null? " + args.interactorObject.transform.tag);
        if (args.interactorObject.transform.tag == "LeftHand")
        {
            Debug.Log("Left Hand ShowGrabbing");
            leftHandModel.SetActive(true);
        }
        if (args.interactorObject.transform.tag == "RightHand")
        {
            Debug.Log("Right Hand ShowGrabbing");
            rightHandModel.SetActive(true);
        }
    }
}
