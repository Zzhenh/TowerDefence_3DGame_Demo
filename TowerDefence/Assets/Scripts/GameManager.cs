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

    public bool m_debug = true;//显示路点的debug开关
    public List<PathNode> m_pathNodes;//路点
    public List<Enemy> m_enemys = new List<Enemy>();//敌人列表
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

        BuildPath();
    }

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
        Debug.Log("UpUpUpUp");
        //创建射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit info;
        //如果碰到地面
        if(Physics.Raycast(ray, out info, 1000,m_groundLayer))
        {
            Debug.Log("Raycast");
            //是一个可以放置的格子
            if(TileObject.instance.GetDataFromPosition(info.point.x,info.point.z)==(int)Defender.TileStatus.GUARD)
            {
                Debug.Log("Guard");
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

    //在场景中的两个路点之间画线
    private void OnDrawGizmos()
    {
        if (!m_debug || m_pathNodes == null)
            return;
        Gizmos.color = Color.red;
        foreach(PathNode node in m_pathNodes)
        {
            if(node.m_next != null)
            {
                Gizmos.DrawLine(node.transform.position, node.m_next.transform.position);
            }
        }
    }
}
