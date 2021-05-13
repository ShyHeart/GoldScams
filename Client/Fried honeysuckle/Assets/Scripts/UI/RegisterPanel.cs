using Protocol.Code;
using Protocol.Dto;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegisterPanel : MonoBehaviour
{
    //用户名 密码 显示按钮 注册按钮 返回按钮
    private InputField Input_UserName;
    private InputField Input_PassWord;
    private Button btn_Pwd;
    private Button btn_Register;
    private Button btn_Back;

    private bool isShowPassWord = false;  //是否显示密码。默认为false

    private void Awake()
    {
        EventCenter.AddListener(EventDefine.ShowRegisterPanel, Show);
        Init();
        gameObject.SetActive(false);
    }
    private void Init()
    {
        Input_UserName = transform.Find("UserName/Input_UserName").GetComponent<InputField>();
        Input_PassWord = transform.Find("PassWord/Input_PassWord").GetComponent<InputField>();

        btn_Pwd = transform.Find("btn_Pwd").GetComponent<Button>();
        btn_Pwd.onClick.AddListener(OnPwdButtonClick);

        btn_Register = transform.Find("btn_Register").GetComponent<Button>();
        btn_Register.onClick.AddListener(OnRegisterButtonClick); 

        btn_Back = transform.Find("btn_Back").GetComponent<Button>();
        btn_Back.onClick.AddListener(OnBackButtonClick);

    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener(EventDefine.ShowRegisterPanel, Show);

    }
    /// <summary>
    /// 密码显示或隐藏按钮点击
    /// </summary>
    private void OnPwdButtonClick()
    {
        isShowPassWord = !isShowPassWord;
        if (isShowPassWord)
        {
            Input_PassWord.contentType = InputField.ContentType.Standard;
            btn_Pwd.GetComponentInChildren<Text>().text = "隐藏";
        }
        else
        {
            Input_PassWord.contentType = InputField.ContentType.Password;
            btn_Pwd.GetComponentInChildren<Text>().text = "显示";

        }
        //因为只有点击密码框才会改变状态，所以默认选中密码框  SetSelectedGameObject 设置选中的游戏物体
        EventSystem.current.SetSelectedGameObject(Input_PassWord.gameObject);
    }
    /// <summary>
    /// 返回按钮点击
    /// </summary>
    private void OnBackButtonClick()
    {
        gameObject.SetActive(false);
        EventCenter.Broadcast(EventDefine.ShowLoginPanel);
    }
    /// <summary>
    /// 注册按钮点击
    /// </summary>
    private void OnRegisterButtonClick()
    {
        if (Input_UserName.text == null || Input_UserName.text == "")
        {
            EventCenter.Broadcast(EventDefine.Hint, "请输入用户名");
            //Debug.Log("请输入用户名");
            return;
        }
        if (Input_PassWord.text == null || Input_PassWord.text == "")
        {
            EventCenter.Broadcast(EventDefine.Hint, "请输入密码");
            //Debug.Log("请输入密码");
            return;
        }
        //向服务器发送数据，注册一个用户
        AccountDto dto = new AccountDto(Input_UserName.text, Input_PassWord.text);
        NetMsgCenter.Instance.SendMsg(OpCode.Accout, AccountCode.Register_CREQ, dto);
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
}
