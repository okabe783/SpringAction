using System;
using UnityEngine;

/// <summary>武器にアタッチする</summary>
public class SwordWeapon : MonoBehaviour
{
    public class AttackPoint
    {
        public float _radius; //半径
        public Vector3 _offset;　//相対位置
        public Transform _attackRoot;　//攻撃の起点となるobject
    }

    private Vector3[] _previousPos = null; //前回の位置
    private static RaycastHit[] _raycastHitCache = new RaycastHit[32];
    private GameObject _owner;
    private bool _inAttack;
    public AttackPoint[] _attackPoints = Array.Empty<AttackPoint>();

    /// <summary>自傷させないようにする</summary>
    public void SetOwner(GameObject owner)
    {
        _owner = owner;
    }

    public void FixedUpdate()
    {
        if (_inAttack)
        {
            for (var i = 0; i < _attackPoints.Length; i++)
            {
                var pts = _attackPoints[i];
                //攻撃が発生する位置を計算
                var worldPos = pts._attackRoot.position + pts._attackRoot.TransformVector(pts._offset);
                var attackVector = worldPos - _previousPos[i];
                //攻撃ベクトルが非常に小さかった場合の為に変わりの値を設定しerrorを回避
                if (attackVector.magnitude < 0.001f)
                    attackVector = Vector3.forward * 0.0001f;
                //worldPosを原点として引数2方向にRayを発射
                var r = new Ray(worldPos, attackVector.normalized);
                
                var contacts = Physics.SphereCastNonAlloc(r,pts._radius,)
            }
        }
    }
}
