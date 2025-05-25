
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MusicZoneOnce : MonoBehaviour
{
    [Tooltip("이 영역에 들어올 때 재생할 BGM")]
    [SerializeField] private AudioClip zoneMusic;
    [Tooltip("영역 탈출과 시간이 지나면 알아서 기본 BGM으로 복귀")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private bool resetOnExit = true;

    // 한 번만 플레이되도록
    private bool _hasPlayed = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasPlayed || !other.CompareTag("Player")) return;
        _hasPlayed = true;
        StartCoroutine(PlayZoneMusicOnce());
    }

    private IEnumerator PlayZoneMusicOnce()
    {
        // zoneMusic 페이드 전환
        AudioManager.Instance.ChangeMusic(zoneMusic, fadeTime);

        // 클립 길이만큼 대기
        yield return new WaitForSeconds(zoneMusic.length);

        // 원래 BGM으로 페이드 복귀
        AudioManager.Instance.ResetToDefault(fadeTime);

        GetComponent<Collider>().enabled = false;
    }
    private void OnTriggerExit(Collider other)
    {
        if (!resetOnExit || !other.CompareTag("Player")) return;

        AudioManager.Instance.ResetToDefault(fadeTime);
    }
}