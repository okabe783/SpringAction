using UnityEngine;

/// <summary>playerを見つける為に使用</summary>
[System.Serializable]
public class TargetScanner
{
    public float _heightOffset = 0.0f; //高さのoffset
    public float _detectionRadius = 10; //半径
    [Range(0.0f, 360.0f)] public float _detectionAngle = 270;　//角度
    public float _maxHeightDifference = 1.0f;　//高さの差
    public LayerMask _viewBlockerLayerMask;

    /// <summary>指定されたTransformからプレイヤーを検出</summary>
    public CharacterCtrl Detect(Transform detector, bool useHeightDifference = true)
    {
        //playerがrespawnしていないか実行中ならplayerをtargetにしない
        if (CharacterCtrl.Instance == null)
            return null;

        //detectorの位置に目の位置を設定
        var eyePos = detector.position + Vector3.up * _heightOffset;
        //playerまでのベクトルを計算
        var toPlayer = CharacterCtrl.Instance.transform.position - eyePos;
        //playerの頭の位置までのベクトルを計算
        var toPlayerTop = CharacterCtrl.Instance.transform.position + Vector3.up * 1.5f - eyePos;
        //高さの差を使用しその高さの差がmaxHeight...を超える場合は追跡を放棄
        if (useHeightDifference && Mathf.Abs(toPlayer.y + _heightOffset) > _maxHeightDifference)
            return null;

        //水平方向のベクトルを計算
        var toPlayerFlat = toPlayer;
        toPlayerFlat.y = 0;
        //playerがdetection...以内にいる場合およびplayerの方向検出角度内にある場合に処理を続行
        if (toPlayerFlat.sqrMagnitude <= _detectionRadius * _detectionRadius)
        {
            if (Vector3.Dot(toPlayerFlat.normalized, detector.forward) >
                Mathf.Cos(_detectionAngle * 0.5f * Mathf.Deg2Rad))
            {
                var canSee = false;
                //直接playerをみることができているか
                canSee |= !Physics.Raycast(eyePos, toPlayer.normalized, _detectionRadius,
                    _viewBlockerLayerMask, QueryTriggerInteraction.Ignore);
                //playerの頭の位置を見ることができるかどうかを判定
                canSee |= !Physics.Raycast(eyePos, toPlayerTop.normalized, toPlayerTop.magnitude,
                    _viewBlockerLayerMask, QueryTriggerInteraction.Ignore);
                if (canSee)
                    return CharacterCtrl.Instance;
            }
        }

        return null;
    }
#if UNITY_EDITOR

    public void EditorGizmo(Transform transform)
    {
        var c = new Color(0, 0, 0.7f, 0.4f);

        UnityEditor.Handles.color = c;
        var rotatedForward = Quaternion.Euler(0, -_detectionAngle * 0.5f, 0) * transform.forward;
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, rotatedForward, _detectionAngle,
            _detectionRadius);

        Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * _heightOffset,0.2f);
    }
    #endif
}
