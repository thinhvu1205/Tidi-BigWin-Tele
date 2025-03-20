using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;
using DG.Tweening;
using Globals;

public class PlayerView : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI txtName, txtMoney;
    [SerializeField]
    public Avatar avatar;
    [SerializeField]
    Image timeCounDown;

    [SerializeField]
    GameObject objHost, objExit, dealerIcon, bkgThanhBar;
    [SerializeField]
    TextMeshProUGUI lbChipWinLose;

    [SerializeField]
    TMP_FontAsset fontWin, fontLose;

    float timeTurn = 0;
    public long agLose = 0, agWin = 0;
    private long agCurrent = 0;
    public bool isThisPlayer = false;

    [SerializeField]
    public SkeletonGraphic animResult;

    [SerializeField]
    public GameObject aniAllIn;


    [SerializeField]
    public GameObject hitpot;

    [SerializeField]
    public List<GameObject> pots;


    [SerializeField]
    [Tooltip("0-lose, 1-draw, 2-win")]
    public List<SkeletonDataAsset> listAnimResult;
    [HideInInspector]
    private Sequence seqTextFly;

    GameObject itemVip;

    public void setName(string namePl)
    {
        txtName.text = namePl;
        Globals.Config.effectTextRunInMask(txtName);
    }

    public void setAvatar(int avaId, string fname, string Faid, int vip)
    {
        //Debug.Log("avaId=" + avaId);
        //Debug.Log("fname=" + fname);
        //Debug.Log("Faid=" + Faid);
        avatar.loadAvatarAsync(avaId, fname, Faid);
        avatar.setVip(vip);

    }

    bool isOnItemVip = true;
    public void updateItemVip(int idVip, int vip, int idPosTongits = -1)
    {
        if (vip >= 5 && idVip != 0)
        {
            if (itemVip == null && isOnItemVip)
            {
                isOnItemVip = false;
                itemVip = Instantiate(UIManager.instance.loadPrefab("GameView/Objects/ItemVip"), transform);
            }
            var vecPos = itemVip.transform.localPosition;
            var vecPosThis = transform.localPosition;
            var size = gameObject.GetComponent<RectTransform>().sizeDelta;
            if (Config.curGameId == (int)GAMEID.TONGITS_OLD || Config.curGameId == (int)GAMEID.TONGITS_JOKER || Config.curGameId == (int)GAMEID.TONGITS)
            {
                switch (idPosTongits)
                {
                    case 0: vecPos.x = 55; break;
                    case 1: vecPos.x = -100; break;
                    case 2: vecPos.x = 100; break;
                }
            }
            else if (Config.curGameId == (int)GAMEID.LUCKY_89 || Config.curGameId == (int)GAMEID.GAOGEA)
            {
                vecPos.x = vecPosThis.x < 0 ? 100 : -100;
                vecPos.y = -60;
            }
            else if (Config.curGameId == (int)GAMEID.SICBO)
            {
                vecPos.x = vecPosThis.x < 0 ? -100 : 100;
                vecPos.y = 0;
            }
            else
            {
                vecPos.x = vecPosThis.x > 0 ? 100 : -100;
                vecPos.y = -60;
            }
            itemVip.SetActive(true);
            itemVip.transform.localPosition = vecPos;
            updateItemVipFromSV(idVip);
            itemVip.transform.SetAsLastSibling();
            Button btn = itemVip.GetComponent<Button>();
            btn.interactable = (vip > 5) && isThisPlayer;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { onClickSelectItemVip(vip); });
        }
        else
        {
            if (itemVip != null) itemVip.SetActive(false);
        }
    }


    public void OnDisable()
    {
        OnDestroy();
    }

    public void OnDestroy()
    {
        if (itemVip != null)
        {
            Destroy(itemVip.gameObject);
        }
        itemVip = null;
        isOnItemVip = true;

        if (BkgVip != null)
        {
            Destroy(BkgVip.gameObject);
        }
        BkgVip = null;
    }

    public void updateItemVipFromSV(int idItem)
    {
        Debug.Log("idItem  " + idItem);
        var itemIdVip = idItem / 10;
        if (itemIdVip > 10) itemIdVip = 10;
        Debug.Log("itemIdVip  " + itemIdVip);
        if (itemVip != null && itemIdVip >= 5)
        {
            GameObject animGO = itemVip.transform.Find("anim").gameObject;
            if (animGO != null)
            {
                SkeletonGraphic animSG = animGO.GetComponent<SkeletonGraphic>();
                if (animSG != null) animSG.AnimationState.SetAnimation(0, itemIdVip.ToString(), true);
            }
        }
    }

    Transform BkgVip;
    void onClickSelectItemVip(int vip)
    {
        if (itemVip != null && isThisPlayer)
        {
            if (BkgVip == null)
            {
                BkgVip = Instantiate(UIManager.instance.loadPrefab("GameView/Objects/BkgItemVip"), UIManager.instance.gameView.transform).transform;
                //BkgVip.gameObject.SetActive(false);
                //var bkgItems = itemVip.transform.GetChild(0);

                for (var i = 0; i < BkgVip.childCount; i++)
                {
                    var index = i;
                    var item = BkgVip.GetChild(i);
                    if (index + 5 <= vip)
                    {
                        item.gameObject.SetActive(true);
                        item.GetComponent<Button>().onClick.RemoveAllListeners();
                        item.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            onClickItemVip(index + 5);
                        });
                    }
                    else
                    {
                        item.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                BkgVip.gameObject.SetActive(!BkgVip.gameObject.activeSelf);
            }


            if (BkgVip.gameObject.activeSelf)
            {
                //var sizeScreen = Screen.currentResolution;
                BkgVip.SetAsLastSibling();
                //BkgVip.localScale = itemVip.transform.localScale;
                var scale = itemVip.transform.localScale.x;
                BkgVip.localScale = Vector3.zero;

                DOTween.Sequence().AppendInterval(0.2f).AppendCallback(() =>
                {
                    var sizeBkg = BkgVip.GetComponent<RectTransform>().sizeDelta;
                    var vecPosThis = transform.localPosition;
                    var posBkg = BkgVip.localPosition;

                    if (itemVip.transform.localPosition.x < 0)
                    {
                        posBkg.x = vecPosThis.x - 100;
                    }
                    else
                    {
                        posBkg.x = vecPosThis.x + 100;
                    }
                    if (Globals.Config.curGameId == (int)Globals.GAMEID.SICBO)
                    {
                        posBkg.y = vecPosThis.y - sizeBkg.y * scale - 30;
                    }
                    else
                        posBkg.y = vecPosThis.y - 30;

                    BkgVip.localPosition = posBkg;
                    BkgVip.DOScale(itemVip.transform.localScale, .2f);
                });
            }
        }
    }


    void onClickItemVip(int vip)
    {
        SocketSend.sendUpdateItemVip(vip);
        if (BkgVip != null)
        {
            BkgVip.gameObject.SetActive(false);
        }
    }

    public void setAg(long ag)
    {
        //txtMoney.text = Globals.Config.FormatMoney(ag);
        Globals.Config.tweenNumberTo(txtMoney, ag, agCurrent, 0.3f, false, false);
        agCurrent = ag;
    }

    public void setCallbackClick(System.Action callback)
    {
        var btnCom = avatar.gameObject.GetComponent<Button>();
        if (btnCom == null)
        {
            btnCom = avatar.gameObject.AddComponent<Button>();
        }
        btnCom.onClick.RemoveAllListeners();
        btnCom.onClick.AddListener(() =>
        {
            Globals.Logging.Log("Click avatar");
            callback();
        });
    }

    public void setPosThanhBarThisPlayer()
    {
        if (
            Globals.Config.curGameId == (int)Globals.GAMEID.TONGITS_JOKER ||
            Globals.Config.curGameId == (int)Globals.GAMEID.TONGITS ||
            Globals.Config.curGameId == (int)Globals.GAMEID.TONGITS11 ||
            Globals.Config.curGameId == (int)Globals.GAMEID.TONGITS_OLD

        )
        {
            bkgThanhBar.transform.localPosition = new Vector2(-120, 5);
            bkgThanhBar.GetComponent<Image>().enabled = false;
            txtMoney.fontSize = 23;
            txtName.fontSize = 26;
            txtName.gameObject.transform.parent.transform.localPosition = new Vector2(txtName.gameObject.transform.parent.transform.localPosition.x, 15);
            return;
        }
        else if (Globals.Config.curGameId == (int)Globals.GAMEID.SICBO)
        {
            bkgThanhBar.transform.localPosition = new Vector2(113, -20);
        }
        else
        {
            bkgThanhBar.transform.localPosition = new Vector2(120, -12);
        }
    }

    public bool getIsTurn()
    {
        return timeCounDown.gameObject.activeInHierarchy;
    }
    public Sprite getAvatarSprite()
    {
        return avatar.image.sprite;
    }
    public void setDark(bool isDark)
    {
        avatar.setDark(isDark);

    }

    public void setExit(bool isExit)
    {
        objExit.SetActive(isExit);
    }

    public void effectFlyMoney(long mo, int fonzSize = 50)
    {
        if (mo == 0)
        {
            return;
        }
        lbChipWinLose.fontSize = fonzSize;
        if (mo < 0)
        {
            lbChipWinLose.font = fontLose;
            lbChipWinLose.text = Globals.Config.FormatMoney2(mo, true, true);
        }
        else
        {
            lbChipWinLose.font = fontWin;
            lbChipWinLose.text = "+" + Globals.Config.FormatMoney2(mo, true, true);
        }


        lbChipWinLose.transform.localPosition = Vector2.zero;
        int height = 100;
        if (Globals.Config.curGameId == (int)Globals.GAMEID.SICBO && transform.localPosition.y > 280)
        {
            height = 50;
        }
        if (Globals.Config.curGameId == (int)Globals.GAMEID.BANDAR_QQ || Globals.Config.curGameId == (int)Globals.GAMEID.PUSOY)
        {
            lbChipWinLose.transform.localPosition = new Vector2(0, -30);
            height = 50;
        }
        if (Globals.Config.curGameId == (int)Globals.GAMEID.RONGHO)
        {
            //lbChipWinLose.transform.localPosition = new Vector2(0, );
            height = 50;
        }
        if (Globals.Config.curGameId == (int)Globals.GAMEID.BLACKJACK)
        {
            //lbChipWinLose.transform.localPosition = new Vector2(0, );
            height = 60;
        }
        if (Globals.Config.curGameId == (int)Globals.GAMEID.BACCARAT)
        {
            height = 60;
        }
        if (Globals.Config.curGameId == (int)Globals.GAMEID.KARTU_QIU)
        {
            height = 60;
        }
        if (Globals.Config.curGameId == (int)Globals.GAMEID.DOMINO)
        {
            lbChipWinLose.transform.localPosition = new Vector2(0, -30);
            height = 50;
        }
        if (Globals.Config.curGameId == (int)Globals.GAMEID.LUCKY9)
        {
            height = 35;
        }
        lbChipWinLose.gameObject.SetActive(true);
        if (seqTextFly != null)
        {
            seqTextFly.Kill();
        }
        seqTextFly = DOTween.Sequence()
             .Append(lbChipWinLose.transform.DOLocalMove(new Vector2(0, height), 2.0f).SetEase(Ease.OutBack))
             .AppendInterval(1.0f)
             .AppendCallback(() =>
             {
                 lbChipWinLose.gameObject.SetActive(false);
             });
    }

}
