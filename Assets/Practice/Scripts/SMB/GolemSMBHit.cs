using UnityEngine;

public class GolemSMBHit : SceneLinkedSMB<EnemyBehavior>
{
   public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   {
      animator.ResetTrigger(EnemyBehavior._hashAttack);
   }

   public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   {
      _monoBehaviour.Controller.ClearForce();
   }
}
