using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Protocol.Dto.Fight;
using UnityEngine.SceneManagement;

public class GameOverPanel_Net : MonoBehaviour
{
    [System.Serializable]
    public class Player
    {
        public Text txt_UserName;
        public Text txt_CoinCount;

    }

    public Player lose_1;
    public Player win;
    public Player lose_2;
    public Button btn_Again;
    public Button btn_MainMenu;
    public AudioClip Clip_Win;
    public AudioClip Clip_Lose;
    private AudioSource m_AudioSource;

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
        EventCenter.AddListener<GameOverDto>(EventDefine.GameOverBRO,GameOverBRO);
        btn_Again.onClick.AddListener(OnAgainButtonClick);
        btn_MainMenu.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("2.Main");
        });
    }

    private void OnDestroy()
    {
        EventCenter.RemoveListener<GameOverDto>(EventDefine.GameOverBRO, GameOverBRO);

    }

    private void OnAgainButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GameOverBRO(GameOverDto dto)
    {
        transform.DOScale(Vector3.one, 0.3f);
        win.txt_UserName.text = dto.winDto.userName;
        win.txt_CoinCount.text = dto.winCount.ToString();

        lose_1.txt_UserName.text = dto.loseDtoList[0].userName;
        lose_1.txt_CoinCount.text = (-dto.loseDtoList[0].stakesSum).ToString();

        lose_2.txt_UserName.text = dto.loseDtoList[1].userName;
        lose_2.txt_CoinCount.text = (-dto.loseDtoList[1].stakesSum).ToString();


        //判断是否胜利播放音效    
        if (dto.winDto.userId==Models.GameModel.userDto.UserId)
        {
            m_AudioSource.clip = Clip_Win;
            m_AudioSource.Play();
        }
        else
        {
            m_AudioSource.clip = Clip_Lose;
            m_AudioSource.Play();

        }

    }


}
