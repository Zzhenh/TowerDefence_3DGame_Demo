using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    //单例模式
    public static TileObject instance = null;

    public LayerMask tileLayer;         //tile碰撞层
    public float tileSize = 1;          //格子大小
    public int xTileCount = 2;          //x轴格子数
    public int zTileCount = 2;          //z轴格子数

    //存储每个格子的数值
    //0表示锁定，无法摆放物体；1表示敌人通道，2表示可摆放防守单位
    public int[] data;                  
    //当前的数据ID（高亮某一种格子用）
    [HideInInspector]
    public int dataID = 0;
    //是否显示数据信息
    [HideInInspector]
    public bool debug = false;

    private void Awake()
    {
        //单例初始化
        instance = this;
    }

    //Reset在编辑状态下添加时会调用，用于改变物体的设置
    //如批量改名
    public void Reset()
    {
        //初始化地图数据
        data = new int[xTileCount * zTileCount];
    }

    //通过坐标返回格子值，如果不在范围内一律返回0
    //计算方法就是用参数坐标减去地图坐标然后除格子大小取整得出是第几个格子
    public int GetDataFromPosition(float pox, float poz)
    {
        int index = (int)((pox - transform.position.x) / tileSize) * zTileCount
                        + (int)((poz - transform.position.z) / tileSize);
        if (index < 0 || index >= data.Length)
            return 0;
        return data[index];
    }

    //通过坐标设置格子值
    //计算方法同上
    public void SetDataFromPosition(float pox, float poz, int number)
    {
        int index = (int)((pox - transform.position.x) / tileSize) * zTileCount
                        + (int)((poz - transform.position.z) / tileSize);
        if (index < 0 || index >= data.Length)
            return;
        data[index] = number;
    }

    //在场景中显示格子数据
    private void OnDrawGizmos()
    {
        //是否显示
        if (!debug)
            return;
        //错误提示
        if(data == null)
        {
            Debug.Log("Please reset data first");
            return;
        }

        Vector3 pos = transform.position;//地图原点位置

        //画出z轴辅助线，并设置高亮
        for(int i = 0; i < xTileCount; i++)
        {
            Gizmos.color = new Color(0, 0, 1, 1);
            //DrawLine方法，画线，参数为起点和终点
            Gizmos.DrawLine(pos + new Vector3(tileSize * i, pos.y, 0),
                        transform.TransformPoint(tileSize * i, pos.y, tileSize * zTileCount));

            //设置高亮
            for(int j = 0; j < zTileCount; j++)
            {
                if((i * zTileCount + j) < data.Length && data[i * zTileCount + j] == dataID)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.3f);
                    //DrawCube画长方体，参数是中心点和三个方向的长
                    Gizmos.DrawCube(new Vector3(pos.x + i * tileSize + tileSize * 0.5f, pos.y,
                                pos.z + j * tileSize + tileSize * 0.5f), new Vector3(tileSize, 0.2f, tileSize));
                }
            }
        }

        //x轴辅助线
        for(int j = 0; j < zTileCount; j++)
        {
            Gizmos.color = new Color(0, 0, 1, 1);
            Gizmos.DrawLine(pos + new Vector3(0, pos.y, tileSize * j),
                        transform.TransformPoint(tileSize * xTileCount, pos.y, tileSize * j));
        }
    }
}
