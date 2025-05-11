// Assets/Editor/MirrorAndRetargetAnimClip.cs
using UnityEngine;
using UnityEditor;

public static class MirrorAndRetargetAnimClip
{
    [MenuItem("Assets/Create/Mirror and Retarget AnimationClip")]
    public static void MirrorSelectedClip()
    {
        // 선택된 에셋이 AnimationClip인지 확인
        var clip = Selection.activeObject as AnimationClip;
        if (clip == null)
        {
            Debug.LogWarning("AnimationClip을 선택해주세요.");
            return;
        }

        // 새 클립 생성
        var mirrorClip = new AnimationClip
        {
            frameRate = clip.frameRate,
            legacy = clip.legacy,
            wrapMode = clip.wrapMode
        };

        // 원본 커브 바인딩들 가져오기
        var bindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var bind in bindings)
        {
            // 원본 커브
            var curve = AnimationUtility.GetEditorCurve(clip, bind);
            bool modified = false;

            // X축 위치 커브 반전
            if (bind.propertyName.EndsWith(".m_LocalPosition.x"))
            {
                for (int i = 0; i < curve.keys.Length; i++)
                {
                    var k = curve.keys[i];
                    k.value *= -1f;
                    k.inTangent *= -1f;
                    k.outTangent *= -1f;
                    curve.keys[i] = k;
                }
                modified = true;
            }
            // X축 회전(eulerAngles) 커브 반전
            else if (bind.propertyName.EndsWith("localEulerAnglesRaw.x") ||
                     bind.propertyName.EndsWith("localEulerAngles.x"))
            {
                for (int i = 0; i < curve.keys.Length; i++)
                {
                    var k = curve.keys[i];
                    k.value *= -1f;
                    k.inTangent *= -1f;
                    k.outTangent *= -1f;
                    curve.keys[i] = k;
                }
                modified = true;
            }

            // 만약 반전 로직을 추가하고 싶으면 이곳에 더 작성하세요...

            // 바인딩 경로에서 LeftHand → RightHand 치환
            var mirrorPath = bind.path.Replace("r", "l");

            // 새 바인딩 생성
            var newBind = new EditorCurveBinding
            {
                path = mirrorPath,
                propertyName = bind.propertyName,
                type = bind.type
            };

            // 반전되었든, 아니든 모두 새 클립에 복사
            AnimationUtility.SetEditorCurve(mirrorClip, newBind, curve);
        }

        // 에셋으로 저장
        var originalPath = AssetDatabase.GetAssetPath(clip);
        var newPath = AssetDatabase.GenerateUniqueAssetPath(
            originalPath.Replace(".anim", "_Mirror.anim")
        );
        AssetDatabase.CreateAsset(mirrorClip, newPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"미러 & 리타겟 애니메이션 생성 완료: {newPath}");
    }
}
