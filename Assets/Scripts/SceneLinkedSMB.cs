using UnityEngine;

public class SceneLinkedSMB<TMonoBehaviour> : SealedSMB
where TMonoBehaviour : MonoBehaviour
{
    protected TMonoBehaviour _monoBehaviour;

    private bool _firstFrameHappened;
    private bool _lastFrameHappened;

    /// <summary>
    /// Animatorに紐付けされたSceneLinkedSMBのinstanceを初期化
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="monoBehaviour"></param>
    public static void Initialise(Animator animator, TMonoBehaviour monoBehaviour)
    {
        var sceneLinkedSMBs = animator.GetBehaviours<SceneLinkedSMB<TMonoBehaviour>>();

        foreach (var t in sceneLinkedSMBs)
        {
            t.InternalInitialise(animator,monoBehaviour);
        }
    }

    /// <summary>OnStartを呼び出して初期化</summary>
    private void InternalInitialise(Animator animator, TMonoBehaviour monoBehaviour)
    {
        _monoBehaviour = monoBehaviour;
        OnStart(animator);
    }
    
    /// <summary>初期化メソッド</summary>
    protected virtual void OnStart(Animator animator){ }
}
/// <summary>シールド化して継承先でoverrideできないようにする</summary>
public abstract class SealedSMB : StateMachineBehaviour
{
    public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){ }
        
    public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){ }
        
    public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){ }
}
