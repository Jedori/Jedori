using UnityEngine;

public class ShutterSoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip shutterSound;

    public void PlayShutter()
    {
        audioSource.PlayOneShot(shutterSound);
    }
}
