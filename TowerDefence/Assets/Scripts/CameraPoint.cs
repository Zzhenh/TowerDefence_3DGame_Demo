using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPoint : MonoBehaviour
{
    public static CameraPoint instance = null;//单例

    private void Awake()
    {
        instance = this;
    }

    //在场景中显示一个图标
    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(this.transform.position, "CameraPoint.tif");
    }
}
