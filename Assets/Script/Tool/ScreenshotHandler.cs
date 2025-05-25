using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine.InputSystem;

public class ScreenshotHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    [Header("UI Preview")]
    [SerializeField] private RawImage previewImage;      // Canvas 위 RawImage
    [SerializeField] private Button saveButton;          // 저장 버튼
    [SerializeField] private Button deleteButton;        // 삭제 버튼

    private InputAction _screenshotAction;

    public string screenshotBaseName = "Artwork";
    public string screenshotExtension = ".png";

    public AudioSource audioSource;
    public AudioClip shutterSound;

    public GameObject notificationPanel;
    public TMP_Text notificationText;

    private string lastSavedPath = "";

    private Texture2D _currentScreenshot;

    void Awake()
    {
        // 버튼 콜백 연결
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        previewImage.gameObject.SetActive(true);
    }

    void OnEnable()
    {
        var map = inputActions
        .FindActionMap("XRI Right Interaction", throwIfNotFound: true);

        _screenshotAction = map.FindAction("ScreenShot", throwIfNotFound: true);

        _screenshotAction.performed += OnScreenshotPerformed;
        _screenshotAction.Enable();
    }
    void OnDisable()
    {
        _screenshotAction.performed -= OnScreenshotPerformed;

        _screenshotAction.Disable();
    }


    private void OnScreenshotPerformed(InputAction.CallbackContext ctx)
    {
        StartCoroutine(TakeScreenshot());
    }

    IEnumerator TakeScreenshot()
    {
        yield return new WaitForEndOfFrame();

        audioSource.PlayOneShot(shutterSound);

        _currentScreenshot = ScreenCapture.CaptureScreenshotAsTexture();

        // Canvas에 뿌리기
        previewImage.texture = _currentScreenshot;
        previewImage.gameObject.SetActive(true);

        ShowNotification("Preview ready.");
    }


    void ShowNotification(string message)
    {
        if (notificationPanel && notificationText)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);
            StartCoroutine(HideNotificationAfterSeconds(2.0f));
        }
    }

    IEnumerator HideNotificationAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        notificationPanel.SetActive(false);
    }

    private void OnSaveButtonClicked()
    {
        if (_currentScreenshot == null)
            return;

        // 파일 경로 생성
        string folderPath = Application.dataPath + "/Resources/";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = folderPath + screenshotBaseName + screenshotExtension;
        int count = 1;
        while (File.Exists(filePath))
        {
            filePath = folderPath + screenshotBaseName + "_" + count + screenshotExtension;
            count++;
        }

        // PNG로 저장
        File.WriteAllBytes(filePath, _currentScreenshot.EncodeToPNG());
        ShowNotification($"Saved: {screenshotBaseName + screenshotExtension}");

    }

    private void OnDeleteButtonClicked()
    {
        if (_currentScreenshot != null)
        {
            Destroy(_currentScreenshot);
            _currentScreenshot = null;
        }

        // 미리보기 & 버튼 숨기기
        previewImage.texture = null;
        previewImage.gameObject.SetActive(false);

        ShowNotification("Preview deleted.");
    }
}
