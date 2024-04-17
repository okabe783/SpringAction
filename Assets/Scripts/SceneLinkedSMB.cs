using UnityEngine;

public class SceneLinkedSMB<TMonoBehaviour> : SealedSMB
where TMonoBehaviour : MonoBehaviour
{
    protected TMonoBehaviour _monoBehaviour;

    private bool _firstFrameHappened;
    private bool _lastFrameHappened;

    /// <summary>
    /// Animatorに紐付けされたSceneLinkedSMBのinstanceを初期化 </summary>
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

    /// <summary>PostEnter後、状態が他の状態に遷移していない間、毎フレーム呼び出される</summary>
    public virtual void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){ }
    
    /// <summary>アニメーションステートが実行され始める直前(ステートへの遷移時)に呼び出される</summary>
    public virtual void OnSLStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo,int layerIndex){ }
    
    /// <summary> Updateの後stateの実行が最初に終了した時（ステートから遷移した後）に呼び出す</summary>
    public virtual void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    
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
