using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using TMPro;

public class ScreenshotHandler : MonoBehaviour
{
    public string screenshotBaseName = "Artwork";
    public string screenshotExtension = ".png";
    
    public AudioSource audioSource;
    public AudioClip shutterSound;

    public GameObject notificationPanel;
    public TMP_Text notificationText;

    // --- 미리보기를 위한 새로운 UI 요소 ---
    public GameObject previewPanel; // 미리보기 이미지와 버튼을 포함하는 패널
    public RawImage previewImage;   // 스크린샷을 표시할 RawImage
    public Button saveButton;       // 스크린샷을 저장할 버튼
    public Button discardButton;    // 스크린샷을 버릴 버튼

    private string lastSavedPath = "";
    private Texture2D screenshotTexture; // 캡처된 스크린샷을 임시로 보관할 변수

    void Start()
    {
        // 초기에는 미리보기 패널을 숨깁니다.
        if (previewPanel != null)
            previewPanel.SetActive(false);

        // 버튼 리스너를 할당합니다.
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveScreenshotConfirmed);
        if (discardButton != null)
            discardButton.onClick.AddListener(DiscardScreenshot);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(CaptureScreenshotForPreview());
        }
    }

    IEnumerator CaptureScreenshotForPreview()
    {
        // 미리보기 및 알림 패널이 꺼져 있는지 확인합니다.
        if (previewPanel != null) previewPanel.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);

        audioSource.PlayOneShot(shutterSound);

        yield return new WaitForEndOfFrame(); // 프레임이 끝날 때까지 기다려 화면을 캡처합니다.

        // 스크린샷을 저장할 텍스처를 생성합니다.
        int width = Screen.width;
        int height = Screen.height;
        screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshotTexture.Apply();

        // 미리보기 패널에 스크린샷을 표시합니다.
        if (previewPanel != null && previewImage != null)
        {
            previewImage.texture = screenshotTexture;
            previewPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Preview Panel or Image not assigned in ScreenshotHandler!");
            ShowNotification("Artwork save failed..."); // 미리보기 표시 실패 메시지
        }
    }

    // "저장" 버튼이 클릭될 때 호출됩니다.
    void SaveScreenshotConfirmed()
    {
        if (screenshotTexture == null)
        {
            ShowNotification("No artwork to save!"); // 저장할 아트워크 없음 메시지
            return;
        }

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
        lastSavedPath = filePath;

        // 텍스처를 PNG로 인코딩하고 파일로 저장합니다.
        byte[] bytes = screenshotTexture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        // 텍스처를 정리합니다.
        Destroy(screenshotTexture);
        screenshotTexture = null;

        // 미리보기 숨기고 성공 알림 표시
        if (previewPanel != null) previewPanel.SetActive(false);
        ShowNotification("Artwork saved successfully!"); // 아트워크 저장 성공 메시지
    }

    // "취소" 버튼이 클릭될 때 호출됩니다.
    void DiscardScreenshot()
    {
        // 텍스처를 정리합니다.
        if (screenshotTexture != null)
        {
            Destroy(screenshotTexture);
            screenshotTexture = null;
        }
        
        // 미리보기 숨기고 취소 알림 표시
        if (previewPanel != null) previewPanel.SetActive(false);
        ShowNotification("Artwork discarded."); // 아트워크 취소됨 메시지
    }

    void ShowNotification(string message)
    {
        if (notificationPanel && notificationText)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);
            // 미리보기 상태의 알림은 자동으로 숨기지 않습니다.
            if (!previewPanel.activeSelf) 
            {
                StartCoroutine(HideNotificationAfterSeconds(2.0f));
            }
        }
    }

    IEnumerator HideNotificationAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        notificationPanel.SetActive(false);
    }
}