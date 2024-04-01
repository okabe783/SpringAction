using System;
using UnityEngine;

/// <summary>武器にアタッチする</summary>
public class SwordWeapon : MonoBehaviour
{
    [System.Serializable]public class AttackPoint
    {
        public float _radius; //半径
        public Vector3 _offset;　//相対位置
        public Transform _attackRoot;　//攻撃の起点となるobject
    }

    public int _damage = 1;
    public LayerMask _targetLayer;
    private Vector3 _direction;
    private Vector3[] _previousPos = null; //前回の位置
    private static RaycastHit[] _raycastHitCache = new RaycastHit[32];
    private GameObject _owner;
    private bool _inAttack;
    private bool _isThrowingHit = false;
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

                var contacts = Physics.SphereCastNonAlloc(r, pts._radius, _raycastHitCache,
                    attackVector.magnitude, ~0, QueryTriggerInteraction.Ignore);

                for (var k = 0; k < contacts; ++k)
                {
                    var col = _raycastHitCache[k].collider;
                    if (col != null)
                    {
                        CheckDamage(col, pts);
                    }

                    _previousPos[i] = worldPos;
                }
            }
        }
    }

    private bool CheckDamage(Collider col, AttackPoint pts)
    {
        var d = col.GetComponent<Damageable>(); //Damageを受けるobjectをcolから検索

        if (d == null)
            return false;
        if (d.gameObject == _owner)
            return true; //自傷しても攻撃は中断しない
        if ((_targetLayer.value & (1 << col.gameObject.layer)) == 0)
            return false; //攻撃対象ではない場合は攻撃終了
        Damageable.DamageMessage data;
        data._amount = _damage;
        data._damager = this;
        data._direction = _direction.normalized;
        data._damageSource = _owner.transform.position;
        data._throwing = _isThrowingHit;
        data._stopCamera = false;
        d.ApplyDamage(data);
        return true;
    }
}