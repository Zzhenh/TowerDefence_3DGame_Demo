using System.Collections;
using System.Collections.Generic;
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
