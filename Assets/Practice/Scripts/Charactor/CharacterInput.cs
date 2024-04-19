using System.Collections;
using UnityEngine;

public class CharacterInput : MonoBehaviour
{
    public static CharacterInput Instance { get; }

    [SerializeField] private float _movePower = 3; //移動速度
    private Rigidbody _rb;
    private Vector3 _dir; //キャラクターの移動方向を表すベクトル
    private Animator _animator;
    private WaitForSeconds _attackInputWait;
    private Coroutine _attackWaitCoroutine;
    private bool _attack;
    private Vector2 _movement;
    [HideInInspector] public bool playerCtrlInputBlocked; //playerの入力を受け付けるかの判定
    private bool _externalInputBlocked;　//外部からの入力を受け付けるかの判定
    private const float _attackInputDuration = 0.03f;

    public Vector2 MoveInput
    {
        get
        {
            if (playerCtrlInputBlocked || _externalInputBlocked)
                return Vector2.zero;
            return _movement;
        }
    }

    public bool Attack => _attack && !playerCtrlInputBlocked && !_externalInputBlocked;

    private void Awake()
    {
        _attackInputWait = new WaitForSeconds(_attackInputDuration);
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Move();
        if (Input.GetButtonDown("Fire1"))
        {
            if (_attackWaitCoroutine != null)
                StopCoroutine(_attackWaitCoroutine);
            _attackWaitCoroutine = StartCoroutine(AttackWait());
        }
    }

    private void FixedUpdate()
    {
        var velocity = _rb.velocity;
        velocity.y = 0;
        _animator.SetFloat("ForwardSpeed", velocity.magnitude);
    }

    private void Move()
    {
        //水平方向と垂直方向の移動を取得
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");
        //入力方向を計算しカメラの向きを合わせる
        _dir = Vector3.forward * v + Vector3.right * h;
        _dir = Camera.main.transform.TransformDirection(_dir);
        //上下方向の移動を無効
        _dir.y = 0;
        //移動方向を正規化
        var forward = _dir.normalized;
        _rb.velocity = forward * _movePower;
        forward.y = 0;
        //キャラクターの向きを移動方向に合わせる
        if (forward != Vector3.zero)
        {
            this.transform.forward = forward;
        }
    }

    private IEnumerator AttackWait()
    {
        _attack = true;
        
        yield return _attackInputWait;
        
        _attack = false;
    }

    private void OnAnimatorMove()
    {
        transform.position = _animator.rootPosition;
    }

    public bool HaveControl()
    {
        return !_externalInputBlocked;
    }
}