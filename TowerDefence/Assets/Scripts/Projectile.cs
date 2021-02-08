using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //攻击事件，当打击到目标时执行的动作
    System.Action<Enemy> onAttack;

    Transform m_target;//目标对象位置

    //表示轴对齐的边界框。
    //轴对齐的边界框或简称AABB，是与坐标轴对齐并完全封闭某些对象的框。
    //由于该框永远不会相对于轴旋转，
    //因此可以仅通过其中心和范围或最小和最大点来定义它 。
    Bounds m_targetCenter;//目标对象模型的边框

    //静态方法，创建弓箭
    public static void Create(Transform target, Vector3 spawnPos, System.Action<Enemy> onAttack)
    {
        //读取弓箭模型，放置到场景中
        GameObject prefab = Resources.Load<GameObject>("arrow");
        GameObject go = Instantiate(prefab, spawnPos, Quaternion.LookRotation(target.position - spawnPos));

        //获取Projectile类，设置对应数据
        Projectile arrowModel = go.AddComponent<Projectile>();
        arrowModel.m_target = target;
        //获得目标对象模型的边框
        arrowModel.m_targetCenter = target.GetComponentInChildren<SkinnedMeshRenderer>().bounds;
        arrowModel.onAttack = onAttack;
        //3秒自动销毁
        Destroy(go, 3.0f);
    }

    private void Update()
    {
        //瞄准目标中心位置
        if (m_target != null)
            this.transform.LookAt(m_targetCenter.center);
        //像目标移动
        this.transform.Translate(new Vector3(0, 0, 10 * Time.deltaTime));
        if(m_target != null)
        {
            //距离检测是否击中目标
            if(Vector3.Distance(this.transform.position, m_targetCenter.center) < 0.5f)
            {
                onAttack(m_target.GetComponent<Enemy>());
                Destroy(this.gameObject);
            }
        }
    }
}
