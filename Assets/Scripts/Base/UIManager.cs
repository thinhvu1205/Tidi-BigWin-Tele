using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.U2D;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine.Networking;
using static Globals.Config;
using System.Linq;
using System.Collections;
using UnityEngine.Video;
using Globals;



public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;
    [SerializeField] Sprite sf_toast = null;
    [SerializeField] GameObject nodeLoad;

    [SerializeField] public Transform parentPopups, parentGame, parentBanner;
    public TMP_FontAsset fontDefault = null;

    public LoginView loginView;
    public LobbyView lobbyView;

    float timeShowLoad = 0;

    public SpriteAtlas avatarAtlas, cardAtlas;
    public CustomKeyboard m_KeyboardCK;
    [SerializeField] Sprite avtDefault;
    [SerializeField] Canvas canvasGame;
    [HideInInspector] public GameView gameView;
    [SerializeField] AlertMessage alertMessage;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] GameObject videoBg;

    public List<DialogView> dialogPool = new List<DialogView>();
    public List<DialogView> listDialogOne = new List<DialogView>();
    [SerializeField] public VideoClip videoStartSiXiang;
    public long PusoyJackPot;
    public bool SendChatEmoToHiddenPlayers = false;

    void Awake()
    {
        // Application.targetFrameRate = 60;

        instance = this;
        curServerIp = PlayerPrefs.GetString("curServerIp", "");
        loadTextConfig();
        getConfigSetting();

        TimeOpenApp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Input.multiTouchEnabled = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    VideoPlayer.EventHandler videoStartedListener;
    VideoPlayer.EventHandler videoEndedListener;
    public void playVideoSiXiang()
    {
        if (!videoPlayer.isPlaying)
        {
            videoBg.SetActive(false);
            videoBg.GetComponent<RawImage>().color = new Color32(255, 255, 225, 0);
            videoPlayer.gameObject.SetActive(true);

            videoPlayer.prepareCompleted += (vp) =>
            {
                Debug.Log("videoPlayer.prepareCompleted is run " + (float)videoPlayer.length);
                videoPlayer.Play();

                DOTween.Sequence().AppendInterval(1.4f).AppendCallback(() =>
                {
                    Debug.Log("showGame is run");
                    showGame();
                });
            };

            videoStartedListener = delegate
            {
                Debug.Log("videoStartedListener is run");
                videoBg.SetActive(true);
                videoBg.GetComponent<RawImage>().color = new Color32(255, 255, 225, 255);
                videoPlayer.started -= videoStartedListener;
            };

            videoEndedListener = delegate
            {
                Debug.Log("videoEndedListener is run");
                videoBg.SetActive(false);
                videoPlayer.gameObject.SetActive(false);
                videoPlayer.loopPointReached -= videoEndedListener;
            };

            videoPlayer.started += videoStartedListener;
            videoPlayer.loopPointReached += videoEndedListener;

            videoPlayer.errorReceived += (vp, message) =>
            {
                Debug.LogError("Error: " + message);
                showGame();
            };

            videoPlayer.Prepare();
        }
    }
    void Start()
    {
        lobbyView.hide(false);
        videoPlayer.Prepare();
        if (Screen.width <= Screen.height)
        {
            RectTransform videoRT = videoPlayer.GetComponent<RectTransform>();
            RectTransform bgVideoRT = videoBg.GetComponent<RectTransform>();
            float ratio = Mathf.Max(Screen.width / 720f, Screen.height / 1280f);
            videoRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ratio * 1280);
            videoRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ratio * 720);
            bgVideoRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ratio * 1280);
            bgVideoRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ratio * 720);
        }
    }

    public Sprite getRandomAvatar()
    {
        return avtDefault;
    }

    public bool isLoginShow()
    {
        return loginView.getIsShow();
    }

    public void showAlertMessage(JObject data)
    {
        if (loginView.getIsShow()) return;
        alertMessage.addAlertMessage(data);
    }
    void Update()
    {
        if (timeShowLoad > 0)
        {
            timeShowLoad -= Time.deltaTime;
            if (timeShowLoad <= 0)
            {
                hideWatting();
            }
        }
    }

    public void showWaiting(float timeOut = 10)
    {
        if (nodeLoad.activeSelf) return;
        timeShowLoad = timeOut;
        nodeLoad.SetActive(true);
    }

    public void hideWatting()
    {
        timeShowLoad = 0;
        nodeLoad.SetActive(false);
    }
    public void updateChipUser()
    {
        if (gameView != null && gameView.gameObject.activeSelf)
        {
            gameView.thisPlayer.updateMoney();
        }
        if (TableView.instance != null && TableView.instance.gameObject.activeSelf)
        {
            TableView.instance.updateAg();
        }
    }
    public void showLoginScreen(bool isReconnect = false)
    {

        if (loginView.getIsShow()) return;
        Logging.Log("UImanager showLoginScreen");
        if (seqPing != null)
        {
            seqPing.Kill();
        }
        seqPing = null;


        lobbyView.hide(false);

        if (TableView.instance != null)
        {
            Destroy(TableView.instance.gameObject);
        }
        TableView.instance = null;

        Logging.Log("gameView   " + (gameView != null));
        if (gameView != null)
        {
            Destroy(gameView);
        }
        gameView = null;
        destroyAllChildren(parentGame);
        dialogPool.Clear();
        listDialogOne.Clear();

        destroyAllPopup();

        WebSocketManager.getInstance().stop();
        SocketIOManager.getInstance().stopIO();
        loginView.show();
        if (isReconnect)
        {
            loginView.reconnect();
        }
    }

    public void showGame()
    {

        if (gameView != null && curGameId != (int)GAMEID.SLOT_SIXIANG)
        {

            Destroy(gameView.gameObject);

        }
        if (gameView != null && curGameId == (int)GAMEID.SLOT_SIXIANG)
        {
            return;
        }
        gameView = null;
        switch (curGameId)
        {
            case (int)GAMEID.DUMMY:
                {
                    Logging.Log("Di vao day RUMMY");
                    gameView = Instantiate(loadPrefabGame("DummyView"), parentGame).GetComponent<DummyView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    Logging.Log("showGame RUMMY 2   " + (gameView != null));
                    break;
                }

            case (int)GAMEID.SLOTNOEL:
                {
                    Logging.Log("showGame SLOTNOEL");
                    gameView = Instantiate(loadPrefabGame("SlotNoelView"), parentGame).GetComponent<SlotNoelView>();
                    break;
                }
            case (int)GAMEID.SLOTTARZAN:
                {
                    Logging.Log("showGame SLOTTARZAN");
                    gameView = Instantiate(loadPrefabGame("SlotTarzanView"), parentGame).GetComponent<SlotTarzanView>();
                    break;
                }
            case (int)GAMEID.SLOT_JUICY_GARDEN:
                {
                    Logging.Log("showGame SLOT_9900");
                    gameView = Instantiate(loadPrefabGame("SlotJuicyGardenView"), parentGame).GetComponent<SlotJuicyGardenView>();
                    break;
                }
            case (int)GAMEID.SLOT_INCA:
                {
                    Logging.Log("showGame SLOTINCA");

                    gameView = Instantiate(loadPrefabGame("SlotInCaView"), parentGame).GetComponent<SlotInCaView>();
                    break;
                }
            case (int)GAMEID.SLOT_SIXIANG:
                {
                    Logging.Log("showGame SLOT_SIXIANG");
                    gameView = Instantiate(loadPrefabGame("SiXiangView"), parentGame).GetComponent<SiXiangView>();
                    break;
                }
            case (int)GAMEID.SLOT20FRUIT:
                {
                    Logging.Log("showGame SLOT20FRUIT");
                    gameView = Instantiate(loadPrefabGame("SlotFruitView"), parentGame).GetComponent<SlotFruitView>();
                    break;
                }
            case (int)GAMEID.LUCKY_89:
                {
                    Logging.Log("showGame Lucky89");
                    gameView = Instantiate(loadPrefabGame("Lucky89View"), parentGame).GetComponent<Lucky89View>();
                    break;
                }
            case (int)GAMEID.KEANG:
                {
                    Logging.Log("showGame KEANG");
                    gameView = Instantiate(loadPrefabGame("KeangView"), parentGame).GetComponent<KeangView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }
            case (int)GAMEID.GAOGEA:
                {
                    Logging.Log("showGame GAOGEA");
                    gameView = Instantiate(loadPrefabGame("GaoGeaView"), parentGame).GetComponent<GaoGeaView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }
            case (int)GAMEID.SICBO:
                {
                    Logging.Log("showGame SICBO");
                    gameView = Instantiate(loadPrefabGame("SicboView"), parentGame).GetComponent<SicboView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }
            case (int)GAMEID.BANDAR_QQ:
                {
                    Logging.Log("showGame Bandar");
                    gameView = Instantiate(loadPrefabGame("BandarQQView"), parentGame).GetComponent<BandarQQView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }

            case (int)GAMEID.RONGHO:
                {
                    Logging.Log("showGame RONGHO");
                    gameView = Instantiate(loadPrefabGame("DragonTigerView"), parentGame).GetComponent<DragonTigerView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }
            case (int)GAMEID.DOMINO:
                {
                    Logging.Log("showGame DOMINO");
                    gameView = Instantiate(loadPrefabGame("DominoGaple"), parentGame).GetComponent<DominoGapleView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }

            case (int)GAMEID.BACCARAT:
                {
                    Logging.Log("showGame BACCARAT");
                    gameView = Instantiate(loadPrefabGame("BaccaratView"), parentGame).GetComponent<BaccaratView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }
            case (int)GAMEID.PUSOY:
                {
                    Logging.Log("showGame BINH");
                    gameView = Instantiate(loadPrefabGame("BinhView"), parentGame).GetComponent<BinhGameView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    Debug.Log("Set Game View Binh:" + gameView);
                    break;
                }
            case (int)GAMEID.KARTU_QIU:
                {
                    Logging.Log("showGame KARTU_QIU");
                    gameView = Instantiate(loadPrefabGame("BorkKdengView"), parentGame).GetComponent<BorkKDengView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }
            case (int)GAMEID.BLACKJACK:
                {
                    Logging.Log("showGame BLACKJACK");
                    gameView = Instantiate(loadPrefabGame("BlackJackView"), parentGame).GetComponent<BlackJackView>();
                    //gameView.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }
            case (int)GAMEID.TONGITS_OLD:
                {
                    Logging.Log("showGame TONGITS thuong");
                    gameView = Instantiate(loadPrefabGame("TongitsView"), parentGame).GetComponent<TongitsView>();
                    break;
                }
            case (int)GAMEID.TONGITS:
                {
                    Logging.Log("showGame TONGITS butasan");
                    gameView = Instantiate(loadPrefabGame("TongitsView"), parentGame).GetComponent<TongitsView>();
                    break;
                }
            case (int)GAMEID.TONGITS_JOKER:
                {
                    Logging.Log("showGame TONGITS joker");
                    gameView = Instantiate(loadPrefabGame("TongitsView"), parentGame).GetComponent<TongitsView>();
                    break;
                }
            case (int)GAMEID.LUCKY9:
                {
                    Logging.Log("showGame LUCKY9");
                    gameView = Instantiate(loadPrefabGame("Lucky9View"), parentGame).GetComponent<Lucky9View>();
                    break;
                }
            case (int)GAMEID.SABONG:
                {
                    Logging.Log("showGame SABONG");
                    gameView = Instantiate(loadPrefabGame("SabongView"), parentGame).GetComponent<SabongGameView>();
                    break;
                }
            case (int)GAMEID.MINE_FINDING:
                {
                    Logging.Log("showGame MineFinding");
                    gameView = Instantiate(loadPrefabGame("PopupMineFinding"), parentGame).GetComponent<MineFindingView>();
                    break;
                }
            case (int)GAMEID.ROULETTE:
                {
                    Logging.Log("showGame ROULETTE");
                    gameView = Instantiate(loadPrefabGame(""), parentGame).GetComponent<SabongGameView>();
                    break;
                }
            case (int)GAMEID.BAUCUA:
                {
                    Logging.Log("showGame BAUCUA");
                    gameView = Instantiate(loadPrefabGame("SabongView"), parentGame).GetComponent<SabongGameView>();
                    break;
                }
            default:
                {
                    Logging.Log("-=-= chua co game nao ma vao. Lm thi tu them vao di;;;;");
                    break;
                }
        }
        if (gameView != null)
        {
            CURRENT_VIEW.setCurView(curGameId.ToString());
            if (TableView.instance)
                TableView.instance.hide(false);
            if (lobbyView.getIsShow())
                lobbyView.hide(false);
            gameView.transform.localScale = Vector3.one;

            destroyAllPopup();
        }
    }

    public void destroyAllChildren(Transform transform)
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
    public void destroyAllPopup()
    {
        destroyAllChildren(parentPopups);
        destroyAllChildren(parentBanner);
    }
    public void DOTextTmp(TextMeshProUGUI tmp, string text, float time = 0.5f)
    {
        GameObject lbTemp = new GameObject("lbTemp");
        Text lbText = lbTemp.AddComponent<Text>();
        lbText.DOText(text, time).OnUpdate(() =>
        {
            tmp.text = lbText.text;
        }).OnComplete(() =>
        {
            Destroy(lbTemp);
        });
    }

    public void updateVip()
    {
        if (gameView != null)
        {
            gameView.updateVip();
        }
    }
    public void updateAG()
    {
        if (gameView != null && gameView.gameObject.activeSelf)
        {
            gameView.thisPlayer.updateMoney();
        }
        if (TableView.instance != null && TableView.instance.gameObject.activeSelf)
        {
            TableView.instance.updateAg();
        }
    }

    Sequence seqPing;
    public void showLobbyScreen(bool isFromLogin = false)
    {
        Logging.Log("showLobbyScreen  ");
        loginView.hide(false);
        destroyAllChildren(parentPopups);
        lobbyView.show();
        SocketSend.getFarmInfo();
        if (gameView != null)
            Destroy(gameView.gameObject);
    }


    public GameObject loadPrefabPopup(string name)
    {
        return loadPrefab("Popups/" + name);
    }

    public GameObject loadPrefabGame(string name)
    {
        return loadPrefab("GameView/" + name);
    }
    public Sprite LoadChipImage(int chipId)
    {
        return Resources.Load<Sprite>("Sprite Assets/Chips/chip_" + chipId);
    }
    public GameObject loadPrefab(string path)
    {
        return Resources.Load(path) as GameObject;
    }
    public SkeletonDataAsset loadSkeletonData(string path)
    {
        return Resources.Load<SkeletonDataAsset>(path);

    }
    public IEnumerator loadSkeletonDataAsync(string path, Action<SkeletonDataAsset> cb)
    {
        ResourceRequest resourceRequest = Resources.LoadAsync<SkeletonDataAsset>(path);
        yield return resourceRequest;
        cb(resourceRequest.asset as SkeletonDataAsset);
    }

    void createMessageBox(GameObject prefab, string msg, Action callback1 = null, bool isHaveClose = false)
    {
        DialogView dialog;
        if (dialogPool.Count == 0)
        {
            Debug.Log("-=-=listDialogOne  " + listDialogOne.Count);
            if (listDialogOne.FirstOrDefault(x => x.getMessage().Equals(msg)) == null)
            {
                dialog = Instantiate(prefab, parentPopups).GetComponent<DialogView>();
            }
            else return;
        }
        else
        {
            dialog = listDialogOne.FirstOrDefault(x => x.getMessage().Equals(msg));
            if (dialog == null)
            {
                dialog = dialogPool[0];
                dialogPool.RemoveAt(0);
                dialog.transform.parent = parentPopups;
            }
        }

        listDialogOne.Add(dialog);
        dialog.gameObject.SetActive(true);
        dialog.transform.localScale = Vector3.one;
        dialog.transform.SetAsLastSibling();
        dialog.setMessage(msg);
        dialog.setIsShowButton1(true, getTextConfig("ok"), callback1);
        dialog.setIsShowButton2(false, "", null);
        dialog.setIsShowClose(isHaveClose, null);

        if (Screen.width < Screen.height) dialog.transform.localRotation = Quaternion.Euler(dialog.transform.localRotation.x, dialog.transform.localRotation.y, 0);
        else
        {
            if (gameView != null)
            {
                dialog.transform.eulerAngles = gameView.transform.eulerAngles;
                if (dialog.transform.eulerAngles.z != 0)
                {
                    dialog.setLanscape();
                }
            }
        }
    }

    public void showMessageBox(string msg, Action callback1 = null, bool isHaveClose = false)
    {
        if (msg == "") return;
#if DEVGAME
        AssetBundleManager.instance.loadPrefab(AssetBundleName.POPUPS, "Dialog", (prefab) =>
        {
            createMessageBox(prefab, msg, callback1, isHaveClose);
        });
#else
        createMessageBox(loadPrefabPopup("Dialog"), msg, callback1, isHaveClose);
#endif
    }

    DialogView createDialog(GameObject prefab, string msg, string nameBtn1 = "", Action callback1 = null, string nameBtn2 = "", Action callback2 = null, bool isShowClose = false, Action callback3 = null)
    {
        DialogView dialog;
        if (dialogPool.Count == 0)
        {
            dialog = listDialogOne.FirstOrDefault(x => x.getMessage().Equals(msg));
            if (dialog == null)
            {
                dialog = Instantiate(prefab, parentPopups).GetComponent<DialogView>();
                listDialogOne.Add(dialog);
            }
        }
        else
        {
            dialog = listDialogOne.FirstOrDefault(x => x.getMessage().Equals(msg));
            if (dialog == null)
            {
                dialog = dialogPool[0];
                dialogPool.RemoveAt(0);
                dialog.transform.parent = parentPopups;
                listDialogOne.Add(dialog);
            }
        }
        dialog.gameObject.SetActive(true);
        dialog.transform.localScale = Vector3.one;
        dialog.transform.SetAsLastSibling();
        dialog.setMessage(msg);
        dialog.setIsShowButton1(nameBtn1 != "", nameBtn1, callback1);
        dialog.setIsShowButton2(nameBtn2 != "", nameBtn2, callback2);
        dialog.setIsShowClose(isShowClose, callback3);
        if (Screen.width < Screen.height) dialog.transform.localRotation = Quaternion.Euler(dialog.transform.localRotation.x, dialog.transform.localRotation.y, 0);
        else
        {
            if (gameView != null)
            {
                dialog.transform.eulerAngles = gameView.transform.eulerAngles;
                if (dialog.transform.eulerAngles.z != 0)
                {
                    dialog.setLanscape();
                }
            }
        }
        return dialog;
    }
    public void showDialog(string msg, string nameBtn1 = "", Action callback1 = null, string nameBtn2 = "", Action callback2 = null, bool isShowClose = false, Action callback3 = null, Action<DialogView> callbaclReturn = null)
    {
#if DEVGAME
        AssetBundleManager.instance.loadPrefab(AssetBundleName.POPUPS, "Dialog", (prefab) =>
        {
            var dialog = createDialog(loadPrefabPopup("Dialog"), msg, nameBtn1 = "", callback1, nameBtn2, callback2, isShowClose, callback3);
            if (callbaclReturn != null)
            {
                callbaclReturn.Invoke(dialog);
            }
        });
#else 
        var dialog = createDialog(loadPrefabPopup("Dialog"), msg, nameBtn1, callback1, nameBtn2, callback2, isShowClose, callback3);
        if (callbaclReturn != null)
        {
            callbaclReturn.Invoke(dialog);
        }
#endif
    }

    public void showToast(string msg, Transform tfParent)
    {
        showToast(msg, 2, tfParent);
    }
    public void showToast(string msg, float timeShow = 2, Transform tfParent = null)
    {
        Logging.Log("Show Toast:" + msg);
        var compToast = createSprite(sf_toast);
        compToast.transform.SetParent(tfParent != null ? tfParent : transform);
        compToast.transform.SetAsLastSibling();
        compToast.type = Image.Type.Sliced;
        compToast.rectTransform.sizeDelta = new Vector2(400, 80);
        compToast.rectTransform.localScale = Vector3.one;
        compToast.transform.localPosition = new Vector2(0, -Screen.height / 4);


        var lbCom = createLabel(msg, 30);
        lbCom.rectTransform.SetParent(compToast.rectTransform);
        lbCom.rectTransform.localScale = Vector3.one;
        lbCom.color = Color.white;
        lbCom.alignment = TextAlignmentOptions.Center;
        lbCom.enableWordWrapping = false;

        if (lbCom.preferredWidth > compToast.rectTransform.sizeDelta.x)
        {
            compToast.rectTransform.sizeDelta = new Vector2(lbCom.preferredWidth + 100, compToast.rectTransform.sizeDelta.y);
        }

        lbCom.rectTransform.sizeDelta = new Vector2(390, 50);
        lbCom.transform.localPosition = new Vector2(0, 5);

        if (gameView != null)
        {
            compToast.transform.eulerAngles = gameView.transform.eulerAngles;
            if (gameView.transform.eulerAngles.z == 0)
            {
                compToast.rectTransform.anchoredPosition = new Vector3(0, -150);
            }
            else
                compToast.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
        }


        compToast.rectTransform.localScale = Vector3.zero;
        DOTween.Sequence().Append(compToast.rectTransform.DOScale(1, .5f).SetEase(Ease.OutBack)).Append(compToast.rectTransform.DOScale(0, .5f).SetEase(Ease.InBack).SetDelay(timeShow)).AppendCallback(() =>
        {
            Destroy(compToast.gameObject);
        }).SetAutoKill(true);
    }

    public void openRuleJPBork()
    {
        Debug.Log("openRuleJPBork:");
        var ruleView = Instantiate(loadPrefab("GameView/Bork/JackpotRuleBork"), parentPopups).GetComponent<BaseView>();
        ruleView.transform.localScale = Vector3.one;
    }
    public void openRuleJPBinh()
    {
        var ruleView = Instantiate(loadPrefab("GameView/Binh/JackpotRuleBinh"), parentPopups).GetComponent<BaseView>();
        ruleView.transform.localScale = Vector3.one;
    }

    public void openFriendInfo()
    {
        var friendInfoView = Instantiate(loadPrefabPopup("PopupFriendInfo"), parentPopups).GetComponent<FriendInfoView>();
        friendInfoView.transform.localScale = Vector3.one;
    }


    bool isOnProfile = true;
    public void openSetting()
    {
        var settingView = Instantiate(loadPrefabPopup("PopupSetting"), parentPopups).GetComponent<SettingView>();
        settingView.transform.localScale = Vector3.one;
    }
    public void openCreateTableView()
    {
        var createTableView = Instantiate(loadPrefabPopup("PopupCreateTable"), parentPopups).GetComponent<CreateTableView>();
        createTableView.transform.localScale = Vector3.one;
    }

    public void openInputPass(int tableID)
    {
        var inputPassView = Instantiate(loadPrefabPopup("PopupInputPass"), parentPopups).GetComponent<InputPassView>();
        inputPassView.setTableID(tableID);
        inputPassView.transform.localScale = Vector3.one;
    }

    public void openTableView()
    {
        if (TableView.instance == null)
        {
            TableView tableView = Instantiate(loadPrefab("Table/TableView"), transform).GetComponent<TableView>();
        }
        else
        {
            TableView.instance.transform.SetParent(transform);
            TableView.instance.show();
        }
        TableView.instance.transform.SetSiblingIndex(2);
        TableView.instance.transform.localScale = Vector3.one;
        lobbyView.hide(false);
    }


    public void showPopupWhenLostChip(bool isBackFromGame = false, bool isChooseGame = false)
    {
        Debug.Log("showPopupWhenLostChip");
        var money = User.userMain.AG;
        if (money <= 0)
        {
            var isInGame = false;
            if (gameView != null && !isBackFromGame) isInGame = true;
            var textShow = getTextConfig("has_mail_show_gold");
            var textBtn1 = getTextConfig("txt_free_chip");
            var textBtn2 = getTextConfig("shop");
            var textBtn3 = getTextConfig("label_cancel");
            if (isInGame)
            {
                textShow = textShow.Split(",")[0];
                textBtn1 = textBtn3;
                textBtn2 = textBtn3;
            }
            if (isChooseGame) textShow = getTextConfig("txt_not_enough_money_gl");
            if (User.userMain.nmAg > 0 || Promotion.countMailAg > 0 ||
                 Promotion.adminMoney > 0
            )
            {
                showDialog(textShow, textBtn1, () =>
                {

                }, textBtn2, () =>
                {

                }, true);
            }
            else
            {
                textShow = getTextConfig("txt_not_enough_money_gl");
                showDialog(textShow, textBtn2, () =>
                {

                }, textBtn3);
            }
        }
    }


    public JArray arrayDataBannerIO;


    public void FixedUpdate()
    {
        foreach (var wwload in listDataLoad.ToArray())//new List<DataLoadImage>(listDataLoad))
        {
            if (wwload != null && !wwload.isDone && wwload.www.isDone)
            {
                wwload.isDone = true;
                if (wwload.sprite != null && !wwload.sprite.IsDestroyed())
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(wwload.www);
                    wwload.sprite.sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                }

                if (wwload.callback != null)
                {
                    wwload.callback.Invoke(DownloadHandlerTexture.GetContent(wwload.www));
                }
                if (wwload.callback2 != null)
                {
                    wwload.callback2.Invoke();
                }
            }
        }

        for (var i = 0; i < listDataLoad.Count; i++)
        {
            if (listDataLoad[i].isDone)
            {
                listDataLoad.RemoveAt(i);
                i--;
            }
        }
    }

    List<DataLoadImage> listDataLoad = new List<DataLoadImage>();
    public void addJobLoadImage(DataLoadImage dataLoadImage)
    {
        listDataLoad.Add(dataLoadImage);
    }
}
