using System.Collections.Generic;
using UnityEngine;

/// <summary>targetの周りに敵を円弧状に配置し、各敵がtargetに対する異なる方向から攻撃できるようにする</summary>
[DefaultExecutionOrder(-1)]
public class TargetDistributor : MonoBehaviour
{
    /// <summary>targetに従う敵</summary>
    public class TargetFollower
    {
        public bool _requireSlot;　//targetの周りが空いているか
        public int _assignedSlot;　//割り当てられたslot
        public Vector3 _requiredPoint;
        public TargetDistributor _distributor;

        public TargetFollower(TargetDistributor owner)
        {
            _distributor = owner;
            _requiredPoint = Vector3.zero;
            _requireSlot = false;
            _assignedSlot = -1;
        }
    }

    public int _arcsCount;
        private Vector3[] _worldDirection;
        private bool[] _freeArcs;
        private float arcDegree;
        private List<TargetFollower> _followers;

        /// <summary>targetの周りに配置された弧状の方向を計算し空きslotを初期化</summary>
        public void OnEnable()
        {
            _worldDirection = new Vector3[_arcsCount];
            _freeArcs = new bool[_arcsCount];

            _followers = new List<TargetFollower>();
            
            arcDegree = 360.0f / _arcsCount; //1つの弧の角度を計算
            var rotation = Quaternion.Euler(0, -arcDegree, 0);
            var currentDirection = Vector3.forward;
            for (var i = 0; i < _arcsCount; ++i)
            {
                _freeArcs[i] = true;
                _worldDirection[i] = currentDirection;
                currentDirection = rotation * currentDirection;
            }
        }

        /// <summary>新しい敵をtargetに登録</summary>
        public TargetFollower RegisterNewFollower()
        {
            var follower = new TargetFollower(this); 
            _followers.Add(follower);
            return follower;
        }

        /// <summary>指定されたTargetFollowerObjectをTargetから登録解除</summary>
        public void UnregisterFollower(TargetFollower follower)
        {
            if (follower._assignedSlot != -1)
            {
                //followerが特定のslotに割り当てられている場合そのスロットを解放
                _freeArcs[follower._assignedSlot] = true;
            }

            _followers.Remove(follower);
        }

        private void LateUpdate()
        {
            for (var i = 0; i < _followers.Count; ++i)
            {
                var follower = _followers[i];
                //特定のslotに割り当てられている場合そのslotを解放
                if (follower._assignedSlot != -1)
                {
                    _freeArcs[follower._assignedSlot] = true;
                }

                //followerがslotを要求している場合followerに新しいslotを取得させる
                if (follower._requireSlot)
                {
                    follower._assignedSlot = GetFreeArcIndex(follower);
                }
            }
        }

        /// <summary>空いているfollowerに対して空いている弧のindexを取得させる</summary>
        private int GetFreeArcIndex(TargetFollower follower)
        {
            var found = false;

            //followerがtargetを中心にどの方向に配置されるべきかを計算
            var wanted = follower._requiredPoint - transform.position;
            //targetの位置から少し高い場所に設定してRaycastが地面に到達するようにする
            var rayCastPosition = transform.position + Vector3.up * 0.4f;

            wanted.y = 0;
            var wantedDistance = wanted.magnitude; //followerの目標地点
            
            wanted.Normalize();

            //followerが目指す地点から見たTargetの方向の角度
            var angle = Vector3.SignedAngle(wanted, Vector3.forward, Vector3.up);
            if (angle < 0)
                angle = 360 + angle;

            //followerが目指す地点が存在する弧のindexを決定
            var wantedIndex = Mathf.RoundToInt(angle / arcDegree);
            //弧の数が余剰している場合は余剰を削除して適切なindexにする
            if (wantedIndex >= _worldDirection.Length)
                wantedIndex -= _worldDirection.Length;

            var chosenIndex = wantedIndex;

            RaycastHit hit;
            //Raycastを使用して選択された弧に沿って障害物があるかをチェック
            if (!Physics.Raycast(rayCastPosition, GetDirection(chosenIndex), out hit, wantedDistance))
                found = _freeArcs[chosenIndex];
            
            //選択された弧が空いていない場合に周囲の弧をテストして空いている弧を探す
            if (!found)
            {
                var offset = 1;　//テスト回数
                var halfCount = _arcsCount / 2;
                //最初に選択された弧の左右にある弧を順番にテストする
                while (offset <= halfCount)
                {
                    var leftIndex = wantedIndex - offset;
                    var rightIndex = wantedIndex + offset;

                    if (leftIndex < 0) leftIndex += _arcsCount;
                    if (rightIndex >= _arcsCount) rightIndex -= _arcsCount;

                    //空いている弧がみつかったらchosenIndexに選択する
                    if (!Physics.Raycast(rayCastPosition, GetDirection(leftIndex), wantedDistance) &&
                        _freeArcs[leftIndex])
                    {
                        chosenIndex = leftIndex;
                        found = true;
                        break;
                    }
                    if (!Physics.Raycast(rayCastPosition, GetDirection(rightIndex), wantedDistance) &&
                        _freeArcs[rightIndex])
                    {
                        chosenIndex = rightIndex;
                        found = true;
                        break;
                    }

                    offset += 1;
                }
            }

            //見つからなかった場合は-1を返す
            if (!found)
                return -1;

            //選択された弧を仕様済みとしてマークする
            _freeArcs[chosenIndex] = false;
            return chosenIndex;
        }

        public Vector3 GetDirection(int index)
        {
            return _worldDirection[index];
        }
}