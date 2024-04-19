using UnityEngine;
using UnityEngine.AI;
public class GolemSMBPursuit : SceneLinkedSMB<EnemyBehavior>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

        _monoBehaviour.FindTarget();

        if (_monoBehaviour.Controller.navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial
            || _monoBehaviour.Controller.navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            _monoBehaviour.StopPursuit();
            return;
        }

        if (_monoBehaviour._target == null)
        {
            //targetが失われたとき追跡を終了する
            _monoBehaviour.StopPursuit();
        }
        else
        {
            _monoBehaviour.RequestTargetPosition();
            var toTarget = _monoBehaviour._target.transform.position - _monoBehaviour.transform.position;

            if (toTarget.sqrMagnitude < _monoBehaviour._attackDistance * _monoBehaviour._attackDistance)
            {
                _monoBehaviour.TriggerAttack();
            }
            else if (_monoBehaviour.followerData._assignedSlot != -1)
            {
                var targetPoint = _monoBehaviour._target.transform.position +
                                  _monoBehaviour.followerData._distributor.GetDirection(_monoBehaviour.followerData
                                      ._assignedSlot) * _monoBehaviour._attackDistance * 0.9f;

                _monoBehaviour.Controller.SetTarget(targetPoint);
            }
            else
            {
                _monoBehaviour.StopPursuit();
            }
        }
    }
}
