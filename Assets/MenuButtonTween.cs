using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Rendering;

[RequireComponent(typeof(CanvasGroup))]
public class MenuButtonTween : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [SerializeField] float duration = 0.8f;
    [SerializeField] Vector2 moveOffset = new Vector2(0, 300);
    [SerializeField] float startRotation = 90f;
    [SerializeField] float termBetweenButtonMove = 0.2f;

    private CanvasGroup canvasGroup;
    private Sequence showSeq;
    private Sequence hideSeq;
    private List<RectTransform> buttonRTs = new List<RectTransform>();
    private List<Vector2> originalPos = new List<Vector2>();
    private List<CanvasGroup> buttonCGs = new List<CanvasGroup>();
    private List<Quaternion> originalRot = new List<Quaternion>();

    void Awake()
    {
        // CanvasGroup 세팅
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // 초기 투명

        // 버튼 RectTransform 모으기
        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            var rt = btn.GetComponent<RectTransform>();
            var cg = btn.GetComponent<CanvasGroup>();
            buttonRTs.Add(rt);
            buttonCGs.Add(cg);
            originalPos.Add(rt.anchoredPosition);
            originalRot.Add(rt.localRotation);
            cg.alpha = 0;
        }        
    }

    // 메뉴 켜질 때
    public void StartMenu()
    {
        Debug.Log("StartMenu");
        canvasGroup.alpha = 1f;
        for (int i = 0; i < buttonRTs.Count; i++)
        {
            var rt = buttonRTs[i];
            var cg = buttonCGs[i];
            var pos = originalPos[i];
            var rot = originalRot[i];
            float delay = i * termBetweenButtonMove;

            // 시작 전 위치, 회전, 알파 세팅
            rt.anchoredPosition = pos + moveOffset;
            rt.localRotation = Quaternion.Euler(0, 0, startRotation);
            cg.alpha = 0f;

            // 이동
            rt.DOAnchorPos(pos, duration)
              .SetEase(Ease.InOutQuint)
              .SetDelay(delay);

            // 회전
            rt.DOLocalRotateQuaternion(rot, duration)
              .SetEase(Ease.InOutQuint)
              .SetDelay(delay);

            // 버튼
            cg.DOFade(1f, duration)
              .SetEase(Ease.InOutQuint)
              .SetDelay(delay);
        }
        //다시 켜주기기
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // 메뉴 끌 때 호출
    public void HideMenu()
    {
        canvasGroup.alpha = 1f;
        Debug.Log("HideMenu");
        // 클릭 차단
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;


        int n = buttonRTs.Count;
        for (int i = 0; i < n; i++)
        {
            // 역순 재생
            int revI = n - 1 - i;                 
            var rt = buttonRTs[revI];
            var cg = buttonCGs[revI];
            var pos = originalPos[revI];
            float delay = i * termBetweenButtonMove;

            // 위치
            rt.DOAnchorPos(pos - moveOffset, duration)
              .SetEase(Ease.InOutQuint)
              .SetDelay(delay);

            // 회전
            rt.DOLocalRotate(new Vector3(0, 0, startRotation), duration)
              .SetEase(Ease.InOutQuint)
              .SetDelay(delay);

            // 페이드아웃
            cg.DOFade(0f, duration)
              .SetEase(Ease.InOutQuint)
              .SetDelay(delay);
        }
    }
}