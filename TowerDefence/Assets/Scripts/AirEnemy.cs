using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirEnemy : Enemy
{
    protected override void Start()
    {
        GameManager.instance.m_enemys.Add(this);
        //读取生命条prefab并放置到场景中
        GameObject prefab = Resources.Load<GameObject>("Canvas3D");
        m_lifeBarObj = Instantiate(prefab, Vector3.zero, Camera.main.transform.rotation, this.transform).transform;
        m_lifeBarObj.localPosition = new Vector3(0, 2.0f, 0);//放在敌人头上
        m_lifeBarObj.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        m_lifebar = m_lifeBarObj.GetComponentInChildren<UnityEngine.UI.Slider>();
        StartCoroutine(UpdateLifebar());//开启协程
    }

    void Update()
    {
        RotateTo();
        MoveTo();
        Fly(); //飞行
    }

    public void Fly()
    {
        float flySpeed = 0;
        if(this.transform.position.y < 2.0f)
        {
            flySpeed = 1.0f;
        }
        this.transform.Translate(new Vector3(0, flySpeed * Time.deltaTime, 0));
    }
}
