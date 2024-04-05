using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>Enemyの制御</summary>
[DefaultExecutionOrder(-1)]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCtrl : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private Rigidbody _rigidbody;
    private Vector3 _externalForce;

    public bool _applyAnimationRotation;
    public bool _interpolateTurning;
    private bool _followNavmeshAgent;
    private bool externalForceAddGravity = true;
    private bool _underExternalForce; //外力がobjectに適用されているか
    private bool _grounded;

    private const float _groundRayDistance = .8f;
    private void OnEnable()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _animator.updateMode = AnimatorUpdateMode.AnimatePhysics;　//?
        _navMeshAgent.updatePosition = false;
        _rigidbody = GetComponentInChildren<Rigidbody>();
        if (_rigidbody == null)
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.isKinematic = true; //物理演算の影響を受けない
        _rigidbody.useGravity = false; //重力を無効
        //物理エンジンの計算結果をフレーム間で補完することでobjectの動きが滑らかになる
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _followNavmeshAgent = true;
    }

    private void FixedUpdate()
    {
        _animator.speed = CharacterInput.Instance != null && CharacterInput.Instance.HaveControl() ? 1.0f : 0.0f;
        CheckGround();
        if(_underExternalForce)
            ForceMove();
    }

    /// <summary>接地判定</summary>
    private void CheckGround()
    {
        RaycastHit hit;
        //objectの下にレイを飛ばす
        var ray = new Ray(transform.position + Vector3.up * _groundRayDistance * 0.5f, -Vector3.up);
        _grounded = Physics.Raycast(ray, out hit, _groundRayDistance, Physics.AllLayers,
            QueryTriggerInteraction.Ignore);
    }
    /// <summary>外力による移動</summary>
    private void ForceMove()
    {
        //外力に重力を追加するかどうかを判定し、重力を追加する場合は外力に重力ベクトルを追加
        if (externalForceAddGravity)
            _externalForce += Physics.gravity * Time.deltaTime;
        RaycastHit hit;
        var move = _externalForce * Time.deltaTime;
        if(!_rigidbody.SweepTest(move.normalized,out hit,move.sqrMagnitude))　//移動先に障害物があるか判定
            _rigidbody.MovePosition(_rigidbody.position + move);　//ない場合は移動量を加算して移動する
        _navMeshAgent.Warp(_rigidbody.position);　//移動先にNavMeshをワープ
    }

    private void OnAnimatorMove()
    {
        if(_underExternalForce)
            return;
        if (_followNavmeshAgent)
        {
            //NavMesh上を正確に移動させる
            _navMeshAgent.speed = (_animator.deltaPosition / Time.deltaTime).magnitude;
            transform.position = _navMeshAgent.nextPosition;
        }
        else
        {
            RaycastHit hit;
            //衝突を計算
            if (!_rigidbody.SweepTest(_animator.deltaPosition.normalized, out hit,
                    _animator.deltaPosition.sqrMagnitude))
            {
                //衝突がなければ物体を移動させる
                _rigidbody.MovePosition(_rigidbody.position + _animator.deltaPosition);
            }
        }

        if (_applyAnimationRotation)
        {
            transform.forward = _animator.deltaRotation * transform.forward;
        }
    }

    /// <summary>特定のAnimationをするときにNavMeshによって設定されている位置を無効にする</summary>
    public void SetFollowNavmeshAgent(bool follow)
    {
        if (!follow && _navMeshAgent.enabled)
        {
            //objectの位置を設定しない
            _navMeshAgent.ResetPath();
        }
        else if(follow && !_navMeshAgent.enabled)
        {
            //objectの位置を設定
            _navMeshAgent.Warp(transform.position);
        }

        _followNavmeshAgent = follow;
        _navMeshAgent.enabled = follow;
    }

    /// <summary>外力をobjectに追加</summary>
    public void AddForce(Vector3 force, bool useGravity = true)
    {
        if(_navMeshAgent.enabled)
            _navMeshAgent.ResetPath();
        _externalForce = force;
        //外力によって移動させる
        _navMeshAgent.enabled = false;
        _underExternalForce = true;
        externalForceAddGravity = useGravity;
    }

    public void ClearForce()
    {
        _underExternalForce = false;
        _navMeshAgent.enabled = true;
    }

    /// <summary>objectの向きを設定</summary>
    public void SetForward(Vector3 forward)
    {
        //objectを指定された方向に移動
        var targetRotation = Quaternion.LookRotation(forward);
        if (_interpolateTurning)
        {
            //Targetに向かって徐々に回転させる
            targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                _navMeshAgent.angularSpeed * Time.deltaTime);
        }

        transform.rotation = targetRotation;
    }

    /// <summary>目標地点の設定</summary>
    public bool SetTarget(Vector3 position)
    {
        return _navMeshAgent.SetDestination(position);
    }
}