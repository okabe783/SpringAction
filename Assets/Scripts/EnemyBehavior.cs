using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBehavior : MonoBehaviour
{
    private static readonly int _hashVerticalDot = Animator.StringToHash("VerticalHitDot");
    private static readonly int _hashHorizontalDot = Animator.StringToHash("HorizontalHitDot");
    private static readonly int _hashHit = Animator.StringToHash("Hit");
    private static readonly int _hashIdleState = Animator.StringToHash("EnemyIdle");
    private static readonly int _hashThrown = Animator.StringToHash("");
    private static readonly int _hashGrounded = Animator.StringToHash("");
    private static readonly int _hashNearBase = Animator.StringToHash("");
    private static readonly int _hashSpotted = Animator.StringToHash("");
    
    private EnemyCtrl _controller;
    private CharacterCtrl _target = null;
    public EnemyCtrl Controller => _controller;
    public TargetScanner _playerScanner;

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

    public void FindTarget()
    {
        var target = _playerScanner.Detect(transform, _target == null);
        if (_target == null)
        {
            if (target != null)
            {
                _controller.Animator.SetTrigger(_hashSpotted);
                _target = target;
                
            }
        }
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