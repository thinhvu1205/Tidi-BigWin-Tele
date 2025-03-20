using UnityEngine;
using TMPro;
using Globals;

public class LoginView : BaseView
{
    [SerializeField] TMP_InputField m_AccountTMPIF, m_PasswordTMPIF, m_IpTMPIF;
    [SerializeField] GameObject m_ButtonLogin, m_ButtonCreateAccount, m_ButtonPlayGuest;
    public string accPlayNow = "";
    public string passPlayNow = "";
    bool isOpenFirst = true;
    protected override void Start()
    {
        base.Start();
        // Config.TELEGRAM_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3NDE3NDQ2NDUsInVpZCI6ODM0ODk4OX0.LbeXtbWcYS36mujRCkPKY9znj2UKwAOW0JvU7RCzaPI";
        // Config.curGameId = 8010;
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
                PlayerPrefs.Save();
            }
            else
            {
                Logging.Log("isOpenFirst " + isOpenFirst);
                if (isOpenFirst)
                {
                    isOpenFirst = false;
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
        Config.invitePlayGame = true;
        Config.isLoginSuccess = false;
        SoundManager.instance.pauseMusic();
        Config.getDataUser();

        if (Config.typeLogin == LOGIN_TYPE.NORMAL)
        {
            if (!Config.username_normal.Equals("")) m_AccountTMPIF.text = Config.username_normal;
        }
        if (UIManager.instance != null) UIManager.instance.destroyAllPopup();
        CURRENT_VIEW.setCurView(CURRENT_VIEW.LOGIN_VIEW);
    }

    public void onClickPlayNow()
    {
        Config.typeLogin = LOGIN_TYPE.PLAYNOW;
        SoundManager.instance.soundClick();

        UIManager.instance.showWaiting();

        SocketSend.onPlayNow();
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
