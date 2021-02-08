using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public PathNode m_currentNode;          //当前的目标路点
    public int m_life = 15;                 //当前生命值
    public int m_maxLife = 15;              //最大生命值
    public float m_speed = 2;               //速度

    protected Transform m_lifeBarObj;                 //生命条位置组件
    protected UnityEngine.UI.Slider m_lifebar;        //生命条滑动组件

    //本质是委托
    public System.Action<Enemy> onDeath;

    protected virtual void Start()
    {
        GameManager.instance.m_enemys.Add(this);
        //读取生命条prefab并放置到场景中
        GameObject prefab = Resources.Load<GameObject>("Canvas3D");
        m_lifeBarObj = Instantiate(prefab, Vector3.zero, Camera.main.transform.rotation, this.transform).transform;
        m_lifeBarObj.localPosition = new Vector3(0, 200.0f, 0);//放在敌人头上
        //m_lifeBarObj.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        m_lifebar = m_lifeBarObj.GetComponentInChildren<UnityEngine.UI.Slider>();
        StartCoroutine(UpdateLifebar());//开启协程
    }

    // Update is called once per frame
    void Update()
    {
        RotateTo();//转向
        MoveTo();//移动
    }

    //更新生命条
    protected IEnumerator UpdateLifebar()
    {
        //更新生命条的值
        m_lifebar.value = (float)m_life / (float)m_maxLife;
        //面向摄像机
        m_lifeBarObj.transform.eulerAngles = Camera.main.transform.eulerAngles;
        yield return 0;//不等待
        StartCoroutine(UpdateLifebar());//循环
    }

    //转向目标路点
    public void RotateTo()
    {
        var position = m_currentNode.transform.position - transform.position;
        position.y = 0;
        var targetRotation = Quaternion.LookRotation(position);
        float next = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, 720 * Time.deltaTime);
        this.transform.eulerAngles = new Vector3(0, next, 0);
    }

    //像目标路点移动，若到达，进行判断
    public void MoveTo()
    {
        Vector3 pos1 = this.transform.position;
        Vector3 pos2 = m_currentNode.transform.position;
        float dist = Vector2.Distance(new Vector2(pos1.x, pos1.z), new Vector2(pos2.x, pos2.z));
        if(dist < 0.1f)
        {
            if(m_currentNode.m_next == null)
            {
                GameManager.instance.SetDamage(1);
                DestroyMe();//不直接调用Destory方法
            }
            else
            {
                m_currentNode = m_currentNode.m_next;
            }
        }
        //移动
        this.transform.Translate(new Vector3(0, 0, m_speed * Time.deltaTime));
    }

    public void DestroyMe()
    {
        GameManager.instance.m_enemys.Remove(this);
        onDeath(this);//发布死亡消息
        Destroy(this.gameObject);
    }

    //受伤
    public void SetDamage(int damage)
    {
        m_life -= damage;
        if(m_life <= 0)
        {
            m_life = 0;
            GameManager.instance.SetPoint(5);
            DestroyMe();
        }
    }
}
