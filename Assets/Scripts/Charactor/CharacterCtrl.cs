using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
    public class CharacterCtrl : MonoBehaviour
    {
        protected static CharacterCtrl _instance;
        public static CharacterCtrl Instance => _instance;

    public SwordWeapon _weapon;
    public bool canAttack; //攻撃できるか判定
    private Animator _animator;
    private CharacterInput _input;
    private bool _inAttack; //攻撃をしているかの判定

    private bool _inCombo; //連続攻撃をしているかの判定

    //AnimatorCtrlの現在の状態や進行状況
    private AnimatorStateInfo _previousCurrentStateInfo;
    private AnimatorStateInfo _currentStateInfo;
    private AnimatorStateInfo _nextStateInfo;
    private bool _isAnimatorTransitioning; //トランジション中か判定

    //Hash値
    private readonly int _hashWeaponAttack = Animator.StringToHash("Attack");
    private readonly int _hashStateTime = Animator.StringToHash("StateTime");
    private readonly int _hashFirstCombo = Animator.StringToHash("Combo1");
    private readonly int _hashSecondCombo = Animator.StringToHash("Combo2");
    private readonly int _hashThirdCombo = Animator.StringToHash("Combo3");

    private void Awake()
    {
        _input = GetComponent<CharacterInput>();
        _animator = GetComponent<Animator>();
        _weapon.SetOwner(gameObject);
        _instance = this;
    }

    private void OnEnable()
    {
        EquipWeapon(false);
    }

    private void FixedUpdate()
    {
        CacheAnimatorState();
        EquipWeapon(IsWeaponEquip());　//武器を装備
        //StateTimeを0から1の範囲でループさせる
        _animator.SetFloat(_hashStateTime, Mathf.Repeat(_animator.GetCurrentAnimatorStateInfo(0)
            .normalizedTime, 1f));
        _animator.ResetTrigger(_hashWeaponAttack); //Triggerをreset
        //攻撃Animを再生
        if (_input.Attack && canAttack)
            _animator.SetTrigger(_hashWeaponAttack);
    }

    private void EquipWeapon(bool equip)
    {
        _weapon.gameObject.SetActive(equip);
        _inAttack = false;
        _inCombo = equip;
        if (!equip)
            _animator.ResetTrigger(_hashWeaponAttack); //装備解除時にTriggerをReset
    }

    /// <summary>Animatorの状態をcache</summary>
    private void CacheAnimatorState()
    {
        _previousCurrentStateInfo = _currentStateInfo;
        _currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        _nextStateInfo = _animator.GetNextAnimatorStateInfo(0);
        _isAnimatorTransitioning = _animator.IsInTransition(0);
    }

    /// <summary>武器がアクティブになっているかの判定</summary>
    private bool IsWeaponEquip()
    {
        var equip = _nextStateInfo.shortNameHash == _hashFirstCombo ||
                    _currentStateInfo.shortNameHash == _hashFirstCombo;
        equip |= _nextStateInfo.shortNameHash == _hashSecondCombo ||
                 _currentStateInfo.shortNameHash == _hashSecondCombo;
        equip |= _nextStateInfo.shortNameHash == _hashThirdCombo ||
                 _currentStateInfo.shortNameHash == _hashThirdCombo;
        return equip;
    }
}