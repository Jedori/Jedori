using UnityEngine;

public class PhysicalRig : MonoBehaviour
{
    [SerializeField]private Transform _playerHead;
    [SerializeField]private CapsuleCollider _bodyCollder;

    [SerializeField]private float _bodyHeightMin = 0.5f;
    [SerializeField]private float _bodyHeightMax = 2f;

    void FixedUpdate()
    {
        _bodyCollder.height = Mathf.Clamp(_playerHead.localPosition.y, _bodyHeightMin, _bodyHeightMax);
        _bodyCollder.center = new Vector3(_playerHead.localPosition.x, _bodyCollder.height / 2, _playerHead.localPosition.z);
    }
}
