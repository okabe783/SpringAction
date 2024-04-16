using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public CharacterCtrl target { get; private set; } = null;

    private static readonly int _hashVerticalDot = Animator.StringToHash("VerticalHitDot");
    private static readonly int _hashHorizontalDot = Animator.StringToHash("HorizontalHitDot");
    private static readonly int _hashHit = Animator.StringToHash("Hit");
    private static readonly int _hashIdleState = Animator.StringToHash("EnemyIdle");
    private static readonly int _hashThrown = Animator.StringToHash("");
    private static readonly int _hashGrounded = Animator.StringToHash("");
    private static readonly int _hashNearBase = Animator.StringToHash("");
    private static readonly int _hashSpotted = Animator.StringToHash("");
    private static readonly int _hashInPursuit = Animator.StringToHash("");　//追跡
    private static readonly int _hashAttack = Animator.StringToHash("");

    private EnemyCtrl _controller;
    private TargetDistributor.TargetFollower _followerInstance;
    private float _timerSinceLostTarget;
    public EnemyCtrl Controller => _controller;
    public TargetScanner _playerScanner;
    public float _timeToStopPursuit;
    public float _attackDistance = 3;


    public Vector3 _originalPosition { get; protected set; }

    private void OnEnable()
    {
        _controller = GetComponentInChildren<EnemyCtrl>();
        _controller.Animator.Play(_hashIdleState, 0, Random.value);
    }

    private void FixedUpdate()
    {
        _controller.Animator.SetBool(_hashGrounded, _controller.grounded); //接地判定
        var toBase = _originalPosition - transform.position; //現在の位置と元の位置の差を計算
        toBase.y = 0;
        //差の大きさが一定値よりも小さい場合にtrueにすることで元の位置に戻っているかを判定
        _controller.Animator.SetBool(_hashNearBase, toBase.sqrMagnitude < 0.1 * 0.1f);
    }

    /// <summary>playerを検知する</summary>
    public void FindTarget()
    {
        //targetがすでに見えている場合は高低差を無視してPlayerを検出する
        var target = _playerScanner.Detect(transform, this.target == null);
        //敵がまだtargetを持っていない場合
        if (this.target == null)
        {
            //敵がplayerを初めて検出した場合playerの周りに移動するための目標地点を選択する
            if (target != null)
            {
                _controller.Animator.SetTrigger(_hashSpotted);
                this.target = target;
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
                    var toTarget = this.target.transform.position - transform.position; //どれだけ離れているか

                    if (toTarget.sqrMagnitude > _playerScanner._detectionRadius * _playerScanner._detectionRadius)
                    {
                        if (_followerInstance != null)
                            _followerInstance._distributor.UnregisterFollower(_followerInstance);
                        //ターゲットが範囲外に移動したら、ターゲットをリセットする。
                        this.target = null;
                    }
                }
            }
            else
            {
                if (target != this.target)
                {
                    //前のfollowerを解除
                    if (_followerInstance != null)
                        _followerInstance._distributor.UnregisterFollower(_followerInstance);

                    this.target = target;

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
        var fromTarget = transform.position - target.transform.position;
        fromTarget.y = 0;
        _followerInstance._requiredPoint = target.transform.position + fromTarget.normalized * _attackDistance * 0.9f;
    }

    public void WalkBackToBase()
    {
        //登録解除
        if (_followerInstance != null)
            _followerInstance._distributor.UnregisterFollower(_followerInstance);
        target = null;
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
        _controller.Animator.SetTrigger(_hashThrown);
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
}