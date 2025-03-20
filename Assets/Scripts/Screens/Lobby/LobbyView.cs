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
    GameObject objDot, icNotiMail, icNotiFree, icNotiMessage, btnBannerNews, m_ConfigOn, m_ConfigOff;
    [SerializeField] TextMeshProUGUI lb_name, lb_id, lb_ag, lb_safe, lbTimeOnline, lbQuickGame;
    [SerializeField] Transform m_MiniGameIconTf, m_OnlySloticonTf;
    [SerializeField] Button btnNext, btnPrevious;
    [SerializeField] SkeletonGraphic animQuickPlay;
    [SerializeField] ScrollRect scrListGame;
    [SerializeField] Avatar avatar;
    [SerializeField] PageSlider bannerLobbyContainer;

    private List<string> listShowPopupNoti = new();
    private Coroutine _GetInfoPusoyJackPotC;
    private int TabGame = 0;
    private bool blockSpamTabGame, isHideBtnScroll, isRunStart;

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
                    OnClickTab(btn);
                });
            }
        }
    }

    void OnClickTab(Button btn)
    {
        SoundManager.instance.soundClick();
        if (!blockSpamTabGame)
        {
            var indexTab = 0;
            for (var i = 0; i < listTabs.Count; i++)
            {
                var gOn = listTabs[i].transform.GetChild(1);
                if (btn == listTabs[i])
                {
                    indexTab = i;
                    gOn.gameObject.SetActive(true);
                }
                else
                {
                    gOn.gameObject.SetActive(false);
                }
            }
            TabGame = indexTab;
            changeTabGame();
            blockSpamTabGame = true;
            DOTween.Kill("blockSpamTabGame");
            DOTween.Sequence().AppendInterval(1.0f).AppendCallback(() =>
            {
                blockSpamTabGame = false;
            }).SetId("blockSpamTabGame");
        }
    }
    private void changeTabGame()
    {
        ContentSizeFitter gameTabsCSF = scrListGame.content.GetComponent<ContentSizeFitter>();
        gameTabsCSF.enabled = true;
        if (TabGame == 0)
        {
            for (int i = 0; i < scrListGame.content.childCount; i++)
                scrListGame.content.GetChild(i).gameObject.SetActive(i != m_OnlySloticonTf.GetSiblingIndex());
        }
        else if (TabGame == 1)
        {
            for (int i = 0; i < scrListGame.content.childCount; i++)
                scrListGame.content.GetChild(i).gameObject.SetActive(i == m_OnlySloticonTf.GetSiblingIndex());
        }
        if (!gameObject.activeSelf) return;
        StartCoroutine(delay1FrameAndCheck()); //có trường hợp màn hình dài content nhỏ hơn viewport sẽ bị dồn lệch về 1 bên
        IEnumerator delay1FrameAndCheck()
        {
            yield return null;
            yield return null;
            if (scrListGame.content.rect.width < scrListGame.viewport.rect.width)
            {
                gameTabsCSF.enabled = false;
                scrListGame.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrListGame.viewport.rect.width);
            }
            scrListGame.content.anchoredPosition = Vector2.zero;
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
            OnClickTab(listTabs[TabGame]);

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
        avatar.loadAvatar(User.userMain.Avatar, User.userMain.Username, User.FacebookID);
        avatar.setVip(User.userMain.VIP);
    }
    private void ClearButtonGameOnDisable()
    {
        foreach (Transform childTf in scrListGame.content) if (childTf != m_MiniGameIconTf && childTf != m_OnlySloticonTf) Destroy(childTf.gameObject);
        foreach (Transform childTf in m_MiniGameIconTf) Destroy(childTf.gameObject);
        foreach (Transform childTf in m_OnlySloticonTf) Destroy(childTf.gameObject);
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
