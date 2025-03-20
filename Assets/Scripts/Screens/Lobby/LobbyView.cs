using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;
using DG.Tweening;
using System;
using Globals;
using TS.PageSlider;
using System.Collections;


public class LobbyView : BaseView
{
    [SerializeField] List<Button> listTabs = new();
    [SerializeField]
    GameObject objDot, btnBannerNews, m_ConfigOn, m_ConfigOff;
    [SerializeField] TextMeshProUGUI lb_name, lb_id, lb_ag, lb_safe, lbTimeOnline;
    [SerializeField] Transform m_MiniGameIconTf, m_OnlySloticonTf;
    [SerializeField] ScrollRect scrListGame;
    [SerializeField] Avatar avatar;
    [SerializeField] PageSlider bannerLobbyContainer;

    private List<string> listShowPopupNoti = new();
    private Coroutine _GetInfoPusoyJackPotC;
    private int TabGame = 0;
    private bool isRunStart;

    public void DoClickDownloadGame()
    {
        Application.OpenURL(Config.ApkFullUrl);
    }
    protected override void Start()
    {
        isRunStart = true;
        base.Start();
        if (Config.is_dt)
        {
            for (var i = 0; i < listTabs.Count; i++)
            {
                var btn = listTabs[i];
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                });
            }
        }
    }

    protected override void OnEnable()
    {
        CURRENT_VIEW.setCurView(CURRENT_VIEW.GAMELIST_VIEW);
        SoundManager.instance.playMusic();
        m_ConfigOn.SetActive(Config.is_dt);
        m_ConfigOff.SetActive(!Config.is_dt);
        if (Config.is_dt)
        {

            if (bannerLobbyContainer.pageCount > 0)
            {
                bannerLobbyContainer.gameObject.SetActive(true);
                setPosWhenBannerActive();
            }
        }
        else
        {
            // foreach (ItemGame ig in m_ConfigOffIGs)
            //     ig.setInfo(int.Parse(ig.name), null, null, materialDefault, () => onClickGame(ig));
        }
        if (Config.isChangeTable)
        {
            Config.isChangeTable = false;
            if (Config.listGamePlaynow.Contains(Config.curGameId)) SocketSend.sendPlayNow(Config.curGameId);
            else SocketSend.sendChangeTable(Config.tableMark, Config.tableId);
        }
        if (Config.ket) updateAgSafe();
        if (isRunStart) onClickLobby();
    }
    private void OnDisable()
    {
        if (_GetInfoPusoyJackPotC != null) StopCoroutine(_GetInfoPusoyJackPotC);
        removeAllPopupNoti();
        ClearButtonGameOnDisable();
    }

    public void updateInfo()
    {
        //updateBannerNews();
        updateName();
        updateAg();
        updateAgSafe();
        updateAvatar();
        updateIdUser();
        //checkAlertMail();

        updateCanInviteFriend();

    }

    public void updateCanInviteFriend()
    {
        objDot.SetActive(User.userMain.canInputInvite);
    }
    private void setPosWhenBannerActive()
    {
        bool isActive = bannerLobbyContainer.gameObject.activeSelf;
        // modelLobby.transform.localPosition = new Vector2(isActive ? -65f : -250f, modelLobby.transform.localPosition.y);
        RectTransform rt = scrListGame.GetComponent<RectTransform>();
        if (isActive)
        {
            rt.offsetMin = new Vector2(370, rt.offsetMin.y);
            rt.offsetMax = new Vector2(-70, rt.offsetMax.y);
        }
    }
    float timeRun = 0;
    protected override void Update()
    {
        if (bannerLobbyContainer.pageCount > 1)
        {
            timeRun += Time.deltaTime;
            if (timeRun >= 5)
            {
                timeRun = 0;

                var page = bannerLobbyContainer.currentPage;
                page++;
                if (page >= bannerLobbyContainer.pageCount)
                {
                    page = 0;
                }
                bannerLobbyContainer.changeToPage(page);
            }
        }
    }

    public void removeAllPopupNoti()
    {
        listShowPopupNoti.Clear();
    }
    public void checkShowPopupNoti()
    {
        if (listShowPopupNoti.Count > 0)
        {
            string typePopup = listShowPopupNoti[0];
            listShowPopupNoti.RemoveAt(0);
            switch (typePopup)
            {
                case "MAIL_ADMIN":
                    {

                        UIManager.instance.showDialog(Config.getTextConfig("has_mail_show_system"), Config.getTextConfig("txt_ok"), () =>
                        {
                        }, Config.getTextConfig("label_cancel"), () =>
                        {
                            checkShowPopupNoti();
                        });
                        break;
                    }
                case "FREE_CHIP":
                    {
                        if (UIManager.instance.loginView.gameObject.activeSelf || UIManager.instance.gameView != null) return;
                        UIManager.instance.showDialog(Config.getTextConfig("has_mail_show_gold"), Config.getTextConfig("txt_free_chip"), () =>
                        {
                        }, Config.getTextConfig("label_cancel"), () =>
                        {
                            checkShowPopupNoti();
                        });
                        break;
                    }
                case "CHAT_PRIVATE":
                    {
                        break;
                    }
            }
        }
    }
    public void updateName()
    {
        lb_name.text = User.userMain.displayName;
        Config.effectTextRunInMask(lb_name, true);
    }

    public void updateAg()
    {
        lb_ag.text = Config.FormatNumber(User.userMain.AG);
    }

    public void updateAgSafe()
    {
        lb_safe.text = Config.FormatNumber(User.userMain.agSafe);
    }
    public void updateIdUser()
    {
        lb_id.text = "ID:" + User.userMain.Userid;
    }
    public void updateAvatar()
    {
        //string fbId = "";
        //if (Config.typeLogin == LOGIN_TYPE.FACEBOOK)
        //{
        //    fbId = User.FacebookID;
        //}
        avatar.loadAvatar(User.userMain.Avatar, User.userMain.Username, User.FacebookID);
        avatar.setVip(User.userMain.VIP);
    }
    private void ClearButtonGameOnDisable()
    {
        foreach (Transform childTf in scrListGame.content) if (childTf != m_MiniGameIconTf && childTf != m_OnlySloticonTf) Destroy(childTf.gameObject);
        foreach (Transform childTf in m_MiniGameIconTf) Destroy(childTf.gameObject);
        foreach (Transform childTf in m_OnlySloticonTf) Destroy(childTf.gameObject);
    }
    private IEnumerator _GetJackpotPusoy()
    {
        while (User.userMain == null)
        {
            yield return new WaitForSeconds(.2f);
        }
        while (true)
        {
            SocketSend.sendUpdateJackpot((int)GAMEID.PUSOY);
            yield return new WaitForSeconds(5);
        }
    }


    public bool isClicked = false;

    public void onClickLobby()
    {
        checkShowPopupNoti();
        CURRENT_VIEW.setCurView(CURRENT_VIEW.GAMELIST_VIEW);
    }


    public void setTimeGetMoney()
    {
        if (Promotion.time <= 0)
        {
            lbTimeOnline.text = Config.getTextConfig("click_to_spin");
            SocketSend.sendPromotion();
        }
        else
        {
            lbTimeOnline.text = Config.convertTimeToString(Promotion.time);
            Promotion.time--;
            DOTween.Sequence().AppendInterval(1).AppendCallback(() =>
            {
                setTimeGetMoney();
            });
        }
    }


    public void resetLogout()
    {
        // modelLobby.SetActive(true);
        // modelLobby.GetComponent<SkeletonGraphic>().Initialize(true);
        // modelLobby.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", true);
        //scrollSnapView.gameObject.SetActive(false);
        bannerLobbyContainer.gameObject.SetActive(false);
        setPosWhenBannerActive();
        btnBannerNews.SetActive(false);
        TabGame = 0;
    }
}
