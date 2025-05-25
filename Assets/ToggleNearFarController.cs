using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ToggleNearFarController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject leftInteractor;
    [SerializeField] private GameObject rightInteractor;

    private InputAction _toggleNearFarAction;

    void OnEnable()
    {
        var map = inputActions
        .FindActionMap("XRI Left Interaction", throwIfNotFound: true);

        _toggleNearFarAction = map.FindAction("ToggleNearFarInteractor", throwIfNotFound: true);

        _toggleNearFarAction.performed += OnToggleButtonPressed;
        _toggleNearFarAction.Enable();
    }

    private void OnToggleButtonPressed(InputAction.CallbackContext ctx)
    {
        leftInteractor.SetActive(!leftInteractor.activeSelf);
        rightInteractor.SetActive(!rightInteractor.activeSelf);
    }
}
