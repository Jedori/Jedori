using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class MenuButtonTween : MonoBehaviour
{
  [Header("애니메이션 설정")]
  [SerializeField] float duration = 0.8f;
  [SerializeField] Vector2 moveOffset = new Vector2(0, 300);
  [SerializeField] float startRotation = 90f;
  [SerializeField] float termBetweenButtonMove = 0.2f;

  CanvasGroup canvasGroup;
  Sequence showSeq;
  Sequence hideSeq;

  List<RectTransform> buttonRTs = new();
  List<Vector2> originalPos = new();
  List<CanvasGroup> buttonCGs = new();
  List<Quaternion> originalRot = new();

  void Awake()
  {
    canvasGroup = GetComponent<CanvasGroup>();
    canvasGroup.alpha = 0f;
    foreach (var btn in GetComponentsInChildren<Button>(true))
    {
      var rt = btn.GetComponent<RectTransform>();
      var cg = btn.GetComponent<CanvasGroup>();
      buttonRTs.Add(rt);
      buttonCGs.Add(cg);
      originalPos.Add(rt.anchoredPosition);
      originalRot.Add(rt.localRotation);
      cg.alpha = 0f;
    }
  }

  public void StartMenu()
  {
    // 1) 기존 시퀀스·트윈 모두 정리
    showSeq?.Kill();
    hideSeq?.Kill();
    foreach (var rt in buttonRTs) rt.DOKill();
    foreach (var cg in buttonCGs) cg.DOKill();

    // 2) 새 쇼 시퀀스 생성
    showSeq = DOTween.Sequence();
    canvasGroup.alpha = 1f;
    canvasGroup.interactable = true;
    canvasGroup.blocksRaycasts = true;

    for (int i = 0; i < buttonRTs.Count; i++)
    {
      var rt = buttonRTs[i];
      var cg = buttonCGs[i];
      var pos = originalPos[i];
      var rot = originalRot[i];
      float d = i * termBetweenButtonMove;

      // 시작값 세팅
      rt.anchoredPosition = pos + moveOffset;
      rt.localRotation = Quaternion.Euler(0, 0, startRotation);
      cg.alpha = 0f;

      // 시퀀스에 삽입
      showSeq.Insert(d, rt.DOAnchorPos(pos, duration).SetEase(Ease.InOutQuint));
      showSeq.Insert(d, rt.DOLocalRotateQuaternion(rot, duration).SetEase(Ease.InOutQuint));
      showSeq.Insert(d, cg.DOFade(1f, duration).SetEase(Ease.InOutQuint));
    }
  }

  public void HideMenu()
  {
    // 1) 기존 시퀀스·트윈 모두 정리
    showSeq?.Kill();
    hideSeq?.Kill();
    foreach (var rt in buttonRTs) rt.DOKill();
    foreach (var cg in buttonCGs) cg.DOKill();

    // 2) 새 하이드 시퀀스 생성
    hideSeq = DOTween.Sequence();
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;

    int n = buttonRTs.Count;
    for (int i = 0; i < n; i++)
    {
      int idx = n - 1 - i;
      var rt = buttonRTs[idx];
      var cg = buttonCGs[idx];
      var pos = originalPos[idx];
      float d = i * termBetweenButtonMove;

      hideSeq.Insert(d, rt.DOAnchorPos(pos - moveOffset, duration).SetEase(Ease.InOutQuint));
      hideSeq.Insert(d, rt.DOLocalRotate(new Vector3(0, 0, startRotation), duration).SetEase(Ease.InOutQuint));
      hideSeq.Insert(d, cg.DOFade(0f, duration).SetEase(Ease.InOutQuint));

      // 마지막 버튼이 다 끝나면 캔버스 숨기기
      if (i == n - 1)
        hideSeq.OnComplete(() => canvasGroup.alpha = 0f);
    }
  }

  void OnDestroy()
  {
    showSeq?.Kill();
    hideSeq?.Kill();
  }
}