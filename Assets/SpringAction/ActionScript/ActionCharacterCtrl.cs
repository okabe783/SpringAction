using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ActionCharacterCtrl : MonoBehaviour
{
    private Animator _animator;
    private ActionCharacterInput _characterInput;
    private bool _canAttack;

    //Hash値
    private readonly int _hashAttack = Animator.StringToHash("Attack");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _characterInput = GetComponent<ActionCharacterInput>();
    }

    private void Attack()
    {
        if (_canAttack)
        {
            _animator.SetTrigger(_hashAttack);
        }
    }
}