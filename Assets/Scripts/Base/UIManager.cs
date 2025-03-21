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

    public SpriteAtlas avatarAtlas;
    [SerializeField] Sprite avtDefault;
    [SerializeField] Canvas canvasGame;
    [HideInInspector] public GameView gameView;
    [SerializeField] AlertMessage alertMessage;

    public List<DialogView> dialogPool = new List<DialogView>();
    public List<DialogView> listDialogOne = new List<DialogView>();
    public long PusoyJackPot;
    public bool SendChatEmoToHiddenPlayers = false;

    void Awake()
    {
        instance = this;
        curServerIp = PlayerPrefs.GetString("curServerIp", "");
        loadTextConfig();
        getConfigSetting();

        TimeOpenApp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Input.multiTouchEnabled = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        lobbyView.hide(false);
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
        lobbyView.updateAg();
        if (gameView != null && gameView.gameObject.activeSelf)
        {
            gameView.thisPlayer.updateMoney();
        }
    }
    public void showLoginScreen(bool isReconnect = false)
    {

        if (loginView.getIsShow()) return;
        Globals.Logging.Log("UImanager showLoginScreen");
        if (seqPing != null)
        {
            seqPing.Kill();
        }
        seqPing = null;


        lobbyView.hide(false);


        Globals.Logging.Log("gameView   " + (gameView != null));
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

        if (gameView != null && curGameId != (int)Globals.GAMEID.SLOT_SIXIANG)
        {

            Destroy(gameView.gameObject);

        }
        if (gameView != null && curGameId == (int)Globals.GAMEID.SLOT_SIXIANG)
        {
            return;
        }
        gameView = null;
        switch (curGameId)
        {
            case (int)Globals.GAMEID.SLOTTARZAN:
                {
                    Globals.Logging.Log("showGame SLOTTARZAN");
                    gameView = Instantiate(loadPrefabGame("SlotTarzanView"), parentGame).GetComponent<SlotTarzanView>();
                    break;
                }
            default:
                {
                    Globals.Logging.Log("-=-= chua co game nao ma vao. Lm thi tu them vao di;;;;");
                    break;
                }
        }
        if (gameView != null)
        {
            Globals.CURRENT_VIEW.setCurView(curGameId.ToString());
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
    public void updateAvatar()
    {
        lobbyView.updateAvatar();
    }

    public void updateVip()
    {
        lobbyView.updateAg();
        lobbyView.updateAgSafe();
        if (gameView != null)
        {
            gameView.updateVip();
        }
    }
    public void updateInfo()
    {
        lobbyView.updateInfo();
    }
    public void updateCanInviteFriend()
    {
        lobbyView.updateCanInviteFriend();
    }
    public void updateAG()
    {
        lobbyView.updateAg();
        if (gameView != null && gameView.gameObject.activeSelf)
        {
            gameView.thisPlayer.updateMoney();
        }
    }
    public void updateAGSafe()
    {
        lobbyView.updateAgSafe();
    }

    Sequence seqPing;
    public void showLobbyScreen(bool isFromLogin = false)
    {
        Globals.Logging.Log("showLobbyScreen  ");
        loginView.hide(false);
        destroyAllChildren(parentPopups);
        lobbyView.show();
        lobbyView.updateInfo();
        SocketSend.getFarmInfo();
        if (gameView != null)
            Destroy(gameView.gameObject);
    }




    public void setTimeOnline()
    {
        lobbyView.setTimeGetMoney();
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
        //loadAsyncTask.Start();
    }

    void createMessageBox(GameObject prefab, string msg, Action callback1 = null, bool isHaveClose = false)
    {
        //new Thread(new ThreadStart(() =>
        //{
        DialogView dialog;
        if (dialogPool.Count == 0)
        {
            //messageBox = Instantiate(loadPrefabPopup("Dialog"), parentPopups).GetComponent<DialogView>();
            Debug.Log("-=-=listDialogOne  " + listDialogOne.Count);
            if (listDialogOne.FirstOrDefault(x => x.getMessage().Equals(msg)) == null)
            {
                dialog = Instantiate(prefab, parentPopups).GetComponent<DialogView>();
            }
            else return;
        }
        else
        {
            //dialog = dialogPool[0];
            //dialogPool.RemoveAt(0);
            //dialog.transform.parent = parentPopups;
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
        AssetBundleManager.instance.loadPrefab(Globals.AssetBundleName.POPUPS, "Dialog", (prefab) =>
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
            //dialog = Instantiate(loadPrefabPopup("Dialog"), parentPopups).GetComponent<DialogView>();
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
        AssetBundleManager.instance.loadPrefab(Globals.AssetBundleName.POPUPS, "Dialog", (prefab) =>
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

    public void showWebView(string url, string title = "")
    {
    }
    public void showToast(string msg, Transform tfParent)
    {
        showToast(msg, 2, tfParent);
    }
    public void showToast(string msg, float timeShow = 2, Transform tfParent = null)
    {
        Globals.Logging.Log("Show Toast:" + msg);
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




    public void openSetting()
    {
        //curGameId = (int)Globals.GAMEID.KEANG;
        //UIManager.instance.showGame();
        var settingView = Instantiate(loadPrefabPopup("PopupSetting"), parentPopups).GetComponent<SettingView>();
        settingView.transform.localScale = Vector3.one;
    }



    public void showPopupWhenNotEnoughChip()
    {
        var isInGame = false;
        if (gameView != null) isInGame = true;
        //var typeBTN = isInGame ? DIALOG_TYPE.ONE_BTN : DIALOG_TYPE.TWO_BTN;
        var textShow = getTextConfig("txt_not_enough_money_gl");
        var textBtn1 = getTextConfig("txt_free_chip");
        var textBtn2 = getTextConfig("shop");
        var textBtn3 = getTextConfig("label_cancel");
        if (isInGame)
        {
            textShow = textShow.Split(",")[0];
            textBtn1 = textBtn3;
            textBtn2 = textBtn3;
        }
        if (Globals.User.userMain.nmAg > 0 || Globals.Promotion.countMailAg > 0 || Globals.Promotion.adminMoney > 0)
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

    public void showPopupWhenLostChip(bool isBackFromGame = false, bool isChooseGame = false)
    {
        Debug.Log("showPopupWhenLostChip");
        var money = Globals.User.userMain.AG;
        if (money <= 0)
        {
            var isInGame = false;
            if (gameView != null && !isBackFromGame) isInGame = true;
            //var typeBTN = isInGame ? DIALOG_TYPE.ONE_BTN : DIALOG_TYPE.TWO_BTN;
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
            if (Globals.User.userMain.nmAg > 0 || Globals.Promotion.countMailAg > 0 ||
                 Globals.Promotion.adminMoney > 0
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
