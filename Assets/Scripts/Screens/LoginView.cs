using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine.EventSystems;
using System;
using Globals;

public class LoginView : BaseView
{
    [SerializeField] TMP_InputField m_AccountTMPIF, m_PasswordTMPIF, m_IpTMPIF;
    [SerializeField] GameObject m_CheckTest, m_ButtonLogin, m_ButtonCreateAccount, m_ButtonPlayGuest;
    public string accPlayNow = "";
    public string passPlayNow = "";
    bool isOpenFirst = true;
    protected override void Start()
    {
        base.Start();
        // Config.TELEGRAM_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3NDEyNDYzNTEsInVpZCI6ODQwNzU2OH0.sbdZYQJL9PWlR7koDumjV6F2C4iqKrES60FGKVuAOTg";
        if (!Config.TELEGRAM_TOKEN.Equals(""))
        {
            m_AccountTMPIF.gameObject.SetActive(false);
            m_PasswordTMPIF.gameObject.SetActive(false);
            m_ButtonLogin.SetActive(false);
            m_ButtonCreateAccount.SetActive(false);
            m_ButtonPlayGuest.SetActive(false);
            OnTelegramLogin();
        }
        else
        {
            if (!Config.username_normal.Equals("")) m_AccountTMPIF.text = Config.username_normal;
            var isFirstOpen = PlayerPrefs.GetInt("isFirstOpen", 0);
            Logging.Log("isFirstOpen " + isFirstOpen);
            if (isFirstOpen == 0)
            {
                PlayerPrefs.SetInt("isFirstOpen", 1);
                // onClickPlayNow();
                PlayerPrefs.Save();
            }
            else
            {
                Logging.Log("isOpenFirst " + isOpenFirst);
                if (isOpenFirst)
                {
                    isOpenFirst = false;
                    // reconnect();
                }
            }
        }
    }

    public void reconnect()
    {
        UIManager.instance.showWaiting();
        switch (Config.typeLogin)
        {
            case LOGIN_TYPE.NORMAL:
                {
                    SocketSend.sendLogin(Config.user_name, Config.user_pass, false);
                    break;
                }
            case LOGIN_TYPE.FACEBOOK:
                {
                    //SocketSend.sendLogin("", aToken.TokenString, false);
                    break;
                }
            case LOGIN_TYPE.PLAYNOW:
                {
                    SocketSend.onPlayNow();
                    break;
                }
            default:
                {
                    Logging.Log("dclm Xem lai di nhe !!!");
                    break;
                }
        }

    }
    protected override void OnEnable()
    {
        Config.arrBannerLobby.Clear();
        Config.arrOnlistTrue.Clear();
        if (UIManager.instance == null)
        {
            LobbyView lobbyView = transform.parent.Find("LobbyView").GetComponent<LobbyView>();
            lobbyView.resetLogout();
        }
        else
        {
            UIManager.instance.lobbyView.resetLogout();
        }

        Config.invitePlayGame = true;
        Config.isLoginSuccess = false;
        SoundManager.instance.pauseMusic();
        Config.getDataUser();

        if (Config.typeLogin == LOGIN_TYPE.NORMAL)
        {
            //ipfAcc.text = Config.username_normal;
            //ipfPass.text = Config.password_normal;
            if (!Config.username_normal.Equals("")) m_AccountTMPIF.text = Config.username_normal;
            //if (Config.password_normal != "")
            //ipfPass.text = Config.password_normal;
        }
        //Config.typeLogin = LOGIN_TYPE.NONE;
        //PlayerPrefs.SetInt("type_login",(int)LOGIN_TYPE.NONE);
        //PlayerPrefs.Save();
        if (UIManager.instance != null)
            UIManager.instance.destroyAllPopup();
        CURRENT_VIEW.setCurView(CURRENT_VIEW.LOGIN_VIEW);
    }






    public void onClickLogin()
    {
        SoundManager.instance.soundClick();
        var strAcc = m_AccountTMPIF.text;
        var strPass = m_PasswordTMPIF.text;


        if (strAcc.Equals("") || strPass.Equals(""))
        {
            return;
        }
        Config.user_name = strAcc;
        Config.user_pass = strPass;

        UIManager.instance.showWaiting();
        Config.typeLogin = LOGIN_TYPE.NORMAL;
        SocketSend.sendLogin(strAcc, strPass, false);
    }
    public void OnTelegramLogin()
    {
        UIManager.instance.showWaiting();
        Config.typeLogin = LOGIN_TYPE.TELEGRAM;
        StartCoroutine(SocketSend.SendTelegramLogin());
    }



}
