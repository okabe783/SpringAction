using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private static readonly int _hashVerticalDot = Animator.StringToHash("VerticalHitDot");
    private static readonly int _hashHorizontalDot = Animator.StringToHash("HorizontalHitDot");
    private static readonly int _hashHit = Animator.StringToHash("");
    public EnemyCtrl Controller => _controller;

    private EnemyCtrl _controller;
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
        _controller.AddForce(pushForce.normalized * 5.5f,false);
        //AnimatorにDamageの方向をセットする
        _controller.Animator.SetFloat(_hashVerticalDot,verticalDot);
        _controller.Animator.SetFloat(_hashHorizontalDot,horizontalDot);
        
        _controller.Animator.SetTrigger(_hashHit);
    }
}
