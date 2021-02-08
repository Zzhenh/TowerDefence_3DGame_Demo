using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public PathNode m_prev;//前一个路点
    public PathNode m_next;//下一个路点

    //设置下一个路点
    public void SetNextNode(PathNode node)
    {
        if(m_next != null)
        {
            m_next.m_prev = null;
        }
        m_next = node;
        node.m_prev = this;
    }

    //在场景内显示为图标
    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(this.transform.position, "Node.tif");
    }
}
