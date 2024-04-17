using UnityEngine;

public class GolemSMBReturn : SceneLinkedSMB<EnemyBehavior>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _monoBehaviour.WalkBackToBase();
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        base.OnSLStateNoTransitionUpdate(animator,stateInfo,layerIndex);
        _monoBehaviour.FindTarget();

        if (_monoBehaviour.target != null)
            _monoBehaviour.StartPursuit(); //もしplayerが範囲内に侵入してきたら追跡を再開する
        else
            _monoBehaviour.WalkBackToBase();
    }
}
