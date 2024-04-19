using UnityEngine;

public class GolemSMBAttack : SceneLinkedSMB<EnemyBehavior>
{
    private Vector3 _attackPosition;

    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        base.OnSLStateEnter(animator,animatorStateInfo,layerIndex);
        _monoBehaviour.Controller.SetFollowNavmeshAgent(false);

        _attackPosition = _monoBehaviour._target.transform.position;
        var toTarget = _attackPosition - _monoBehaviour.transform.position;
        toTarget.y = 0;

        _monoBehaviour.transform.forward = toTarget.normalized;
        _monoBehaviour.Controller.SetForward(_monoBehaviour.transform.forward);
    }
    
    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateExit(animator, stateInfo, layerIndex);

        _monoBehaviour.Controller.SetFollowNavmeshAgent(true);
    }
}
