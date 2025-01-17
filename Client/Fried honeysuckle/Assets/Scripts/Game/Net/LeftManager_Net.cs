﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Protocol.Dto;
using Protocol.Dto.Fight;
using UnityEngine;
using UnityEngine.UI;

public class LeftManager_Net : MonoBehaviour
{
    public GameObject go_CardPre;

    private GameObject go_LookCardHint;
    protected Image img_HeadIcon;
    protected Image img_Banker;
    protected Transform CardPoints;
    protected GameObject go_CountDown;
    protected Text txt_CountDown;
    protected StakesCountHint m_StakesCountHint;
    protected Text txt_StakesSum;
    private Text txt_Hint;
    private Text txt_UserName;
    private GameObject go_Coin;
    private Text txt_CoinCount;
    private GameObject go_StakesSum;
    private int m_CardPointX = -40;
    private List<GameObject> go_SpawnCardList = new List<GameObject>();

    /// <summary>
    /// 是否逃跑
    /// </summary>
    public bool m_IsRun = false;
    /// <summary>
    /// 是否弃牌
    /// </summary>
    public bool m_IsGiveUpCard = false;
    /// <summary>
    /// 是否开始下注
    /// </summary>
    private bool m_IsStartStakes = false;

    private float m_Timer = 0f;
    private float m_Time = 60;
    private PlayerDto m_PlayerDto;


    #region Unity生命周期

    private void Awake()
    {
        EventCenter.AddListener<int>(EventDefine.GiveUpCardBRO, GiveUpCardBRO);
        EventCenter.AddListener<StakesDto>(EventDefine.PutStakesBRO,PutStakesBRO);
        EventCenter.AddListener<int>(EventDefine.LookCardBRO,LookCardBRO);
        EventCenter.AddListener<int>(EventDefine.StartStakes,StartStakes);
        EventCenter.AddListener<int>(EventDefine.LeaveFightRoom,LeaveFightRoom);
        EventCenter.AddListener<PlayerDto>(EventDefine.LestDealCard,DealCard);
        EventCenter.AddListener(EventDefine.LeftBanker,Banker);
        EventCenter.AddListener(EventDefine.StartGame, StartGame);
        EventCenter.AddListener(EventDefine.RefreshUI,ResreshUI);
        Init();
    }

    private void OnDestroy()
    {
        EventCenter.RemoveListener<int>(EventDefine.GiveUpCardBRO, GiveUpCardBRO);
        EventCenter.RemoveListener<StakesDto>(EventDefine.PutStakesBRO, PutStakesBRO);
        EventCenter.RemoveListener<int>(EventDefine.LookCardBRO,LookCardBRO);
        EventCenter.RemoveListener<int>(EventDefine.StartStakes, StartStakes);
        EventCenter.RemoveListener<int>(EventDefine.LeaveFightRoom, LeaveFightRoom);
        EventCenter.RemoveListener<PlayerDto>(EventDefine.LestDealCard, DealCard);
        EventCenter.RemoveListener(EventDefine.LeftBanker,Banker);
        EventCenter.RemoveListener(EventDefine.StartGame, StartGame);
        EventCenter.RemoveListener(EventDefine.RefreshUI,ResreshUI);

    }


    private void Init()
    {
        go_LookCardHint = transform.Find("LookCardHint").gameObject;
        go_StakesSum = transform.Find("StakesSum").gameObject;
        go_Coin = transform.Find("Coin").gameObject;
        txt_CoinCount = go_Coin.transform.Find("txt_CoinCount").GetComponent<Text>();
        txt_UserName = transform.Find("txt_UserName").GetComponent<Text>();
        m_StakesCountHint = transform.Find("StakesCountHint").GetComponent<StakesCountHint>();
        img_HeadIcon = transform.Find("img_HeadIcon").GetComponent<Image>();
        img_Banker = transform.Find("img_Banker").GetComponent<Image>();
        txt_StakesSum = transform.Find("StakesSum/txt_StakesSum").GetComponent<Text>();
        go_CountDown = transform.Find("CountDown").gameObject;
        txt_CountDown = transform.Find("CountDown/txt_CountDown").GetComponent<Text>();
        CardPoints = transform.Find("CardPoints");
        txt_Hint = transform.Find("txt_Hint").GetComponent<Text>();

        txt_StakesSum.text = "0";
        HideObj();
    }

    private void HideObj()
    {
        img_Banker.gameObject.SetActive(false);
        go_CountDown.SetActive(false);
        txt_Hint.gameObject.SetActive(false);
        txt_UserName.gameObject.SetActive(false);
        img_HeadIcon.gameObject.SetActive(false);
        go_StakesSum.SetActive(false);
        go_Coin.SetActive(false);
        go_LookCardHint.SetActive(false);

    }

    // 如果启用 MonoBehaviour，则每个固定帧速率的帧都将调用此函数
    private void FixedUpdate()
    {
        if (m_IsStartStakes)
        {
            if (m_Time<=0)
            {
                m_IsStartStakes = false;
                m_Time = 60;
                m_Timer = 0;
                return;
            }
            m_Timer += Time.deltaTime;
            if (m_Timer>=1)
            {
                m_Timer = 0;
                m_Time--;
                txt_CountDown.text = m_Time.ToString();
            }
        }
    }

    #endregion

    /// <summary>
    /// 有玩家下注的服务器广播
    /// </summary>
    /// <param name="dto"></param>
    private void PutStakesBRO(StakesDto dto)
    {
        if (dto.userId == Models.GameModel.MatchRoomDto.LeftPlayerId)
        {
            txt_CoinCount.text = dto.remainCoin.ToString();
            if (dto.stakesType == StakesDto.StakesType.NoLook)
            {
                m_StakesCountHint.Show(dto.stakesCount + "不看");
                txt_StakesSum.text = dto.stakesSum.ToString();
            }
            else
            {
                m_StakesCountHint.Show(dto.stakesCount + "看看");
                txt_StakesSum.text = dto.stakesSum.ToString();
            }
        }
        go_CountDown.SetActive(false);
        m_IsStartStakes = false;
    }

    /// <summary>
    /// 有玩家看牌的服务器广播
    /// </summary>
    /// <param name="lookCardUserId"></param>
    private void LookCardBRO(int lookCardUserId)
    {
        if (lookCardUserId == Models.GameModel.MatchRoomDto.LeftPlayerId)
        {
            go_LookCardHint.SetActive(true);
        }
    }

    /// <summary>
    /// 开始下注
    /// </summary>
    private void StartStakes(int userId)
    {
        if (userId==Models.GameModel.MatchRoomDto.LeftPlayerId)
        {
            m_Time = 60;
            go_CountDown.SetActive(true);
            txt_CoinCount.text = "60";
            m_IsStartStakes = true;
        }
        else
        {
            go_CountDown.SetActive(false);
            m_IsStartStakes = false;
        }
    }

    /// <summary>
    /// 有玩家离开了服务器发来的响应
    /// </summary>
    /// <param name="leaveUserId"></param>
    private void LeaveFightRoom(int leaveUserId)
    {
        if (leaveUserId==Models.GameModel.MatchRoomDto.LeftPlayerId)
        {
            HideObj();
            txt_Hint.text = "逃跑了";
            txt_Hint.gameObject.SetActive(true);
            m_IsRun = true;

            foreach (var item in go_SpawnCardList)
            {
                Destroy(item);
            }
        }
    }
    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartGame()
    {
        txt_StakesSum.text = m_PlayerDto.stakesSum.ToString();
        txt_Hint.gameObject.SetActive(false);
    }

    /// <summary>
    /// 有玩家弃牌的服务器广播
    /// </summary>
    /// <param name="giveIpUserId"></param>
    private void GiveUpCardBRO(int giveIpUserId)
    {
        if (giveIpUserId == Models.GameModel.MatchRoomDto.LeftPlayerId)
        {
            go_CountDown.SetActive(false);
            m_IsStartStakes = false;
            txt_Hint.text = "已弃牌";
            txt_Hint.gameObject.SetActive(true);
            m_IsGiveUpCard = true;

            foreach (var card in go_SpawnCardList)
            {
                Destroy(card);
            }
        }
    }


    /// <summary>
    /// 成为庄家
    /// </summary>
    private void Banker()
    {
        img_Banker.gameObject.SetActive(true);
    }
    /// <summary>
    /// 发牌
    /// </summary>
    private void DealCard( PlayerDto player)
    {
        m_PlayerDto = player;
        go_SpawnCardList.Clear();
        foreach (var card in player.cardList)
        {
            DealCard(0.3f, new Vector3(566.6001f,-36.89001f,0));
        }
    }

    /// <summary>
    /// 发牌
    /// </summary>
    private void DealCard(float duration, Vector3 initPos)
    {
        GameObject go = Instantiate(go_CardPre, CardPoints);
        go.GetComponent<RectTransform>().localPosition = initPos;
        go.GetComponent<RectTransform>().DOLocalMove(new Vector3(m_CardPointX, 0, 0), duration);

        m_CardPointX += 40;

        
        go_SpawnCardList.Add(go);
    }

    /// <summary>
    /// 当有新玩家进来时或自身玩家进来或离开时调用这个方法，刷新一下界面UI显示
    /// </summary>
    private void ResreshUI()
    {
        MatchRoomDto room = Models.GameModel.MatchRoomDto;

        if (room.LeftPlayerId!=-1)
        {
            UserDto dto = room.userIdUserDtoDic[room.LeftPlayerId];
            img_HeadIcon.gameObject.SetActive(true);
            img_HeadIcon.sprite = ResourcesManager.GetSprite(dto.IconName);
            go_Coin.SetActive(true);
            txt_CoinCount.text = dto.CoinCount.ToString();
            go_StakesSum.SetActive(true);
            txt_UserName.gameObject.SetActive(true);
            txt_UserName.text = dto.UserName;

            //左边玩家在准备中
            if (room.readyUserIdList.Contains(room.LeftPlayerId))
            {
                txt_Hint.text = "已准备";
                txt_Hint.gameObject.SetActive(true);
            }
            else
            {
                txt_Hint.gameObject.SetActive(false);
            }
        }
        else
        {
            txt_Hint.gameObject.SetActive(false);
            img_HeadIcon.gameObject.SetActive(false);
            go_Coin.SetActive(false);
            go_StakesSum.SetActive(false);
            txt_UserName.gameObject.SetActive(false);
        }

    }
}
