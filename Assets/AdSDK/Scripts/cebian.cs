using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;

public class cebian : MonoBehaviour
{
    //前往按钮
    public Button btn_Go;


    private void Awake()
    {
        // 面板默认隐藏，所以按钮监听必须在这里注册（隐藏状态下 Start 不会执行）
        if (btn_Go != null)
        {
            btn_Go.onClick.RemoveListener(NavigateToSideBar);
            btn_Go.onClick.AddListener(NavigateToSideBar);
        }
        // 注意：这里不再 SetActive(false)。
        // 面板初始隐藏统一由 AdPanelService 在场景加载后处理，
        // 否则面板一旦在场景里是未激活状态，Awake 会在"第一次打开它的那一帧"才执行，
        // 然后立刻把自己关掉，表现就是点了按钮没反应。
    }

    private void OnEnable()
    {
        // 打开时置顶（提成顶层画布的工作由 AdPanelService 在打开时统一做）
        transform.SetAsLastSibling();
    }

    /// <summary>关闭侧边栏引导面板（右上角关闭按钮）。</summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 跳转到侧边栏
    /// </summary>
    public void NavigateToSideBar()
    {
        try
        {
            var data = new JsonData
            {
                ["scene"] = "sidebar",
            };
            TT.NavigateToScene(data, () =>
            {
                Debug.Log("navigate to scene success");
            }, () =>
            {
                Debug.Log("navigate to scene complete");
            }, (errCode, errMsg) =>
            {
                Debug.Log($"navigate to scene error, errCode:{errCode}, errMsg:{errMsg}");
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError("跳转到侧边栏异常: " + e.Message);
        }
    }
}
