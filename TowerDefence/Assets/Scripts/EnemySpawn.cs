using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public PathNode m_startNode;    //起始路点
    private int m_liveEnemy = 0;    //存活的敌人数量
    public List<WaveData> waves;    //战斗波数配置数组
    int enemyIndex = 0;             //生成敌人数组的下标
    int waveIndex = 0;              //战斗波数数组的下标

    private void Start()
    {
        //开启协程，开始生成敌人
        StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        //WaitForEndOfFrame会在一帧结束后调用
        //准确来说，会在LateUpdate之后调用
        //区别于yield return null在Update之后调用
        yield return new WaitForEndOfFrame();
        //更新UI的波数
        GameManager.instance.SetWave(waveIndex + 1);
        //获取当前波的信息配置
        WaveData wave = waves[waveIndex];
        //根据配置的间隔时间生成敌人
        yield return new WaitForSeconds(wave.interval);

        //如果生成的敌人比配置数少
        while(enemyIndex < wave.enemyPrefab.Count)
        {
            //生成敌人
            Vector3 dir = m_startNode.transform.position - this.transform.position;
            GameObject enemyObj = Instantiate(wave.enemyPrefab[enemyIndex], transform.position, Quaternion.LookRotation(dir));
            //获取Enemy脚本，设置初始路点
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            enemy.m_currentNode = m_startNode;

            //设置敌人数值，一般来说通过数据库设置
            enemy.m_life = wave.level * 3;
            enemy.m_maxLife = enemy.m_life;

            //增加敌人数量
            m_liveEnemy++;
            //当敌人死掉时减少敌人数量
            //lambda表达式调用Action实现委托
            enemy.onDeath = new System.Action<Enemy>((Enemy e) => { m_liveEnemy--; });
            //更新敌人数组下标
            enemyIndex++;
            //根据时间间隔生成敌人
            yield return new WaitForSeconds(wave.interval);
        }

        //等待敌人全部被消灭
        //因为一波的敌人在上面的循环全部被创建出来了，要等到全部被消灭才开始下一波
        while(m_liveEnemy > 0)
        {
            yield return 0;
        }

        //重设敌人数组下标
        enemyIndex = 0;
        //更新战斗波数
        waveIndex++;
        //如果不是最后一波，继续开始协程
        if(waveIndex < waves.Count)
        {
            StartCoroutine(SpawnEnemy());
        }
        else
        {
            //胜利代码
        }
    }

    //在场景中显示为一个图标
    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "spawner.tif");
    }
}
