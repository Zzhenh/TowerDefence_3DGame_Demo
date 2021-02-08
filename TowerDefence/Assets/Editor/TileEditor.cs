using System.Collections;
using System.Collections.Generic;
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
