using System;
using UnityEngine;
using UnityEngine.Events;

public partial class Damageable : MonoBehaviour
{
    [Tooltip("無敵になる時間")] public float _invincibleTime;
    public bool _isInvincible { get; set; }

    [Tooltip("damageを受ける角度")] [Range(0f, 360.0f)]
    public float _hitAngle = 360.0f;

    [Tooltip("objectの回転にあわせてdamageをうける範囲も回転")] [Range(0f, 360.0f)]
    public float _hitForwardRotation = 360.0f;

    public int _maxHp;
    public int _currentHp { get; private set; } //現在のHP
    //死、ダメージを受けた、無敵の間にダメージを受けた、無敵状態になった、ダメージをリセット
    public UnityEvent OnDeath, TakeDamage, OnHitWhileInvincible, OnBecomeInvincible, OnResetDamage;
    private float _lastHitTime;
    private Collider col;
    private System.Action _schedule;
    
    private void Update()
    {
        if (_isInvincible)
        {
            _lastHitTime += Time.deltaTime;
            //最後にダメージを受けてから_invincibleTimeを過ぎたなら
            if (_lastHitTime > _invincibleTime)
            {
                _lastHitTime = 0.0f;
                _isInvincible = false;
                OnBecomeInvincible.Invoke();
            }
        }
    }

    public void ResetDamage()
    {
        _currentHp = _maxHp;
        _isInvincible = false;
        _lastHitTime = 0.0f;
        OnResetDamage.Invoke();
    }

    public void ApplyDamage(DamageMessage data)
    {
        if (_currentHp <= 0)
            return;
        if (_isInvincible)
        {
            OnHitWhileInvincible.Invoke();
            return;
        }

        var forward = transform.forward;
        forward = Quaternion.AngleAxis(_hitForwardRotation, transform.up) * forward;
        var _positionToDamage = data._damageSource - transform.position;
        _positionToDamage -= transform.up * Vector3.Dot(transform.up, _positionToDamage);
        if(Vector3.Angle(forward,_positionToDamage) > _hitAngle * 0.5)
            return;
        _isInvincible = true;
        _currentHp -= data._amount;
        _schedule += _currentHp <= 0 ? OnDeath.Invoke : TakeDamage.Invoke;
    }

    private void LateUpdate()
    {
        if (_schedule != null)
        {
            _schedule();
            _schedule = null;
        }
    }
}

public struct DamageMessage
{
    public int _amount;
    public Vector3 _damageSource;
}