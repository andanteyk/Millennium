using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Stage
{
    [CreateAssetMenu(fileName = "Stage", menuName = "ScriptableObjects/StageData")]
    public class StageData : ScriptableObject
    {
        public EnemyData[] Enemies;
    }

    [Serializable]
    public class EnemyData
    {
        public float SpawnSeconds;
        public Vector2 Position;
        public GameObject Prefab;
    }
}
