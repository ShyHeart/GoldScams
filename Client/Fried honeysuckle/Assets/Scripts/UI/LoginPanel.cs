using Protocol.Code;
using Protocol.Dto;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    private InputField Input_UserName;
    private InputField Input_PassWord;
    private Button btn_Login;
    private Button btn_Register;

    private void Awake()
    {
        EventCenter.AddListener(EventDefine.ShowLoginPanel, Show);
        Init();
    }
    private void Init()
    {
        Input_UserName = transform.Find("Input_UserName").GetComponent<InputField>();
        Input_PassWord = transform.Find("Input_PassWord").GetComponent<InputField>();

        btn_Login = transform.Find("btn_Login").GetComponent<Button>();
        btn_Login.onClick.AddListener(OnLoginButtonClick);

        btn_Register = transform.Find("btn_Register").GetComponent<Button>();
        btn_Register.onClick.AddListener(OnRegisterButtonClick);

    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener(EventDefine.ShowLoginPanel, Show);

    }
    /// <summary>
    /// 注册按钮点击
    /// </summary>
    private void OnRegisterButtonClick()
    {
        EventCenter.Broadcast(EventDefine.ShowRegisterPanel);
    }
    /// <summary>
    /// 登录按钮点击
    /// </summary>
    private void OnLoginButtonClick()
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
        //向服务器发送登录请求
        AccountDto dto = new AccountDto(Input_UserName.text, Input_PassWord.text);
        NetMsgCenter.Instance.SendMsg(OpCode.Accout, AccountCode.Login_CREQ, dto);
        //Debug.Log("登录成功");
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }

}
