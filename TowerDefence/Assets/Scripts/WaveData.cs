using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]//序列化属性，添加该属性才能序列化
public class WaveData
{
    public int wave = 0;//波数
    public List<GameObject> enemyPrefab;//敌人的prefab
    public int level = 1;//敌人的等级
    public float interval = 3;//创建敌人的间隔时间
}
