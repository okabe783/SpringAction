using System;
using UnityEngine;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(100)]
public class EnemyBehavior : MonoBehaviour
{
    //hash
    private static readonly int _hashVerticalDot = Animator.StringToHash("VerticalHitDot");
    private static readonly int _hashHorizontalDot = Animator.StringToHash("HorizontalHitDot");
    private static readonly int _hashHit = Animator.StringToHash("Hit");
    private static readonly int _hashIdleState = Animator.StringToHash("EnemyIdle");
    private static readonly int _hashNearBase = Animator.StringToHash("NearBase");
    private static readonly int _hashInPursuit = Animator.StringToHash("InPursuit");　//追跡
    public static readonly int _hashAttack = Animator.StringToHash("Attack");

    private EnemyCtrl _controller;
    protected TargetDistributor.TargetFollower _followerInstance;
    public TargetScanner _playerScanner;
    
    [Tooltip("Playerが範囲外に外れた時に追跡をやめるまでの秒数")] public float _timeToStopPursuit;
    [System.NonSerialized] public float _attackDistance = 3;
    
    private float _timerSinceLostTarget;
    public EnemyCtrl Controller => _controller;
    public CharacterCtrl _target { get; private set; } = null;
    public Vector3 _originalPosition { get; protected set; }
    public TargetDistributor.TargetFollower followerData => _followerInstance;

    private void OnEnable()
    {
        _controller = GetComponentInChildren<EnemyCtrl>();
        _originalPosition = transform.position; //初期位置を格納
        _controller.Animator.Play(_hashIdleState, 0, Random.value);
        SceneLinkedSMB<EnemyBehavior>.Initialise(_controller.Animator,this);
    }

    protected void OnDisable()
    {
        if(_followerInstance != null)
            _followerInstance._distributor.UnregisterFollower(_followerInstance); //followerの登録解除
    }

    private void FixedUpdate()
    {
        var toBase = _originalPosition - transform.position; //現在の位置と元の位置の差を計算
        toBase.y = 0;
        //差の大きさが一定値よりも小さい場合にtrueにすることで元の位置に戻っているかを判定
        _controller.Animator.SetBool(_hashNearBase, toBase.sqrMagnitude < 0.1 * 0.1f);
    }

    /// <summary>playerを検知する</summary>
    public void FindTarget()
    {
        //targetがすでに見えている場合は高低差を無視してPlayerを検出する
        var target = _playerScanner.Detect(transform, this._target == null);
        //敵がまだtargetを持っていない場合
        if (_target == null)
        {
            //敵がplayerを初めて検出した場合playerの周りに移動するための目標地点を選択する
            if (target != null)
            {
                _target = target;
                var distributor = target.GetComponentInChildren<TargetDistributor>();
                if (distributor != null)
                    _followerInstance = distributor.RegisterNewFollower();
            }
        }
        //敵がすでにtargetを持っている場合
        else
        {
            //Enemyがplayerを見失った後playerが検出範囲外に移動した場合にのみtargetをresetする
            if (target == null)
            {
                _timerSinceLostTarget += Time.deltaTime;
                //playerを見失った後追跡をやめるまで
                if (_timerSinceLostTarget >= _timeToStopPursuit)
                {
                    var toTarget = this._target.transform.position - transform.position; //どれだけ離れているか

                    if (toTarget.sqrMagnitude > _playerScanner._detectionRadius * _playerScanner._detectionRadius)
                    {
                        if (_followerInstance != null)
                            _followerInstance._distributor.UnregisterFollower(_followerInstance);
                        //ターゲットが範囲外に移動したら、ターゲットをリセットする。
                        _target = null;
                    }
                }
            }
            else
            {
                if (target != _target)
                {
                    //前のfollowerを解除
                    if (_followerInstance != null)
                        _followerInstance._distributor.UnregisterFollower(_followerInstance);

                    _target = target;

                    var distributor = target.GetComponentInChildren<TargetDistributor>();
                    //新しいfollowerを登録
                    if (distributor != null)
                        _followerInstance = distributor.RegisterNewFollower();
                }

                _timerSinceLostTarget = 0.0f;
            }
        }
    }

    /// <summary>追跡を始める</summary>
    public void StartPursuit()
    {
        if (_followerInstance != null)
        {
            _followerInstance._requireSlot = true;
            RequestTargetPosition();
        }

        _controller.Animator.SetBool(_hashInPursuit, true);
    }

    /// <summary>追跡をやめる </summary>
    public void StopPursuit()
    {
        if (_followerInstance != null)
        {
            _followerInstance._requireSlot = false;
        }

        _controller.Animator.SetBool(_hashInPursuit, false);
    }

    /// <summary>followerがtargetに対して適切な位置に配置される</summary>
    public void RequestTargetPosition()
    {
        var fromTarget = transform.position - _target.transform.position;
        fromTarget.y = 0;
        _followerInstance._requiredPoint = _target.transform.position + fromTarget.normalized * _attackDistance * 0.9f;
    }

    public void WalkBackToBase()
    {
        //登録解除
        if (_followerInstance != null)
            _followerInstance._distributor.UnregisterFollower(_followerInstance);
        _target = null;
        StopPursuit();
        _controller.SetTarget(_originalPosition);
        _controller.SetFollowNavmeshAgent(true);
    }

    public void TriggerAttack()
    {
        _controller.Animator.SetTrigger(_hashAttack);
    }

    private void Death(Damageable.DamageMessage msg)
    {
        var pushForce = transform.position - msg._damageSource;
        pushForce.y = 0;
        transform.forward = -pushForce.normalized;
        _controller.AddForce(pushForce.normalized * 7.0f - Physics.gravity * 0.6f);
        _controller.Animator.SetTrigger(_hashHit);
    }

    /// <summary>Damageを受けた時に呼び出す</summary>
    private void ApplyDamage(Damageable.DamageMessage msg)
    {
        //Damageをうけた方向を計算
        var verticalDot = Vector3.Dot(Vector3.up, msg._direction);
        var horizontalDot = Vector3.Dot(transform.right, msg._direction);

        var pushForce = transform.position - msg._damageSource;
        pushForce.y = 0;

        transform.forward = -pushForce.normalized;
        //playerに押し出す力を加える
        _controller.AddForce(pushForce.normalized * 5.5f, false);
        //AnimatorにDamageの方向をセットする
        _controller.Animator.SetFloat(_hashVerticalDot, verticalDot);
        _controller.Animator.SetFloat(_hashHorizontalDot, horizontalDot);

        _controller.Animator.SetTrigger(_hashHit);
    }
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        _playerScanner.EditorGizmo(transform);
    }
    #endif
}