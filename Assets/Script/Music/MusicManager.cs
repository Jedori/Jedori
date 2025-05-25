using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [Tooltip("기본 BGM 클립")]
    [SerializeField] private AudioClip defaultMusic;
    [Tooltip("음악 최대 크기기")]
    [SerializeField] private float maxVolum = 1f;

    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 시작할 때 기본 음악 플레이
        musicSource.clip = defaultMusic;
        musicSource.volume = 1f;
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>
    /// 새로운 BGM으로 부드럽게 전환
    /// </summary>
    public void ChangeMusic(AudioClip newClip, float fadeDuration = 1f)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeToNewClip(newClip, fadeDuration));
    }

    private IEnumerator FadeToNewClip(AudioClip newClip, float duration)
    {
        float half = duration * 0.5f;
        float t = 0f;

        // 1) 페이드아웃
        while (t < half)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(maxVolum, 0f, t / half);
            yield return null;
        }

        // 2) 클립 교체 및 재생
        musicSource.clip = newClip;
        musicSource.Play();

        // 3) 페이드인
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, maxVolum, t / half);
            yield return null;
        }

        musicSource.volume = maxVolum;
        _fadeRoutine = null;
    }

    /// <summary>
    /// 기본 BGM으로 복귀
    /// </summary>
    public void ResetToDefault(float fadeDuration = 1f)
    {
        ChangeMusic(defaultMusic, fadeDuration);
    }
}