using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public static GameCamera instance = null;//单例

    protected float m_distance = 10;        //摄像机离地面的距离
    protected Vector3 m_rotation = new Vector3(-55, 180, 0);//摄像机的角度
    protected float m_moveSpeed = 60;       //摄像机移动速度
    protected float m_vx = 0;               //摄像机的x移动值
    protected float m_vy = 0;               //摄像机的y移动值
    //自身位置组件
    protected Transform m_transform;
    //摄像机的目标点
    public Transform m_cameraPoint;

    //初始化
    private void Awake()
    {
        instance = this;
        m_transform = this.transform;
    }
    private void Start()
    {
        //获得摄像机的焦点
        //因为CameraPoint是单例，只会创建一个，所以不用Find，直接通过单例调用
        m_cameraPoint = CameraPoint.instance.transform;
        Follow();
    }

    //Update之后调用，和Update功能相同，这里是为了确保在Update之后所有操作完成再移动
    private void LateUpdate()
    {
        Follow();
    }

    //使摄像机对齐焦点
    void Follow()
    {
        //设置旋转角度
        m_cameraPoint.eulerAngles = m_rotation;
        //将摄像机移动到指定位置
        m_transform.position = m_cameraPoint.TransformPoint(new Vector3(0, 0, m_distance));
        //将摄像机镜头对准目标点
        m_transform.LookAt(m_cameraPoint);
    }

    //摄像机移动
    public void Control(bool mouse, float mx, float my)
    {
        if (!mouse)
            return;
        m_cameraPoint.eulerAngles = Vector3.zero;//Zero？为什么
        //平移摄像机
        m_cameraPoint.Translate(-mx, 0, my);
    }
}
