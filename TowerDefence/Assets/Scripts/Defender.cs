using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender : MonoBehaviour
{
    //枚举，代表格子的状态
    public enum TileStatus
    {
        DEAD = 0,
        ROAD = 1,
        GUARD = 2,
    }

    public float m_attackArea = 2.0f;           //攻击范围
    public int m_power = 1;                     //攻击力
    public float m_attackInterval = 2.0f;       //攻击时间间隔
    protected Enemy m_targetEnemy;              //目标敌人
    protected bool m_isFaceEnemy;               //是否面对敌人
    protected GameObject m_model;               //prefab模型
    protected Animator m_anim;                  //动画

    //静态方法，用于创建防守单位的游戏体，并实现了泛型，创建任意Defender类的游戏体
    public static T Create<T>(Vector3 pos, Vector3 angle) where T : Defender
    {
        GameObject go = new GameObject("defender");
        go.transform.position = pos;
        go.transform.eulerAngles = angle;
        T d = go.AddComponent<T>();
        d.Init();
        //修改格子信息，放上单位后不能再放其他的单位，所以设置为Dead
        TileObject.instance.SetDataFromPosition(d.transform.position.x, d.transform.position.z, (int)TileStatus.DEAD);
        return d;
    }

    //初始化，虚函数，不同的子类的数据不同
    protected virtual void Init()
    {
        //数值设置，一般用数据库处理
        m_attackArea = 2.0f;
        m_power = 2;
        m_attackInterval = 2.0f;

        //创建模型
        CreateModel("swordman");
        //开启协程，实现攻击逻辑
        StartCoroutine(Attack());
    }

    //创建对应的模型prefab，虚函数，不同的子类的模型不同
    protected virtual void CreateModel(string myname)
    {
        GameObject model = Resources.Load<GameObject>(myname);
        m_model = Instantiate(model, this.transform.position, this.transform.rotation, this.transform);
        m_anim = m_model.GetComponent<Animator>();
    }

    private void Update()
    {
        //寻找敌人
        FindEnemy();
        //转向敌人
        RotateTo();
        //攻击逻辑
        Attack();
    }

    //转向敌人
    public void RotateTo()
    {
        if (m_targetEnemy == null)
            return;

        //计算方向
        var targetDir = m_targetEnemy.transform.position - this.transform.position;
        targetDir.y = 0;
        Vector3 rot_delta = Vector3.RotateTowards(this.transform.forward, targetDir, 20.0f * Time.deltaTime, 0.0F);
        Quaternion targetRotation = Quaternion.LookRotation(rot_delta);

        float angle = Vector3.Angle(targetDir, transform.forward);
        if(angle < 1.0f)//小于1度时认为已经面向敌人
        {
            m_isFaceEnemy = true;
        }
        else
        {
            m_isFaceEnemy = false;
        }

        transform.rotation = targetRotation;
    }

    //寻找目标敌人
    void FindEnemy()
    {
        if (m_targetEnemy != null)
            return;

        m_targetEnemy = null;
        int minlife = 0;
        //通过GameManager里的敌人列表遍历
        foreach(Enemy e in GameManager.instance.m_enemys)
        {
            if (e.m_life == 0)
                continue;

            //计算距离
            Vector3 pos1 = this.transform.position;
            Vector3 pos2 = e.transform.position;
            pos1.y = 0;
            pos2.y = 0;
            float dist = Vector3.Distance(pos1, pos2);
            //在攻击范围内的生命值最低的确定为目标敌人
            if(dist > m_attackArea)
            {
                continue;
            }
            if(minlife == 0 || minlife > e.m_life)
            {
                m_targetEnemy = e;
                minlife = e.m_life;
            }
        }
    }

    //攻击逻辑，虚函数，不同子类的攻击逻辑不同
    protected virtual IEnumerator Attack()
    {
        //未找到敌人或未面向敌人不攻击
        while (m_targetEnemy == null || !m_isFaceEnemy)
            yield return 0;
        //播放攻击动画
        m_anim.CrossFade("attack", 0.1f);
        //等待进入攻击动画，如果当前不是攻击动画则等待
        while(!m_anim.GetCurrentAnimatorStateInfo(0).IsName("attack"))
        {
            yield return 0;
        }

        //攻击动画的长度
        float anim_length = m_anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(anim_length * 0.5f);
        //在完成动画的攻击动作后计算伤害，再播放另一半的动画
        if (m_targetEnemy != null)
        {
            m_targetEnemy.SetDamage(m_power);
            m_targetEnemy = null;
        }
        yield return new WaitForSeconds(anim_length * 0.5f);
        //由于不明原因，攻击动画不会结束，所以添加该句代码来手动更改
        m_anim.CrossFade("idle", 0.1f);
        //等待攻击间隔
        yield return new WaitForSeconds(m_attackInterval);

        m_targetEnemy = null;
        //继续下一次攻击，开启协程
        StartCoroutine(Attack());
    }
}
