using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*ScriptableObject是一个数据容器，可用于保存大量数据，而与类实例无关。
 * ScriptableObjects的主要用例之一是通过避免复制值来减少Project的内存使用量。
 * 如果您的项目有一个prefab
 * 在连接的MonoBehaviour Scripts中存储不变数据。
 * 每次实例化该Prefab时，它都会获得该数据的自己的副本。
 * 可以使用ScriptableObject来存储数据，
 * 然后通过引用来自所有Prefabs的方式访问它们，
 * 而不是使用该方法并存储重复的数据。
 * 这意味着内存中有一个数据副本。
 * 就像MonoBehaviours一样，ScriptableObjects派生自基本Unity对象，
 * 但与MonoBehaviours不同，您不能将ScriptableObject附加到GameObject。
 * 而是将它们另存为项目中的Assets。
 * 使用编辑器时，可以在编辑时和运行时将数据保存到ScriptableObjects，
 * 因为ScriptableObjects使用Editor命名空间和Editor脚本。
 * 但是，在已部署的内部版本中，不能使用ScriptableObjects保存数据，
 * 但是可以使用在开发过程中设置的ScriptableObject Assets中保存的数据。
 * 您从编辑器工具作为Assets保存到ScriptableObjects的数据将写入磁盘，
 * 因此在会话之间是持久的。
 */
public class PathTool : ScriptableObject
{
    static PathNode m_prev = null;

    //创建一个新的路点
    [MenuItem("PathTool/CreatePathNode")]
    static void CreatePathNode()
    {
        GameObject go = new GameObject();
        go.AddComponent<PathNode>();
        go.name = "PathNode";
        go.tag = "PathNode";
        Selection.activeTransform = go.transform;
    }

    //设置父路点
    [MenuItem("PathTool/Set Prev %q")]
    static void SetPrev()
    {
        if (!Selection.activeGameObject || Selection.GetTransforms(SelectionMode.Unfiltered).Length > 1)
            return;
        if(Selection.activeGameObject.tag.CompareTo("PathNode") == 0)
        {
            m_prev = Selection.activeGameObject.GetComponent<PathNode>();
        }
    }

    //设置子路点
    [MenuItem("PathTool/Set Next %w")]
    static void SetNext()
    {
        if (!Selection.activeGameObject || m_prev == null || Selection.GetTransforms(SelectionMode.Unfiltered).Length > 1)
            return;
        if(Selection.activeGameObject.tag.CompareTo("PathNode") == 0)
        {
            m_prev.SetNextNode(Selection.activeGameObject.GetComponent<PathNode>());
            m_prev = null;
        }
    }
}