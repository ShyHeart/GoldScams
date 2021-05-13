using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Protocol.Code;

public class RechargePanel : MonoBehaviour
{
    private GameObject goods;
    private Button[] goodsBtnArr;
    private Button btn_Close;
    private int rechargeCount;

    private void Awake()
    {
        EventCenter.AddListener<int>(EventDefine.UpdateCoinCount, UpdateCoinCount);

        EventCenter.AddListener(EventDefine.ShowRechargePanel, Show);
        Init();
    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener<int>(EventDefine.UpdateCoinCount, UpdateCoinCount);

        EventCenter.RemoveListener(EventDefine.ShowRechargePanel, Show);

    }

    private void Init()
    {
        goods = transform.Find("goods").gameObject;
        //new出button数组
        goodsBtnArr = new Button[goods.transform.childCount];
        //给button赋值 拿到所有button组件
        for (int i = 0; i < goods.transform.childCount; i++)
        {
            goodsBtnArr[i] = goods.transform.GetChild(i).GetComponentInChildren<Button>();

        }
        btn_Close = transform.Find("btn_Close").GetComponent<Button>();
        btn_Close.onClick.AddListener(() =>
        {
            transform.DOScale(Vector3.zero, 0.3f);

        });

        //充值按钮点击监听
        goodsBtnArr[0].onClick.AddListener(delegate { Recharge(10); });
        goodsBtnArr[1].onClick.AddListener(delegate { Recharge(20); });
        goodsBtnArr[2].onClick.AddListener(delegate { Recharge(30); });
        goodsBtnArr[3].onClick.AddListener(delegate { Recharge(400); });
        goodsBtnArr[4].onClick.AddListener(delegate { Recharge(500); });
        goodsBtnArr[5].onClick.AddListener(delegate { Recharge(600); });

    }

    private void Show()
    {
        //scale的缩放
        transform.DOScale(Vector3.one, 0.3f);
    }
    /// <summary>
    /// 充值的方法
    /// </summary>
    /// <param name="coinCount"></param>
    private void Recharge(int coinCount)
    {
        rechargeCount = coinCount;
        //向服务器发送充值的请求    coinCount 充值的数量
        NetMsgCenter.Instance.SendMsg(OpCode.Accout, AccountCode.UpdateCoinCount_CREQ, coinCount);
        
    }
    /// <summary>
    /// 金币数量的更新
    /// </summary>
    /// <param name="value"></param>
    private void UpdateCoinCount(int value)
    {
        EventCenter.Broadcast(EventDefine.Hint, "充值" + rechargeCount.ToString() + "金币成功");
    }
}
