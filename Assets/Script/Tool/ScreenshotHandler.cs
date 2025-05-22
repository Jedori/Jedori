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

    private string lastSavedPath = "";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(TakeScreenshot());
        }
    }

    IEnumerator TakeScreenshot()
    {
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

        audioSource.PlayOneShot(shutterSound);

        ScreenCapture.CaptureScreenshot(filePath);
        ShowNotification("Generating Artwork...");

        yield return new WaitForSeconds(1.0f);
        if (File.Exists(lastSavedPath))
            ShowNotification("Artwork saved successfully!");
        else
            ShowNotification("Artwork save failed...");
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
}
