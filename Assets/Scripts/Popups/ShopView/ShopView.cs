using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Globals;
public class ShopView : BaseView
{
    public static ShopView instance = null;
    [SerializeField] private List<Sprite> spTab;
    [SerializeField] private List<TMP_InputField> listEdb;
    [SerializeField] private ScrollRect scrTabsChannel, scrTabs, scrContent;
    [SerializeField] private GameObject btnTab, itemShop, m_APKContent, m_ContentOnTelegram;
    [SerializeField] private TextMeshProUGUI txt_best, m_BestDealChipsTMPUI, m_TONAddressTMPUI, m_MemoTMPUI;
    [SerializeField] private BaseView inputView;
    [SerializeField] private Button btnConfirmInput;
    [SerializeField] private RawImage m_QRCodeRI;
    private JObject itemBest = null;
    private IAPManager iapManager = null;
    private bool isTab = false;
    private string dataDefault = "[{\"type\":\"iap\",\"title\":\"iap\",\"focus\":false,\"title_img\":\"https://cdn.jakartagames.net/api/public/dl/sdfs45d/img/shop/IAPAND.png?inline=true\",\"items\":[{\"url\":\"diamond.domino.slots.1\",\"txtPromo\":\"1USD = 7,576 Chips\",\"txtChip\":\"7,500 Chips\",\"txtBuy\":\"0.990000 USD\",\"txtBonus\":\"0%\",\"cost\":1},{\"url\":\"diamond.domino.slots.2\",\"txtPromo\":\"1USD = 7,538 Chips\",\"txtChip\":\"15,000 Chips\",\"txtBuy\":\"1.990000 USD\",\"txtBonus\":\"0%\",\"cost\":2},{\"url\":\"diamond.domino.slots.5\",\"txtPromo\":\"1USD = 7,515 Chips\",\"txtChip\":\"37,500 Chips\",\"txtBuy\":\"4.990000 USD\",\"txtBonus\":\"0%\",\"cost\":5},{\"url\":\"diamond.domino.slots.10\",\"txtPromo\":\"1USD = 9,009 Chips\",\"txtChip\":\"90,000 Chips\",\"txtBuy\":\"9.990000 USD\",\"txtBonus\":\"0%\",\"cost\":10},{\"url\":\"diamond.domino.slots.20\",\"txtPromo\":\"1USD = 9,005 Chips\",\"txtChip\":\"180,000 Chips\",\"txtBuy\":\"19.990000 USD\",\"txtBonus\":\"0%\",\"cost\":20},{\"url\":\"diamond.domino.slots.50\",\"txtPromo\":\"1USD = 9,002 Chips\",\"txtChip\":\"450,000 Chips\",\"txtBuy\":\"49.990000 USD\",\"txtBonus\":\"0%\",\"cost\":50},{\"url\":\"diamond.domino.slots.100\",\"txtPromo\":\"1USD = 9,001 Chips\",\"txtChip\":\"900,000 Chips\",\"txtBuy\":\"99.990000 USD\",\"txtBonus\":\"0%\",\"cost\":100}]}]";

    public void init()
    {

        if (UIManager.instance.gameView == null)
        {
            CURRENT_VIEW.setCurView(CURRENT_VIEW.PAYMENT);
            SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickShop_%s", CURRENT_VIEW.getCurrentSceneName()));
        }
        instance = this;
        if (Config.infoChip != "")
        {
            LoadConfig.instance.getInfoShop(updateInfo, () =>
            {
                updateInfo(dataDefault);
            });
        }
        else
        {
            updateInfo(dataDefault);
        }
        scrContent.onValueChanged.AddListener(onDragScroll);
    }
    public void DoClickCopyWalletAddress()
    {
        string[] results = m_TONAddressTMPUI.text.Split(" ");
        TextEditor te = new();
        te.text = results.Last();
        te.SelectAll();
        te.Copy();
        UIManager.instance.showToast("Copied");
    }
    public void DoClickCopyMemo()
    {
        string[] results = m_MemoTMPUI.text.Split(" ");
        TextEditor te = new();
        te.text = results.Last();
        te.SelectAll();
        te.Copy();
        UIManager.instance.showToast("Copied");

    }
    async void updateInfo(string strData)
    {
        Logging.Log("updateInfo shop   " + strData + " / " + Config.TELEGRAM_TOKEN);
        bool isTelegram = !Config.TELEGRAM_TOKEN.Equals("");
        JArray arrayData = JArray.Parse(strData);
        //ton
        JToken tonData = null;
        foreach (JToken data in arrayData)
        {
            if (((string)data["title"]).ToLower().Equals("ton"))
            {
                tonData = data;
                break;
            }
        }
        if (tonData != null)
        {
            string paymentUrl = (string)tonData["linkpayment"];
            string[] results = paymentUrl.Split("/").Last().Split("?text=");
            m_TONAddressTMPUI.text = "Wallet address: " + results[0];
            m_MemoTMPUI.text = "Memo: " + results[1];
            Color32[] outputC32 = EncodeTextToQRCode(paymentUrl, 256, 256);
            Texture2D t2D = new(256, 256);
            t2D.SetPixels32(outputC32);
            t2D.Apply();
            m_QRCodeRI.texture = t2D;
        }
        //other
        if (arrayData.Count > 0)
        {
            UIManager.instance.destroyAllChildren(scrTabs.content.transform);
            UIManager.instance.destroyAllChildren(scrContent.content.transform);
            if (arrayData.Count == 1 && !arrayData[0]["title"].Equals("ton"))
            {
                iapManager = new IAPManager((JObject)arrayData[0]);
                scrTabs.gameObject.SetActive(false);
                reloadListContent((JArray)arrayData[0]["items"], (string)arrayData[0]["type"], (string)arrayData[0]["title"]);
                RectTransform rectTransform = scrContent.viewport.GetComponent<RectTransform>();
                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);
                return;
            }
            scrTabs.gameObject.SetActive(true);
            JObject item0 = null;
            int chosenId = 0;
            for (int i = 0; i < arrayData.Count; i++)
            {
                JObject obItem = (JObject)arrayData[i];
                if (obItem.ContainsKey("focus") && (bool)obItem["focus"])
                {
                    chosenId = i;
                    item0 = obItem;
                }
                string title = (string)obItem["title"], title_img = (string)obItem["title_img"];
                if (title.Equals("iap") && iapManager == null) iapManager = new IAPManager(obItem);
                GameObject btn = Instantiate(btnTab, scrTabs.content);
                if (Screen.width < Screen.height) btn.transform.localRotation = Quaternion.Euler(btn.transform.localRotation.x, btn.transform.localRotation.y, 0);
                Image bkg = btn.transform.Find("Bkg").GetComponent<Image>();
                bkg.transform.localScale = Vector3.one;
                if (i >= arrayData.Count - 1)
                {
                    bkg.transform.localScale = new Vector3(-1, 1, 1);
                    btn.transform.Find("Line").gameObject.SetActive(false);
                }
                TextMeshProUGUI titleTMP = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                titleTMP.text = "";
                Image logoImg = btn.transform.Find("Icon").GetComponent<Image>();
                logoImg.gameObject.SetActive(false);
                if (title_img.Equals("")) titleTMP.text = title.ToUpper();
                else
                {
                    Sprite spr = await Config.GetRemoteSprite(title_img);
                    if (spr != null)
                    {
                        logoImg.sprite = spr;
                        if (logoImg != null && logoImg.sprite != null)
                        {
                            logoImg.gameObject.SetActive(true);
                            logoImg.SetNativeSize();
                        }
                        else titleTMP.text = title.ToUpper();
                    }
                }
                btn.transform.localScale = Vector3.one;
                btn.transform.position = new Vector3(btn.transform.position.x, 0);
                btn.GetComponent<Button>().onClick.AddListener(() => { onClickTab(btn.gameObject, obItem); });
            }
            float contentWidth = 0, tabWidth = btnTab.GetComponent<RectTransform>().rect.width;
            foreach (Transform childTf in scrTabs.content) contentWidth += tabWidth;
            scrTabs.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
            if (item0 == null && arrayData.Count > 0)
            {
                chosenId = 0;
                item0 = (JObject)arrayData[0];
            }
            if (scrTabs.content.childCount > chosenId) onClickTab(scrTabs.content.transform.GetChild(chosenId).gameObject, item0);
        }

    }
    private Color32[] EncodeTextToQRCode(string inputText, int width, int height)
    {
        // BarcodeWriter writer = new()
        // {
        //     Format = BarcodeFormat.QR_CODE,
        //     Options = new() { Height = height, Width = width }
        // };
        // return writer.Write(inputText);
        return new Color32[2];
    }

    void getBest(JArray items, string partner, string title)
    {
        //console.log('-=-= title   ', title)
        var vip = User.userMain.VIP;
        var priceBest = 0.0f;
        switch (title.ToUpper().Trim())
        {
            case "XL":
                {
                    if (vip <= 1) priceBest = 20000;
                    else if (vip <= 4) priceBest = 50000;
                    else if (vip <= 7) priceBest = 70000;
                    else priceBest = 100000;
                    break;
                }
            case "TELKOMSEL":
                {
                    if (vip <= 1) priceBest = 25000;
                    else if (vip <= 7) priceBest = 50000;
                    else priceBest = 100000;
                    break;
                }
            case "HUTCHISON":
            case "H3I":
                {
                    if (vip <= 1) priceBest = 10000;
                    else if (vip <= 7) priceBest = 30000;
                    else priceBest = 100000;
                    break;
                }
            case "OVO":
            case "DANA":
            case "GOPAY":
            case "LINKAJA":
                {
                    if (vip <= 1) priceBest = 20000;
                    else if (vip <= 3) priceBest = 50000;
                    else if (vip <= 5) priceBest = 100000;
                    else if (vip <= 7) priceBest = 200000;
                    else if (vip <= 9) priceBest = 500000;
                    else priceBest = 1000000;
                    break;
                }

            case "OVO SPECIAL":
            case "DANA SPECIAL":
            case "GOPAY SPECIAL":
            case "LINKAJA SPECIAL":
                {
                    if (vip <= 1) priceBest = 10000;
                    else if (vip <= 3) priceBest = 50000;
                    else if (vip <= 5) priceBest = 100000;
                    else if (vip <= 7) priceBest = 200000;
                    else if (vip <= 9) priceBest = 500000;
                    else priceBest = 1000000;
                    break;
                }

            case "IAP":
                {
                    if (vip <= 1) priceBest = 1.99f;
                    else if (vip <= 3) priceBest = 4.99f;
                    else if (vip == 4) priceBest = 9.99f;
                    else if (vip <= 6) priceBest = 19.99f;
                    else if (vip <= 8) priceBest = 49.99f;
                    else priceBest = 99.99f;
                    break;
                }
        }
        JObject itemData = null;
        var itemData2 = items.FirstOrDefault(it => Config.convertStringToNumber((string)it["txtBuy"]) == priceBest);
        //console.log('-=-= data best   ', itemData)
        if (itemData2 == null)
        {
            itemData = (JObject)items[0];
        }
        else
        {
            itemData = (JObject)itemData2;
        }

        // this.lb_best.string = GameManager.getInstance().formatNumber(this.convertStringToNumber(itemData.txtBuy));

        var txtBuy = (string)itemData["txtBuy"];
        m_BestDealChipsTMPUI.SetText((string)itemData["txtChip"]);
        if (txtBuy.Contains("USD"))
        {
            //this.txt_best.node.parent.getChildByName("P").active = false;
            this.txt_best.text = Config.convertStringToNumber(txtBuy).ToString().Replace(",", ".") + "$";
        }
        else
        {
            //this.txt_best.node.parent.getChildByName("P").active = true;
            this.txt_best.text = txtBuy;//Config.FormatNumber(this.convertStringToNumber(txtBuy));
        }
        itemData["partner"] = partner;
        this.itemBest = itemData;
    }



    public void updateAg()
    {

    }
    void onClickTab(GameObject tab, JObject dataItem)
    {
        Debug.Log("onClickTab  " + dataItem.ToString());
        SoundManager.instance.soundClick();
        bool isTonTab = ((string)dataItem["title"]).Equals("ton");
        m_APKContent.SetActive(!isTonTab);
        m_ContentOnTelegram.SetActive(isTonTab);
        for (var i = 0; i < scrTabs.content.childCount; i++)
        {
            Transform thisTf = scrTabs.content.GetChild(i);
            bool isTab = tab == thisTf.gameObject;
            thisTf.Find("Bkg").gameObject.SetActive(isTab);
            if (!isTonTab && isTab) handleInfoItem(dataItem);
        }
        //reloadListContent(dataItem);
    }

    async void handleInfoItem(JObject data)
    {
        Debug.Log("handleInfoItem   " + data.ToString());
        RectTransform rectTransform = scrContent.transform.Find("Viewport").GetComponent<RectTransform>();
        if (data.ContainsKey("child"))
        {
            scrTabsChannel.gameObject.SetActive(true);
            JArray child = (JArray)data["child"];
            JObject dataSuget = null;
            int suggestedId = 0, suggestedIdFromBanner = -1;
            if (child.Count == 1)
            {
                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -10);
                scrTabsChannel.gameObject.SetActive(false);
                reloadListContent((JArray)child[0]["items"], (string)child[0]["type"], (string)child[0]["title"]);
                return;
            }
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -65);
            for (var i = 0; i < child.Count; i++)
            {
                JObject dataChild = (JObject)child[i];
                string title = (string)dataChild["title"], title_img = (string)dataChild["title_img"];
                GameObject itemTab;// = this.scv_tab_channel.content.children[i]
                if (i < scrTabsChannel.content.childCount) itemTab = scrTabsChannel.content.GetChild(i).gameObject;
                else itemTab = Instantiate(scrTabsChannel.content.GetChild(0).gameObject, scrTabsChannel.content.transform);
                itemTab.SetActive(true);
                Image bkg = itemTab.transform.Find("Bkg").GetComponent<Image>();
                bkg.transform.localScale = new Vector3(1, 1, 1);
                if (i >= child.Count - 1)
                {
                    bkg.transform.localScale = new Vector3(-1, 1, 1);
                    itemTab.transform.Find("Line").gameObject.SetActive(false);
                }
                TextMeshProUGUI txt = itemTab.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                txt.text = "";
                Image spLogo = itemTab.transform.Find("Icon").GetComponent<Image>();
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
                itemTab.transform.localScale = Vector3.one;
                itemTab.transform.localPosition = new Vector3(itemTab.transform.localPosition.x, 0);
                itemTab.GetComponent<Button>().onClick.AddListener(() => { onClickTabChannel(itemTab.gameObject, dataChild); });
                if (dataChild.ContainsKey("focus") && ((bool)dataChild["focus"]))
                {
                    suggestedId = i;
                    dataSuget = dataChild;
                }
            }
            if (suggestedIdFromBanner < 0) suggestedIdFromBanner = suggestedId;
            for (var i = child.Count; i < scrTabsChannel.content.childCount; i++)
                scrTabsChannel.content.GetChild(i).gameObject.SetActive(false);
            if (dataSuget == null && child.Count > 0)
            {
                suggestedId = 0;
                dataSuget = (JObject)child[0];
            }
            if (scrTabsChannel.content.childCount > suggestedId)
                onClickTabChannel(scrTabsChannel.content.GetChild(suggestedId).gameObject, dataSuget);
        }
        else
        {
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -10);
            scrTabsChannel.gameObject.SetActive(false);
            reloadListContent((JArray)data["items"], (string)data["type"], (string)data["title"]);
        }
    }
    void onClickTabChannel(GameObject evv, JObject dataItem)
    {
        SoundManager.instance.soundClick();
        for (var i = 0; i < scrTabsChannel.content.childCount; i++)
        {
            var bkg = scrTabsChannel.content.GetChild(i).transform.Find("Bkg");
            bkg.gameObject.SetActive(evv == scrTabsChannel.content.GetChild(i).gameObject);
            if (evv == scrTabsChannel.content.GetChild(i).gameObject)
            {
                reloadListContent((JArray)dataItem["items"], (string)dataItem["type"], (string)dataItem["title"]);
            }
        }

        //reloadListContent(dataItem);
    }


    void reloadListContent(JArray listItem, string partner, string title)
    {
        itemBest = null;
        getBest(listItem, partner, title);
        for (var i = 0; i < listItem.Count; i++)
        {
            JObject dtItem = (JObject)listItem[i];
            dtItem["type"] = partner;

            GameObject itemS = null;
            if (i < scrContent.content.childCount)
            {
                itemS = scrContent.content.GetChild(i).gameObject;
            }
            else
            {
                itemS = Instantiate(itemShop, scrContent.content);
                // if (Screen.width < Screen.height) itemS.transform.localRotation = Quaternion.Euler(itemS.transform.localRotation.x, itemS.transform.localRotation.y, -90);
            }

            itemS.SetActive(true);
            itemS.transform.SetParent(scrContent.content);
            itemS.transform.SetSiblingIndex(i);
            itemS.transform.localScale = Vector3.one;
            itemS.GetComponent<ItemShop>().setInfo(dtItem, i, () =>
            {
                onBuy(dtItem);
            }, i >= listItem.Count - 1);
        }
        for (var i = listItem.Count; i < scrContent.content.childCount; i++)
        {
            scrContent.content.GetChild(i).gameObject.SetActive(false);
        }

    }
    public void onClickBest()
    {
        onBuy(itemBest);
    }
    public void onBuy(JObject dataItem)
    {
        Logging.Log("onBuy  " + Config.formatStr("ClickPrice_%s", CURRENT_VIEW.getCurrentSceneName()));
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickPrice_%s", CURRENT_VIEW.getCurrentSceneName()));
        Logging.Log("ShopView: Data Item= " + dataItem);
        var url = (string)dataItem["url"];
        url.Replace("%uid%", User.userMain.Userid.ToString());
        if (!Config.TELEGRAM_TOKEN.Equals(""))
        { // luồng chơi tele thì auto mở web
            Application.OpenURL(url);
            return;
        }

        switch ((string)dataItem["type"])
        {
            case CMD.W_DEFAULT:
                {
                    //open webview
                    //require("Util").onCallWebView(data.url);
                    UIManager.instance.showWebView(url);
                    break;
                }
            case CMD.W_REPLACE:
                {
                    //show input, replace in textbox, open webview
                    onShowInput(dataItem);
                    break;
                }
            case CMD.U_DEFAULT:
                {
                    //cc.sys.openURL(data.url);
                    Application.OpenURL(url);
                    break;
                }
            case CMD.U_REPLACE:
                {
                    ////show input, replace in textbox open url

                    onShowInput(dataItem);
                    break;
                }
            case CMD.IAP:
                {
                    //require("Util").onBuyIap(data.url);
                    Debug.Log("-=-= buy iapp 0");
                    if (iapManager != null)
                    {
                        Debug.Log("-=-= buy iapp 1");
                        iapManager.buyIAP(url);
                    }
                    break;
                }
        }
    }

    void onShowInput(JObject dataItem)
    {

        inputView.show();
        var url = (string)dataItem["url"];
        var lsTextBox = (JArray)dataItem["textBox"];
        for (var i = 0; i < listEdb.Count; i++)
        {
            if (i < lsTextBox.Count)
            {
                listEdb[i].gameObject.SetActive(true);
                listEdb[i].placeholder.GetComponent<TextMeshProUGUI>().text = (string)lsTextBox[i]["placeHolder"];
            }
            else
            {
                listEdb[i].gameObject.SetActive(false);

            }
        }

        btnConfirmInput.onClick.RemoveAllListeners();
        btnConfirmInput.onClick.AddListener(() =>
        {
            var canSend = true;
            for (var i = 0; i < lsTextBox.Count; i++)
            {
                var str = listEdb[i].text;
                var key = (string)lsTextBox["key"];
                url = url.Replace(key, str);
                if (str == "") canSend = false;
            }
            if (canSend)
            {
                UIManager.instance.showWebView(url);
                Logging.Log("URL====" + url);
                inputView.hide(false);
            }
        });
    }
    //public void onDragScroll() { }
    public void onDragScroll(Vector2 value)
    {
        if (!isTab) return;
        if (scrContent.content.childCount <= 0) return;
        UIManager.instance.updateBotWithScrollShop(value);
    }
    public override void OnDestroy()
    {
        instance = null;
        base.OnDestroy();
        if (UIManager.instance.gameView == null)
        {
            CURRENT_VIEW.setCurView(CURRENT_VIEW.GAMELIST_VIEW);
        }

    }

}
