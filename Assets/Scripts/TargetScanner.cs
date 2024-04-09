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
        if (CharacterCtrl.Instance == null)
            return null;

        var eyePos = detector.position + Vector3.up * _heightOffset;
        var toPlayer = CharacterCtrl.Instance.transform.position - eyePos;
        var toPlayerTop = CharacterCtrl.Instance.transform.position + Vector3.up * 1.5f - eyePos;
        if (useHeightDifference && Mathf.Abs(toPlayer.y + _heightOffset) > _maxHeightDifference)
            return null;

        var toPlayerFlat = toPlayer;
        toPlayerFlat.y = 0;
        if (toPlayerFlat.sqrMagnitude <= _detectionRadius * _detectionRadius)
        {
            if (Vector3.Dot(toPlayerFlat.normalized, detector.forward) >
                Mathf.Cos(_detectionAngle * 0.5f * Mathf.Deg2Rad))
            {
                var canSee = false;
                canSee |= !Physics.Raycast(eyePos, toPlayer.normalized, _detectionRadius,
                    _viewBlockerLayerMask, QueryTriggerInteraction.Ignore);
                canSee |= !Physics.Raycast(eyePos, toPlayerTop.normalized, toPlayerTop.magnitude, 
                    _viewBlockerLayerMask, QueryTriggerInteraction.Ignore);
                if (canSee)
                    return CharacterCtrl.Instance;
            }
        }

        return null;
    }
}
