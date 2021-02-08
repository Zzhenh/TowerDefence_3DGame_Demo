using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Defender
{
    //重写初始化
    protected override void Init()
    {
        //数值设置，一般使用数据库
        m_attackArea = 5.0f;
        m_power = 1;
        m_attackInterval = 1.0f;

        //创建游戏体模型prefab
        CreateModel("archer");

        //开始攻击逻辑,开启协程
        StartCoroutine(Attack());
    }

    //重写攻击逻辑
    protected override IEnumerator Attack()
    {
        //当没有目标敌人时或者未面向敌人时不攻击
        while (m_targetEnemy == null || !m_isFaceEnemy)
            yield return 0;

        //播放攻击动画
        m_anim.CrossFade("attack", 0.1f);
        //当当前动画不是攻击动画时等待
        while (!m_anim.GetCurrentAnimatorStateInfo(0).IsName("attack"))
            yield return 0;
        //获取攻击动画长度
        float anim_length = m_anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(anim_length * 0.5f);
        //攻击动画播放到对应动作创建弓箭，进行伤害判断
        if(m_targetEnemy != null)
        {
            Vector3 pos = this.m_model.transform.Find("atkpoint").position;

            Projectile.Create(m_targetEnemy.transform, pos, (Enemy e) =>
            {
                e.SetDamage(m_power);
                m_targetEnemy = null;
            });
        }
        yield return new WaitForSeconds(anim_length * 0.5f);
        //由于不明原因，攻击动画不会结束，所以添加该句代码来手动更改
        m_anim.CrossFade("idle", 0.1f);
        //等待攻击间隔
        yield return new WaitForSeconds(m_attackInterval);
        //下一次攻击，开启协程
        StartCoroutine(Attack());
    }
}
