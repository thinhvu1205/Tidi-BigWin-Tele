using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;
using Globals;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class ExchangeView : BaseView
{
    public static ExchangeView instance;
    [SerializeField] List<Sprite> spTab;
    [SerializeField]
    GameObject
        tabTop, itemEx, itemAgency, itemHistory, m_Keyboard, m_OpenKeyboardPhone, m_OpenKeyBoardConfirmPhone,
        m_HighlightPhone, m_HighlightConfirmPhone, m_HighlightTonAmount;
    [SerializeField] Transform m_PrefabHistoryTf, m_HistoryTf, m_TonBoardTf;
    [SerializeField] TextMeshProUGUI lbChips, m_RewardTMP, m_HistoryTMP, m_TonRateTMP, m_TonWalletTMP;
    [SerializeField] private Button m_WithdrawBtn;
    [SerializeField] BaseView popupInput;
    [SerializeField] ScrollRect scrContentRedeem, scrContentAgency, scrContentHistory, scrTabs, scrTabsHis;
    [SerializeField] private InputField m_PhoneIF, m_ConfirmPhoneIF, m_TonAmountIF;

    private List<JObject> listDataHis = new List<JObject>();
    private JObject firstTabHistItem, curDataTabNap, rewardData;
    private JArray dataCO;
    private InputField _CurrentIF;
    private Transform _TabTonTf, _TabHistoryTonTf;
    private string typeTabHistory = "", typeNet;
    private int indexTabHis = 0, indexTabNap = 1, valueCO;
    private float _contentHeight = 0;

    #region Button
    public override void onClickClose(bool isDestroy = true)
    {
        if (!Config.TELEGRAM_TOKEN.Equals("")) SocketSend.sendSelectGame(Config.curGameId);
        else base.onClickClose(true);
    }
    public void DoClickCloseInput()
    {
        m_Keyboard.SetActive(false);
        popupInput.gameObject.SetActive(false);
    }
    public void DoClickNumber(int number)
    {
        if (_CurrentIF == null) return;
        if (_CurrentIF.text.Equals("") && number == -3) return;
        switch (number)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                if (_CurrentIF.text.Length <= 15) _CurrentIF.text += number.ToString();
                break;
            case -1:
                if (_CurrentIF.text.Length > 0)
                {
                    string output = _CurrentIF.text.Remove(_CurrentIF.text.Length - 1);
                    _CurrentIF.text = output;
                }
                break;
            case -2:
                _CurrentIF.text = "";
                break;
            case -3:
                if (!_CurrentIF.text.Contains(".")) _CurrentIF.text += ".";
                break;
            default:
                break;
        }
    }
    public void DoClickCloseKeyboard()
    {
        _CurrentIF = null;
        m_Keyboard.SetActive(false);
        m_HighlightPhone.SetActive(false);
        m_HighlightConfirmPhone.SetActive(false);
        m_HighlightTonAmount.SetActive(false);
        m_TonBoardTf.DOLocalMoveX(0, .1f);
    }
    public void DoClickOpenKeyboardPhone()
    {
        _CurrentIF = m_PhoneIF;
        m_Keyboard.SetActive(true);
        m_HighlightPhone.SetActive(true);
        m_HighlightConfirmPhone.SetActive(false);
    }
    public void DoClickOpenKeyboardConfirmPhone()
    {
        _CurrentIF = m_ConfirmPhoneIF;
        m_Keyboard.SetActive(true);
        m_HighlightPhone.SetActive(false);
        m_HighlightConfirmPhone.SetActive(true);
    }
    public void DoClickOpenKeyboardTonAmount()
    {
        _CurrentIF = m_TonAmountIF;
        m_HighlightTonAmount.SetActive(true);
        m_TonBoardTf.DOLocalMoveX(-120f, .1f).SetEase(Ease.Linear).OnComplete(() => { m_Keyboard.SetActive(true); });
    }
    public void DoClickWithdrawTon()
    {
        // Config.TELEGRAM_WALLET_ADDRESS = "test";
        if (Config.TELEGRAM_WALLET_ADDRESS.Equals(""))
        {
            UIManager.instance.showMessageBox("Please add your Ton address.");
            return;
        }
        if (!float.TryParse(m_TonAmountIF.text, out float tonAmount) || tonAmount <= 0)
        {
            UIManager.instance.showMessageBox("Invalid amount of Ton!");
            return;
        }
        UIManager.instance.showMessageBox(
            "You will receive " + tonAmount + " Ton for " + Mathf.CeilToInt(tonAmount * 6000000) + " chips",
            () => SocketSend.SendWithdrawTon(tonAmount, Config.TELEGRAM_WALLET_ADDRESS),
            true
        );
    }
    public void onConfirmCashOut()
    {
        SoundManager.instance.soundClick();
        var value = valueCO;
        var typeName = typeNet;
        var phoneNumber = m_PhoneIF.text;
        var phoneNumberRetype = m_ConfirmPhoneIF.text;

        if (phoneNumber.Equals("") || phoneNumberRetype.Equals(""))
            UIManager.instance.showMessageBox(Config.formatStr(Config.getTextConfig("txt_notEmty"), typeNet.Equals("Mobile") ? Config.getTextConfig("txt_phone_numnber") : (string)rewardData["TypeName"], ""));
        else if (!phoneNumber.Equals(phoneNumberRetype))
            UIManager.instance.showMessageBox(Config.formatStr(Config.getTextConfig("txt_notSame"), typeNet.Equals("Mobile") ? Config.getTextConfig("txt_phone_numnber") : (string)rewardData["TypeName"]));
        else
        {
            m_PhoneIF.text = "";
            m_ConfirmPhoneIF.text = "";
            SocketSend.sendCashOut(value, phoneNumber, typeName);
            UIManager.instance.showWaiting();
        }
    }
    #endregion

    public void UpdateAg() { lbChips.text = Config.FormatNumber(User.userMain.AG); }
    public async void HandleGiftHistory(JObject data)
    {
        JArray content = (JArray)data["content"];
        foreach (Transform tf in m_HistoryTf) Destroy(tf.gameObject);
        for (int i = 0; i < content.Count; i++)
        {
            Transform tf = Instantiate(m_PrefabHistoryTf, m_HistoryTf);
            tf.gameObject.SetActive(true);
            tf.GetChild(0).GetComponent<TextMeshProUGUI>().text = DateTimeOffset.FromUnixTimeMilliseconds((long)content[i]["time"]).DateTime.ToString("dd/MM/yyyy hh:mm:ss tt");
            tf.GetChild(1).GetComponent<TextMeshProUGUI>().text = (string)content[i]["content"];
        }
        await ScrollHistory();
        async UniTask ScrollHistory()
        {
            try
            {
                await UniTask.Yield();
                await UniTask.Yield();
                _contentHeight = m_HistoryTf.GetComponent<RectTransform>().rect.height;
                float viewportheight = m_HistoryTf.parent.GetComponent<RectTransform>().rect.height;
                while (true)
                {
                    if (m_HistoryTf.localPosition.y > (_contentHeight - viewportheight)) m_HistoryTf.localPosition = Vector3.zero;
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                    m_HistoryTf.localPosition += Time.fixedDeltaTime * new Vector3(0, 100, 0);
                }
            }
            catch { }
        }
    }
    public void HandleUpdateHistory(JObject data)
    {
        Transform tf = Instantiate(m_PrefabHistoryTf, m_HistoryTf);
        tf.gameObject.SetActive(true);
        tf.GetChild(0).GetComponent<TextMeshProUGUI>().text = DateTimeOffset.FromUnixTimeMilliseconds((long)data["time"]).DateTime.ToString();
        tf.GetChild(1).GetComponent<TextMeshProUGUI>().text = (string)data["content"];
        _contentHeight += tf.GetComponent<RectTransform>().rect.height;
    }
    private void _UpdateInfo(string strData)
    {
        Debug.Log("exchange data:" + strData);
        dataCO = JArray.Parse(strData);
        if (dataCO.Count <= 0) return;
        _SetDataButtons();
    }
    private async void _SetDataButtons()
    {
        bool hasNormalType = false;
        Button rewardBtn = m_RewardTMP.transform.parent.GetComponent<Button>();
        foreach (JObject item in dataCO)
        {
            if (((string)item["title"]).Equals("reward"))
            { // type thường
                hasNormalType = true;
                m_RewardTMP.text = ((string)item["title"]).ToUpper();
                rewardBtn.onClick.AddListener(() => _DoClickAButton(rewardBtn.gameObject, item));
                if (!((string)item["type"]).Equals("agency"))
                {
                    m_HistoryTMP.text = Config.getTextConfig("history").ToUpper();
                    GameObject historyObj = m_HistoryTMP.transform.parent.gameObject;
                    historyObj.GetComponent<Button>().onClick.AddListener(() => _DoClickAButton(historyObj, null));
                }
                await _GenerateTabTop((JArray)item["child"], false);
            }
            else
            { // Ton
                m_TonRateTMP.text = "= " + Config.FormatNumber((int)item["rate"]);
                JArray tonDataJA = new();
                tonDataJA.Add(new JObject()
                {
                    ["title"] = "TON",
                    ["TypeName"] = "Ton_Coin", // phải giống với data trả về trong ["typeName"] của event "cashOutHistory"
                    ["title_img"] = item["title_img"]
                });
                await _GenerateTabTop(tonDataJA, true);
            }
        }
        if (!hasNormalType) rewardBtn.onClick.AddListener(() => _DoClickTabTon());
        rewardBtn.onClick.Invoke();
    }
    private void _DoClickTabTon()
    {
        SoundManager.instance.soundClick();
        scrContentRedeem.gameObject.SetActive(false);
        m_TonBoardTf.gameObject.SetActive(true);
        foreach (Transform tf in scrTabs.content) tf.Find("Bkg").gameObject.SetActive(tf.gameObject == _TabTonTf.gameObject);
    }
    private async Task _GenerateTabTop(JArray arrayData, bool isTon = false)
    {
        if (!isTon)
        {
            JObject item0 = null;
            var indSelect = 0;
            for (var i = 0; i < arrayData.Count; i++)
            {
                JObject obItem = (JObject)arrayData[i];
                if (i == 0) { item0 = obItem; indSelect = i; }
                string title = (string)obItem["TypeName"], title_img = (string)obItem["title_img"];
                GameObject btn = Instantiate(tabTop, scrTabs.content);
                var bkg = btn.transform.Find("Bkg").GetComponent<Image>();
                bkg.sprite = spTab[(i == 0 || i >= arrayData.Count - 1) ? 0 : 1];
                if (i >= arrayData.Count - 1)
                {
                    bkg.transform.localScale = new Vector3(-1, 1, 1);
                    btn.transform.Find("Line").gameObject.SetActive(false);
                }
                var txt = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                txt.text = "";
                var spLogo = btn.transform.Find("Icon").GetComponent<Image>();
                spLogo.gameObject.SetActive(false);
                if (title_img.Equals("")) txt.text = title.ToUpper();
                else
                {
                    Sprite spr = await Config.GetRemoteSprite(title_img);
                    if (spr != null)
                    {
                        spLogo.sprite = spr;
                        if (spLogo != null && spLogo.sprite != null)
                        {
                            spLogo.gameObject.SetActive(true);
                            spLogo.SetNativeSize();
                        }
                        else txt.text = title.ToUpper();
                    }
                }
                btn.transform.localScale = Vector3.one;
                btn.GetComponent<Button>().onClick.AddListener(() => { _OnClickTabTopNotTon(btn.gameObject, obItem); });
            }
            if (item0 == null && arrayData.Count > 0)
            {
                indSelect = 0;
                item0 = (JObject)arrayData[0];
            }
            if (scrTabs.content.childCount > indSelect)
            {
                Logging.Log("item   " + item0.ToString());
                _OnClickTabTopNotTon(scrTabs.content.GetChild(indSelect).gameObject, item0);
                curDataTabNap = item0;
            }
        }
        else
        {
            _TabTonTf = Instantiate(tabTop, scrTabs.content).transform;
            _TabTonTf.localScale = Vector3.one;
            _TabTonTf.GetComponent<Button>().onClick.AddListener(() => _DoClickTabTon());
            TextMeshProUGUI titleTMP = _TabTonTf.Find("Text").GetComponent<TextMeshProUGUI>();
            titleTMP.text = "TON";
            titleTMP.gameObject.SetActive(false);
            Sprite logoS = await Config.GetRemoteSprite((string)arrayData[0]["title_img"]);
            if (logoS != null)
            {
                Image tabImg = _TabTonTf.Find("Icon").GetComponent<Image>();
                tabImg.sprite = logoS;
                tabImg.SetNativeSize();
                tabImg.gameObject.SetActive(true);
            }
            else titleTMP.gameObject.SetActive(true);
        }
        _GenerateTabTopHistory(arrayData, isTon);
        scrTabs.enabled = scrTabs.content.childCount > 4;
    }
    private async void _GenerateTabTopHistory(JArray arrayData, bool isTon = false)
    {
        if (!isTon)
        {
            JObject item0 = null;
            indexTabHis = 0;
            for (var i = 0; i < arrayData.Count; i++)
            {
                JObject obItem = (JObject)arrayData[i];
                if (i == 0) { item0 = obItem; indexTabHis = i; }
                string title = (string)obItem["TypeName"], title_img = (string)obItem["title_img"];
                GameObject btn = Instantiate(tabTop, scrTabsHis.content);
                var bkg = btn.transform.Find("Bkg").GetComponent<Image>();
                bkg.sprite = spTab[(i == 0 || i >= arrayData.Count - 1) ? 0 : 1];
                if (i >= arrayData.Count - 1)
                {
                    bkg.transform.localScale = new Vector3(-1, 1, 1);
                    btn.transform.Find("Line").gameObject.SetActive(false);
                }
                var txt = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                txt.text = "";
                var spLogo = btn.transform.Find("Icon").GetComponent<Image>();
                spLogo.gameObject.SetActive(false);
                if (title_img.Equals("")) txt.text = title.ToUpper();
                else
                {
                    Sprite spr = await Config.GetRemoteSprite(title_img);
                    if (spr != null)
                    {
                        spLogo.sprite = spr;
                        if (spLogo != null && spLogo.sprite != null)
                        {
                            spLogo.gameObject.SetActive(true);
                            spLogo.SetNativeSize();
                        }
                        else txt.text = title.ToUpper();
                    }
                }
                btn.transform.localScale = Vector3.one;
                btn.GetComponent<Button>().onClick.AddListener(() => { _OnClickTabHistory(btn.gameObject, obItem); });
                if (typeTabHistory == (string)obItem["TypeName"])
                {
                    firstTabHistItem = obItem;
                    indexTabHis = i;
                }
            }
        }
        else
        {
            _TabHistoryTonTf = Instantiate(tabTop, scrTabsHis.content).transform;
            _TabHistoryTonTf.localScale = Vector3.one;
            _TabHistoryTonTf.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.instance.soundClick();
                foreach (Transform tf in scrTabsHis.content) tf.Find("Bkg").gameObject.SetActive(tf.gameObject == _TabHistoryTonTf.gameObject);
                typeTabHistory = (string)arrayData[0]["TypeName"];
                if (listDataHis.Count > 0) reloadListItemHistory(listDataHis);
            });
            TextMeshProUGUI titleTMP = _TabHistoryTonTf.Find("Text").GetComponent<TextMeshProUGUI>();
            titleTMP.text = "TON";
            titleTMP.gameObject.SetActive(false);
            Sprite logoS = await Config.GetRemoteSprite((string)arrayData[0]["title_img"]);
            if (logoS != null)
            {
                Image tabImg = _TabHistoryTonTf.Find("Icon").GetComponent<Image>();
                tabImg.sprite = logoS;
                tabImg.SetNativeSize();
                tabImg.gameObject.SetActive(true);
            }
            else titleTMP.gameObject.SetActive(true);
        }
        scrTabsHis.enabled = scrTabsHis.content.childCount > 4;
    }
    private void _OnClickTabHistory(GameObject tab, JObject dataItem)
    {
        SoundManager.instance.soundClick();
        for (var i = 0; i < scrTabsHis.content.childCount; i++)
        {
            var bkg = scrTabsHis.content.GetChild(i).transform.Find("Bkg");
            bkg.gameObject.SetActive(tab == scrTabsHis.content.GetChild(i).gameObject);
            if (tab == scrTabsHis.content.GetChild(i).gameObject) indexTabNap = i;
        }
        if (dataItem["TypeName"] != null) typeTabHistory = (string)dataItem["TypeName"];
        else
        {
            JArray tabNamesJA = (JArray)dataItem["child"];
            typeTabHistory = (string)tabNamesJA[indexTabNap]["TypeName"];
        }
        curDataTabNap = dataItem;
        if (listDataHis.Count > 0) reloadListItemHistory(listDataHis);
    }
    private void _OnClickTabTopNotTon(GameObject evv, JObject dataItem)
    {
        SoundManager.instance.soundClick();
        rewardData = dataItem;
        for (var i = 0; i < scrTabs.content.childCount; i++)
        {
            var bkg = scrTabs.content.GetChild(i).transform.Find("Bkg");
            bkg.gameObject.SetActive(evv == scrTabs.content.GetChild(i).gameObject);
            if (evv == scrTabs.content.GetChild(i).gameObject)
            {
                indexTabHis = i;
                indexTabNap = i;
            }
        }
        typeTabHistory = (string)dataItem["TypeName"];
        firstTabHistItem = dataItem;
        _ReloadListNotTonItems(rewardData);
    }
    private void _DoClickAButton(GameObject obj, JObject objDataItem)
    {
        SoundManager.instance.soundClick();
        GameObject rewardGo = m_RewardTMP.transform.parent.gameObject;
        GameObject historyGo = m_HistoryTMP.transform.parent.gameObject;
        rewardGo.SetActive(obj != rewardGo);
        historyGo.SetActive(obj != historyGo);
        if (objDataItem == null && obj == historyGo)
        {
            scrContentRedeem.transform.parent.gameObject.SetActive(false);
            scrContentAgency.transform.parent.gameObject.SetActive(false);
            scrContentHistory.transform.parent.gameObject.SetActive(true);
            _OnClickTabHistory(scrTabsHis.content.GetChild(indexTabHis).gameObject, firstTabHistItem);
            SocketSend.sendDTHistory();
        }
        else if (((string)objDataItem["type"]).Equals("agency"))
        {
            typeNet = (string)objDataItem["type"];
            scrContentRedeem.transform.parent.gameObject.SetActive(false);
            scrContentAgency.transform.parent.gameObject.SetActive(true);
            scrContentHistory.transform.parent.gameObject.SetActive(false);
            _ReloadListNotTonItems(objDataItem);
        }
        else
        {
            typeNet = (string)curDataTabNap["TypeName"];
            scrContentRedeem.transform.parent.gameObject.SetActive(true);
            scrContentAgency.transform.parent.gameObject.SetActive(false);
            scrContentHistory.transform.parent.gameObject.SetActive(false);
            if (indexTabNap != -1) _OnClickTabTopNotTon(scrTabs.content.GetChild(indexTabNap).gameObject, objDataItem);
        }
    }
    private void _ReloadListNotTonItems(JObject objDataItem)
    {
        scrContentRedeem.gameObject.SetActive(true);
        m_TonBoardTf.gameObject.SetActive(false);
        if (objDataItem != null)
        {
            JArray items = new JArray(); ;
            Transform parent;
            if (objDataItem["TypeName"] != null) typeNet = (string)objDataItem["TypeName"];
            else
            {
                JArray tabNamesJA = (JArray)objDataItem["child"];
                typeNet = (string)tabNamesJA[indexTabNap]["TypeName"];
            }
            bool isAgency = objDataItem.ContainsKey("type") && ((string)objDataItem["type"]).Equals("agency");
            items = (JArray)objDataItem["items"];
            parent = isAgency ? scrContentAgency.content : scrContentRedeem.content;
            if (items == null || items.Count <= 0) return;
            for (var i = 0; i < items.Count; i++)
            {
                JObject dt = (JObject)items[i];
                GameObject item = i < parent.childCount ? parent.GetChild(i).gameObject : Instantiate(isAgency ? itemAgency : itemEx, parent);
                if (isAgency) item.GetComponent<ItemAgency>().setInfo(dt);
                else item.GetComponent<ItemEx>().setInfo(dt, () => _OnClickCashOut((int)dt["ag"], (int)dt["m"]));
                item.SetActive(true);
                item.transform.SetParent(parent);
                item.transform.localScale = Vector3.one;
            }
            for (var i = items.Count; i < parent.childCount; i++) parent.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void reloadListItemHistory(List<JObject> listItem)
    {
        listDataHis = listItem;
        for (int i = 0; i < scrContentHistory.content.childCount; i++) scrContentHistory.content.GetChild(i).gameObject.SetActive(false);
        for (var i = 0; i < listDataHis.Count; i++)
        {
            string typeNameItem = (string)listDataHis[i]["typeName"];
            if (typeNameItem.Equals(typeTabHistory))
            {
                GameObject objItem;
                if (i < scrContentHistory.content.childCount) objItem = scrContentHistory.content.GetChild(i).gameObject;
                else objItem = Instantiate(itemHistory, scrContentHistory.content);
                objItem.SetActive(true);
                objItem.transform.SetParent(scrContentHistory.content);
                objItem.transform.localScale = Vector3.one;
                objItem.GetComponent<ItemHistoryEx>().setInfo(listDataHis[i]);
            }
        }
    }
    private void _OnClickCashOut(int ag, int value)
    {
        SoundManager.instance.soundClick();
        if (User.userMain.AG < ag) UIManager.instance.showMessageBox(Config.getTextConfig("txt_koduchip"));
        else
        {
            popupInput.show();
            if (rewardData != null)
            {
                JArray textBox = null;
                if (rewardData["textBox"] != null) textBox = (JArray)rewardData["textBox"];
                else textBox = (JArray)rewardData["child"][indexTabNap]["textBox"];
                m_PhoneIF.placeholder.GetComponent<Text>().text = Config.getTextConfig((string)textBox[0]["key_placeHolder"]);
                m_ConfirmPhoneIF.placeholder.GetComponent<Text>().text = Config.getTextConfig((string)textBox[1]["key_placeHolder"]);
            }
        }
        valueCO = value;
    }
    public void cashOutReturn(JObject data)
    {
        Logging.Log("-=-=-=-=cashOutReturn  " + data.ToString());
        UIManager.instance.showMessageBox((string)data["data"]);
        if ((bool)data["status"])
        {
            SocketSend.sendUAG();
            if (Config.TELEGRAM_TOKEN.Equals(""))
            {
                m_PhoneIF.text = "";
                m_ConfirmPhoneIF.text = "";
                popupInput.hide(false);
                _DoClickAButton(m_HistoryTMP.transform.parent.gameObject, null);
            }
        }
    }
    protected override void Start()
    {
        base.Start();
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickShowExchange_%s", CURRENT_VIEW.getCurrentSceneName()));
        CURRENT_VIEW.setCurView(CURRENT_VIEW.DT_VIEW);
        LoadConfig.instance.getInfoEX(_UpdateInfo);
        lbChips.text = Config.FormatNumber(User.userMain.AG);
        m_Keyboard.SetActive(false);
        bool isTelegram = !Config.TELEGRAM_TOKEN.Equals("");
        m_OpenKeyboardPhone.SetActive(isTelegram);
        m_OpenKeyBoardConfirmPhone.SetActive(isTelegram);
        bool hasTonWalletAdress = !Config.TELEGRAM_WALLET_ADDRESS.Equals("");
        if (hasTonWalletAdress) m_TonWalletTMP.text = Config.TELEGRAM_WALLET_ADDRESS;
        m_WithdrawBtn.interactable = hasTonWalletAdress;
    }
    protected override void Awake()
    {
        base.Awake();
        instance = this;
        SocketSend.SendGiftsHistory();
    }
}
