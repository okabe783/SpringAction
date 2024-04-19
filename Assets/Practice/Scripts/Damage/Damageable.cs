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

    private void Start()
    {
        ResetDamage();
        col = GetComponent<Collider>();
    }

    private void Update()
    {
        if (_isInvincible)
        {
            _lastHitTime += Time.deltaTime;
            //最後にダメージを受けてから_invincibleTimeを過ぎたなら無敵ではなくなる
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
        //無敵状態でDamageを受けた時
        if (_isInvincible)
        {
            OnHitWhileInvincible.Invoke();
            return;
        }

        var forward = transform.forward;　//z軸を取得
        //Damageをどの方向から受けたかを判定
        //Damageを受けた時のPlayerの向きを調整。引数1を軸に引数2だけ回転
        forward = Quaternion.AngleAxis(_hitForwardRotation, transform.up) * forward;
        //Damageが発生した場所とplayerの相対的な位置を取得
        var _positionToDamage = data._damageSource - transform.position;
        //上方向に平行な成分を取り除きdamage発生地点からpPlayerの上方向に垂直な成分が除去された新しい位置ベクトルを取得
        _positionToDamage -= transform.up * Vector3.Dot(transform.up, _positionToDamage);
        //一定の角度以上外側から攻撃されてもDamageを受けない
        if(Vector3.Angle(forward,_positionToDamage) > _hitAngle * 0.5)
            return;
        _isInvincible = true;
        _currentHp -= data._amount; //Damageを受ける
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