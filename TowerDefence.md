# TowerDefence-塔防游戏

### 地图编辑器

**格子数据**

1.创建一个空物体
2.创建一个脚本TileObject，添加给空物体
```c#
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
    private void Reset()
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
```

**在Inspector窗口中添加自定义UI控件**

1.创建脚本TileEditor，继承自Editor
```c#
using UnityEngine;
using UnityEditor;

//定义定制编辑器类可以编辑的对象类型。
[CustomEditor(typeof(TileObject))]
//用于解决Muti-object editing not supported
[CanEditMultipleObjects]
public class TileEditor : Editor
{
    protected bool editMode = false;        //是否是编辑模式
    protected TileObject tileObject;        //TileObject脚本

    //开始时调用
    private void OnEnable()
    {
        //Editor.target：所检查的对象
        //这里的相关知识我还不明白，target的检查原理和过程
        //获得tile脚本
        tileObject = (TileObject)target;
    }

    //使编辑器可以在“场景”视图中处理事件。
    /*在OnSceneGUI中，您可以执行例如网格编辑，地形绘制或高级Gizmos。
     * 如果Event.current.Use() 调用，则事件将由编辑器“吃掉”，
     * 而不由“场景”视图本身使用。
     */
     //这里不是很明白，暂且做记录
    //更改场景中的操作
    private void OnSceneGUI()
    {
        //如果在编辑模式
        if(editMode)
        {
            /*Handles,HandleUtility——控制柄工具类，功能相当强大，
             * 引擎场景视图界面也是用这个类搞定的
             * Handles主要提供用于在SceneView中绘制各种控制柄的方法。
             * HandleUtility用户场景视图样式3D GUI的辅助函数
             * 主要提供了一些算法，如各种距离计算方法以及物体选择的方法。
             */
            //AddDefaultControl添加默认控件的ID。如果没有其他选择，将选择此选项。

            /*GUIUtility用于创建新 GUI 控件的 Utility 类。
             * GetControlID为控件获取一个唯一的 ID
             * FocusType由 GUIUtility.GetControlID 使用，
             * 用于告知 IMGUI 系统给定控件是否能够获得键盘焦点。
             * 当用户按 Tab 键在控件之间进行循环时，可使 IMGUI 系统相应地提供焦点。
             * passive:不能接受键盘焦点
             */
             //ID是做什么的？

            //取消编辑器的选择功能
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            //在编辑器中显示数据（通过TileObject类中的方法）
            tileObject.debug = true;
            //获取Input事件（如何获得的？）
            //current：现在正在处理的当前事件
            Event e = Event.current;

            //如果摁下了鼠标左键
            if(e.button == 0 
                && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) 
                && !e.alt)
            {
                //鼠标位置产生的射线
                //GUIPointToWorldRay
                //将2D GUI位置转换为世界空间射线。
                //使用当前相机计算光线
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                //计算碰撞
                RaycastHit info;
                if(Physics.Raycast(ray, out info, 2000,tileObject.tileLayer))
                {
                    //设置数据
                    tileObject.SetDataFromPosition(info.point.x, info.point.z, tileObject.dataID);
                }
            }
        }
        //重新绘制当前视图
        HandleUtility.Repaint();
    }

    //OnInspectorGUI()是Unity的Editor类里的相关函数，
    //通过对该方法的重写，可以自定义对Inspector面板的绘制。
    public override void OnInspectorGUI()
    {
        //GUILayout类是具有自动布局的Unity gui的接口。
        //EditorGUILayout是EditorGUI 的自动布局版本
        //标签，编辑器名称
        GUILayout.Label("Tile Editor");
        //开关，是否启用编辑模式
        editMode = EditorGUILayout.Toggle("Edit", editMode);
        //开关，是否显示帮助信息
        tileObject.debug = EditorGUILayout.Toggle("Debug", tileObject.debug);

        //数据选项
        string[] editDataStrs = { "Dead", "Road", "Guard" };
        tileObject.dataID = GUILayout.Toolbar(tileObject.dataID, editDataStrs);

        EditorGUILayout.Separator();//分隔符
        if(GUILayout.Button("Reset"))//重置按钮
        {
            //初始化
            tileObject.Reset();
        }
        //绘制内置的检查器
        DrawDefaultInspector();
    }
}
```
2.添加一个碰撞Layer，设置为tile,设置对应的参数
3.创建一个Plane，置于TileObject下，将Layer设置为tile，取消MeshRenderer
4.绘制地图

**创建一个自定义窗口**

1.在Editor文件夹中创建自定义窗口脚本（继承EditorWindow
```c#
using UnityEngine;
using UnityEditor;

/*
 * EditorWindow从此类派生以创建编辑器窗口。
 * 创建自己的自定义编辑器窗口，该窗口可以自由浮动或作为选项卡停靠，
 * 就像Unity界面中的本机窗口一样。
 */
public class TileWnd : EditorWindow
{
    //TileObject脚本
    protected static TileObject tileObject;

    //菜单栏选项
    /*MenuItem属性使您可以将菜单项添加到主菜单和检查器上下文菜单。
     * MenuItem属性将任何静态功能转换为菜单命令。
     * 仅静态函数可以使用MenuItem属性
     */
    [MenuItem("Tools/Tile Window")]
    static void Create()
    {
        //EditorWindow中的方法，获取自身的窗口
        //返回t当前在屏幕上的第一个类型的EditorWindow 。
        //如果没有，则创建并显示新窗口并返回其实例
        GetWindow(typeof(TileWnd));

        //Selection类是编辑器类，使用需要using UnitryEditor;且脚本要放在Editor文件夹。
        //返回活跃的Transform。（Inspector中显示的那个）。
        //这将永远不会返回预制件或不可修改的对象。
        if (Selection.activeTransform != null)
        {
            //寻找TileObject脚本实例
            tileObject = Selection.activeTransform.GetComponent<TileObject>();
        }
    }

    //Selection更改时调用。
    private void OnSelectionChange()
    {
        if(Selection.activeTransform != null)
        {
            tileObject = Selection.activeTransform.GetComponent<TileObject>();
        }
    }

    //UI界面
    private void OnGUI()
    {
        if (tileObject == null)
            return;
        //编辑器名称
        GUILayout.Label("TileEditor");
        //读取一张贴图
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/td/GUI/butPlayer1.png");
        //在窗口内显示贴图
        GUILayout.Label(tex);
        //是否显示帮助信息
        tileObject.debug = EditorGUILayout.Toggle("Debug", tileObject.debug);
        //切换TileObject的数据
        string[] editDataStrs = { "Dead", "Road", "Guard" };
        tileObject.dataID = GUILayout.Toolbar(tileObject.dataID, editDataStrs);
        EditorGUILayout.Separator();//分隔符
        if(GUILayout.Button("Reset"))//重置按钮
        {
            tileObject.Reset();
        }
    }
}
```
2.在场景中选择TileObject实例，然后在菜单栏中打开Tools-TileWindow，打开自定义窗口

### 制作游戏场景

**游戏场景**

1.游戏场景由Sprite拼接而成，由于不会其他方法，只能手动添加，这里进行简化，三种数据分别对应三个图片，手动排列好

### 制作UI

**UI界面**

1.将图片设置为Sprite类型
2.创建Text并调整位置（波数，金钱，生命）
3.将Canvas的CanvasScaler设置为Scale With Screen Size模式（随不同分辨率自动缩放）
4.选择文字，Component-UI-Effects-Outline文字描边效果
5.创建按钮（创建防守单位和重新开始），Image组件中的Set Native Size可以使按钮的大小适配图片

### 创建游戏管理器

**游戏管理器**

1.创建脚本GameManager,添加到Canvas上
```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;               //UI控件命名空间
using UnityEngine.Events;           //UI事件命名空间
using UnityEngine.EventSystems;     //UI事件命名空间

public class GameManager : MonoBehaviour
{
    public static GameManager instance;//单例

    //数据
    public LayerMask m_groundLayer; //地面的layer
    public int m_wave = 1;
    public int m_life = 10;
    public int m_waveMax = 10;
    public int m_point = 30;

    //UI控件
    public Text m_Text_Wave;
    public Text m_Text_Life;
    public Text m_Text_Point;
    public Button m_Button_Reset;

    //当前是否选中了创建防守单位的按钮
    bool m_isSelectedButton = false;

    //初始化单例
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //UnityAction本质上是delegate，且有数个泛型版本（参数最多是4个）,
        //一个UnityAction可以添加多个函数（多播委托）

        //UnityAction是UnityEvent使用的零参数委托。

        /* UnityEvent本质上是继承自UnityEventBase的类，
         * 它的AddListener()方法能够注册UnityAction，
         * RemoveListener能够取消注册UnityAction，
         * 还有Invoke()方法能够一次性调用所有注册了的UnityAction。
         * UnityEvent也有数个泛型版本（参数最多也是4个），但要注意的一点是，
         * UnityAction的所有带参数的泛型版本都是抽象类（abstract），
         * 所以如果要使用的话，需要自己声明一个类继承之，
         * 然后再实例化该类才可以使用。
         */

        //BaseEventData:包含新EventSystem中所有事件类型共有的基本事件数据的类。
        UnityAction<BaseEventData> downAction = 
            new UnityAction<BaseEventData>(OnButtonCreateDefenderDown);
        UnityAction<BaseEventData> upAction =
            new UnityAction<BaseEventData>(OnButtonCreateDefenderUp);
        //Entry:EventSystem委托列表中的一个条目。
        //它存储回调以及应触发该回调的事件类型。

        //EventTrigger:从EventSystem接收事件，并为每个事件调用已注册的函数。
        //EventTrigger可用于指定希望为每个EventSystem事件调用的函数。
        //您可以为一个事件分配多个功能，并且每当EventTrigger接收到该事件时，
        //它将按提供顺序调用这些功能。
        EventTrigger.Entry down = new EventTrigger.Entry();
        //eventID 关联的回调正在侦听什么类型的事件
        down.eventID = EventTriggerType.PointerDown;
        //callback 要调用的所需TriggerEvent
        down.callback.AddListener(downAction);
        EventTrigger.Entry up = new EventTrigger.Entry();
        up.eventID = EventTriggerType.PointerUp;
        up.callback.AddListener(upAction);

        //按名称查找组件
        foreach(Transform t in this.GetComponentsInChildren<Transform>())
        {
            if(t.name.CompareTo("Text_Wave") == 0)
            {
                m_Text_Wave = t.GetComponent<Text>();
                SetWave(1);
            }
            else if(t.name.CompareTo("Text_Life") == 0)
            {
                m_Text_Life = t.GetComponent<Text>();
                m_Text_Life.text = string.Format("Life:<color=yellow>{0}</color>", m_life);
            }
            else if(t.name.CompareTo("Text_Point") == 0)
            {
                m_Text_Point = t.GetComponent<Text>();
                m_Text_Point.text = string.Format("Point:<color=yellow>{0}</color>", m_point);
            }
            else if(t.name.CompareTo("Button_Reset") == 0)
            {
                m_Button_Reset = t.GetComponent<Button>();
                m_Button_Reset.onClick.AddListener(delegate ()
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
                m_Button_Reset.gameObject.SetActive(false);
            }
            else if(t.name.Contains("Button_Player"))
            {
                //EventTrigger 触发器的UnityEvent类。

                //EventTrigger类中的TriggerEvent类继承字UnityEvent<BaseEventData>
                EventTrigger trigger = t.gameObject.AddComponent<EventTrigger>();
                //triggers 在此EventTrigger中注册的所有功能(List)
                trigger.triggers = new List<EventTrigger.Entry>();
                trigger.triggers.Add(down);
                trigger.triggers.Add(up);
            }
        }
    }

    //更新波数
    public void SetWave(int wave)
    {
        m_wave = wave;
        m_Text_Wave.text = string.Format("Wave:<color=yellow>{0}/{1}</color>", m_wave, m_waveMax);
    }

    //更新生命值
    public void SetDamage(int damage)
    {
        m_life -= damage;
        if(m_life <= 0)
        {
            m_life = 0;
            m_Button_Reset.gameObject.SetActive(true);
        }

        m_Text_Life.text = string.Format("Life:<color=yellow>{0}</color>", m_life);
    }

    //更新分数
    public bool SetPoint(int point)
    {
        if (m_point + point < 0)
            return false;
        m_point += point;
        m_Text_Point.text = string.Format("Point:<color=yellow>{0}</color>", m_point);
        return true;
    }

    //按钮摁下的回调方法
    void OnButtonCreateDefenderDown(BaseEventData data)
    {
        m_isSelectedButton = true;
    }

    //按钮抬起的回调方法
    void OnButtonCreateDefenderUp(BaseEventData data)
    {
        GameObject go = data.selectedObject;

    }
}
```
2.对于事件的回调机制还有不理解的地方，只知道会回调，但是回调的原理不清楚。

### 摄像机

**摄像机**

1.创建一个空游戏体作为摄像机的观察点，并为其创建脚本CameraPoint
```c#
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
```
2.图片要保存在Gizmos文件夹中
3.创建脚本GameCamera，指定给摄像机
```c#
	public static GameCamera instance = null;//单例

    protected float m_distance = 15;        //摄像机离地面的距离
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
```
4.修改GameManager脚本
```c#
	private void Update()
    {
        //如果选中创建士兵的按钮，则不移动摄像机
        if (m_isSelectedButton)
            return;
        //鼠标或触屏操作，不同平台代码不同
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        //是否触屏
        bool press = Input.touches.Length > 0 ? true : false;
        float mx = 0;
        float my = 0;
        if(press)
        {
            //手指的移动距离
            if(Imput.GetTouch(0).phase == TouchPhase.Moved)
            {
                mx = Input.GetTouch(0).deltaPosition.x * 0.01f;
                my = Input.GetTouch(0).deltaPosition.y * 0.01f;
            }
        }
#else
        bool press = Input.GetMouseButton(0);
        //获得鼠标移动距离
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
#endif
        //移动摄像机
        GameCamera.instance.Control(press, mx, my);
    }
```

### 路点

**路点**

1.敌人的前进路线由若干个路点组成，添加路点的Tag-pathnode，创建脚本PathNode
```c#
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
```
2.创建Editor文件夹，在其中添加脚本PathTool
```c#
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
```
3.在菜单栏中选择PathTool-CreatePathNode创建路点
4.复制若干个路点沿道路摆放，按下Ctrl+Q设置为父路点，再选择下一个路点按下Ctrl+W设置为子路点，再将其设置为父路点，再将下一个路点设置为子路点，循环操作。
5.修改GameManager脚本
```c#
	public bool m_debug = true;//显示路点的debug开关
    public List<PathNode> m_pathNodes;//路点
    
    
	//在start中调用，进行初始化
    [ContextMenu("BuildPath")]
    void BuildPath()
    {
        m_pathNodes = new List<PathNode>();
        //通过路点的Tag查找路点
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PathNode");
        for(int i = 0; i < objs.Length; i++)
        {
            m_pathNodes.Add(objs[i].GetComponent<PathNode>());
        }
    }
    
	//在场景中的两个路点之间画线
    private void OnDrawGizmos()
    {
        if (!m_debug || m_pathNodes == null)
            return;
        Gizmos.color = Color.blue;
        foreach(PathNode node in m_pathNodes)
        {
            if(node.m_next != null)
            {
                Gizmos.DrawLine(node.transform.position, node.m_next.transform.position);
            }
        }
    }
```
6.选择GameManager组件，将m_debug属性设置为真，单击齿轮按钮选择BuildPath打开自定义菜单

### 敌人

**敌人**

1.创建敌人脚本Enemy
```c#
	public PathNode m_currentNode;          //当前的目标路点
    public int m_life = 15;                 //当前生命值
    public int m_maxLife = 15;              //最大生命值
    public float m_speed = 2;               //速度

	//本质是委托
    public System.Action<Enemy> onDeath;    

    // Update is called once per frame
    void Update()
    {
        RotateTo();//转向
        MoveTo();//移动
    }

    //转向目标路点
    public void RotateTo()
    {
        var position = m_currentNode.transform.position - transform.position;
        position.y = 0;
        var targetRotation = Quaternion.LookRotation(position);
        float next = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, 120 * Time.deltaTime);
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
        Destroy(this.gameObject);
    }
```
2.导入资源模型，挂载敌人脚本
3.为模型创建动画控制器AnimatorController，将该控制器指定给模型的Animator组件
4.打开Animator窗口，将boarA@run动画拖入窗口，并设置为LoopTime
5.保存为prefab
6.创建另一个敌人脚本AirEnemy
```c#
public class AirEnemy : Enemy
{
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
```
7.按之前步骤找到对应的模型设置为prefab

### 敌人生成器

**敌人生成器**

1.创建脚本WaveData记录每波敌人的位置
```c#
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]//序列化属性，添加该属性才能序列化
public class WaveData
{
    public int wave = 0;//波数
    public List<GameObject> enemyPrefab;//敌人的prefab
    public int level = 1;//敌人的等级
    public float interval = 3;//创建敌人的间隔时间
}
```
2.创建脚本EnemySpawn，每隔一定时间生成敌人
```c#
	public PathNode m_startNode;    //起始路点
    private int m_liveEnemy = 0;    //存活的敌人数量
    public List<WaveData> waves;    //战斗波数配置数组
    int enemyIndex = 0;             //生成敌人数组的下标
    int waveIndex = 0;              //战斗波数数组的下标

    private void Start()
    {
        //开启协程，开始生成敌人
        StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        //WaitForEndOfFrame会在一帧结束后调用
        //准确来说，会在LateUpdate之后调用
        //区别于yield return null在Update之后调用
        yield return new WaitForEndOfFrame();
        //更新UI的波数
        GameManager.instance.SetWave(waveIndex + 1);
        //获取当前波的信息配置
        WaveData wave = waves[waveIndex];
        //根据配置的间隔时间生成敌人
        yield return new WaitForSeconds(wave.interval);

        //如果生成的敌人比配置数少
        while(enemyIndex < wave.enemyPrefab.Count)
        {
            //生成敌人
            Vector3 dir = m_startNode.transform.position - this.transform.position;
            GameObject enemyObj = Instantiate(wave.enemyPrefab[enemyIndex], transform.position, Quaternion.LookRotation(dir));
            //获取Enemy脚本，设置初始路点
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            enemy.m_currentNode = m_startNode;

            //设置敌人数值，一般来说通过数据库设置
            enemy.m_life = wave.level * 3;
            enemy.m_maxLife = enemy.m_life;

            //增加敌人数量
            m_liveEnemy++;
            //当敌人死掉时减少敌人数量
            //lambda表达式调用Action实现委托
            enemy.onDeath = new System.Action<Enemy>((Enemy e) => { m_liveEnemy--; });
            //更新敌人数组下标
            enemyIndex++;
            //根据时间间隔生成敌人
            yield return new WaitForSeconds(wave.interval);
        }

        //等待敌人全部被消灭
        //因为一波的敌人在上面的循环全部被创建出来了，要等到全部被消灭才开始下一波
        while(m_liveEnemy > 0)
        {
            yield return 0;
        }

        //重设敌人数组下标
        enemyIndex = 0;
        //更新战斗波数
        waveIndex++;
        //如果不是最后一波，继续开始协程
        if(waveIndex < waves.Count)
        {
            StartCoroutine(SpawnEnemy());
        }
        else
        {
            //胜利代码
        }
    }

    //在场景中显示为一个图标
    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "spawner.tif");
    }
```
3.创建一个空游戏体作为敌人生成器，并设置起始路点等
4.修改GameManager脚本和Enemy脚本，添加一个List将所有的敌人存放进去，方便对敌人进行遍历
```c#
//GameManager中代码
public List<Enemy> m_enemys = new List<Enemy>();//敌人列表

//Enemy中代码
	private void Start()
    {
        GameManager.instance.m_enemys.Add(this);
        //...
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
```

### 防守单位

**防守单位**

1.使用资源文件创建prefab，添加动画，将prefab放入Resources文件夹中（加载用）
2.创建脚本Defender
```c#
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
            //在攻击范围内的确定为目标敌人
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
            m_targetEnemy.SetDamage(m_power);
        yield return new WaitForSeconds(anim_length * 0.5f);
		//由于不明原因，攻击动画不会结束，所以添加该句代码来手动更改
        m_anim.CrossFade("idle", 0.1f);
        //等待攻击间隔
        yield return new WaitForSeconds(m_attackInterval);

        //继续下一次攻击，开启协程
        StartCoroutine(Attack());
    }
```
3.创建远程防守单位，为其prefab添加一个空物体作为攻击点，即发射弓箭的位置
4.创建脚本Archer，继承Defender，增加功能生成箭的模型
```c#
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
```
5.导入弓箭prefab，创建脚本Projectile
```c#
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
```
6.修改GameManager脚本，实现鼠标点击创建防守单位的功能
```c#
	void OnButtonCreateDefenderUp(BaseEventData data)
    {
        //创建射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit info;
        //如果碰到地面
        if(Physics.Raycast(ray, out info, 1000,m_groundLayer))
        {
            //是一个可以放置的格子
            if(TileObject.instance.GetDataFromPosition(info.point.x,info.point.z)==(int)Defender.TileStatus.GUARD)
            {
                //碰撞点位置
                Vector3 pos = new Vector3(info.point.x, 0, info.point.z);
                //grid位置
                Vector3 grid = TileObject.instance.transform.position;
                //格子大小
                float tileSize = TileObject.instance.tileSize;
                //计算出碰撞点所在格子的中心位置
                pos.x = grid.x + (int)((pos.x - grid.x) / tileSize) * tileSize + tileSize * 0.5f;
                pos.z = grid.z + (int)((pos.z - grid.z) / tileSize) * tileSize + tileSize * 0.5f;

                //获得选择的按钮的GameObject
                GameObject go = data.selectedObject;
                //名字包括“1”
                if(go.name.Contains("1"))
                {
                    //减15个铜钱
                    if(SetPoint(-15))
                    {
                        //调用Defender类创建对应的游戏体
                        Defender.Create<Defender>(pos, new Vector3(0, 180, 1));
                    }
                }
                //名字包括“2”
                else if(go.name.Contains("2"))
                {
                    //减20个铜钱
                    if(SetPoint(-20))
                    {
                        //调用Defender类创建对应的游戏体
                        Defender.Create<Archer>(pos, new Vector3(0, 180, 0));
                    }
                }
            }
        }

        //按钮选中取消
        m_isSelectedButton = false;
    }
```

### 生命条

**生命条**

1.创建一个新的Canvas， 将RanderMode设置为WorldSpace，使这个UI成为一个3D的UI
2.创建一个滑动条控件Slider，在Source Image中选择相应的图片，，将Handle Slider Area隐藏
3.在Slider的下级Fill的Source Image中设置前景图片，ImageType选择Filled，FillMethod选择Horizontal
4.将UI保存为prefab，并放置在Resources文件夹下
5.修改Enemy脚本
```c#
	Transform m_lifeBarObj;                 //生命条位置组件
    UnityEngine.UI.Slider m_lifebar;        //生命条滑动组件
    
    //start中的代码
    	//读取生命条prefab并放置到场景中
        GameObject prefab = Resources.Load<GameObject>("Canvas3D");
        m_lifeBarObj = Instantiate(prefab, Vector3.zero, Camera.main.transform.rotation, this.transform).transform;
        m_lifeBarObj.localPosition = new Vector3(0, 2.0f, 0);//放在敌人头上
        m_lifeBarObj.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        m_lifebar = m_lifeBarObj.GetComponentInChildren<UnityEngine.UI.Slider>();
        StartCoroutine(UpdateLifebar());//开启协程
       
    //更新生命条
    IEnumerator UpdateLifebar()
    {
        //更新生命条的值
        m_lifebar.value = (float)m_life / (float)m_maxLife;
        //面向摄像机
        m_lifeBarObj.transform.eulerAngles = Camera.main.transform.eulerAngles;
        yield return 0;//不等待
        StartCoroutine(UpdateLifebar());//循环
    }
```