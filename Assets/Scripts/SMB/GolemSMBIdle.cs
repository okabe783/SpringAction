using UnityEngine;

/// <summary>ゴーレムのIdle状態を制御</summary>
public class GolemSMBIdle : SceneLinkedSMB<EnemyBehavior>
{
    public float _minimumIdleGruntTime = 2.0f;　//最小のIdle時間
    public float maximumIdleGruntTime = 5.0f; //最大のIdle時間

    private float _remainingToNextGrunt = 0.0f;　//咆哮の時間

    /// <summary>次のグルントまでの時間を設定</summary>
    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (_minimumIdleGruntTime > maximumIdleGruntTime)
            _minimumIdleGruntTime = maximumIdleGruntTime;

        _remainingToNextGrunt = Random.Range(_minimumIdleGruntTime, maximumIdleGruntTime);
    }

    /// <summary>
    /// animStateが遷移しない間呼び出され残り時間を減算し0になるとrandomな時間を再設定しグルントを再生
    /// </summary>
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateNoTransitionUpdate(animator,stateInfo,layerIndex);

        _remainingToNextGrunt -= Time.deltaTime;

        if (_remainingToNextGrunt < 0)
        {
            _remainingToNextGrunt = Random.Range(_minimumIdleGruntTime, maximumIdleGruntTime);
        }
        _monoBehaviour.FindTarget();
        if (_monoBehaviour.target != null)
        {
            _monoBehaviour.StartPursuit();
        }
    }
}
