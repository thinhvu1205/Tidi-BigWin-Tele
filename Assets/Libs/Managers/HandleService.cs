using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using Globals;
using Socket.Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class HandleService
{
    public static async Task processData(JObject jsonData)
    {

        SocketIOManager.getInstance().emitSIOWithValue(jsonData, "ServiceTransportPacket", false);
        if (jsonData.ContainsKey("evt"))
        {
            string evt = (string)jsonData["evt"];
            Logging.Log("--------------------------------------------------->EVT: " + evt + " <------------------------------------------->\n" + jsonData);
            switch (evt)
            {
                case "promotion_info":
                    Promotion.setPromotionInfo(jsonData);
                    UIManager.instance.setTimeOnline();
                    break;
                case "promotion_online":

                    break;
                case "reconnect":
                    User.userMain.lastGameID = (int)jsonData["gameid"];
                    // Config.curGameId = (int)jsonData["gameid"];
                    break;
                case "promotion":
                    break;
                case "addInviteFriendID":
                    bool isSuccess = (bool)jsonData["isSuccess"];
                    UIManager.instance.hideWatting();
                    if (isSuccess)
                    {
                        User.userMain.canInputInvite = false;
                        UIManager.instance.updateCanInviteFriend();
                        UIManager.instance.showToast((string)jsonData["data"]);
                    }
                    else
                    {
                        UIManager.instance.showToast((string)jsonData["data"]);
                    }
                    break;
                case "giftDetail":
                    break;
                case "historyGiftfDetail":
                    break;
                case "dp":
                    {
                        var agDp = (long)jsonData["AG"];
                        SocketIOManager.getInstance().emitUpdateInfo();
                        break;
                    }
                case "GiftCode":
                    {
                        int gold = (int)jsonData["G"];
                        string msg = (string)jsonData["Msg"];
                        if (gold > 0)
                        {
                            User.userMain.AG += gold;
                            System.Action cb1 = () =>
                            {
                            };
                            SocketSend.sendPromotion();
                            UIManager.instance.showDialog(msg, Config.getTextConfig("txt_free_chip"), cb1, Config.getTextConfig("ok"));
                        }
                        else
                        {
                            UIManager.instance.showMessageBox(msg);
                        }

                        break;
                    }

                case "toprich":

                    break;
                case "followlist":
                    break;
                case "follow":
                    break;
                case "10":
                    {
                        if (jsonData.ContainsKey("data"))
                        {
                            UIManager.instance.showMessageBox((string)jsonData["data"]);
                        }
                        else
                        {
                            //UIManager.instance.showMessageBox((string)jsonData["Cmd"]);
                            UIManager.instance.showDialog((string)jsonData["Cmd"], Config.getTextConfig("ok"), () =>
                            {
                            });
                        }
                        break;
                    }
                case "changea":
                    int status = (int)jsonData["error"];
                    Logging.Log("status avatar:" + status);

                    if (status == 0)
                    {
                        UIManager.instance.updateAvatar();
                    }
                    break;
                case "promotion_online_2":
                    if (!jsonData.ContainsKey("result") || ((string)jsonData["result"]).Equals("")) break;


                    break;
                case "promotion_daily_2":

                    break;
                case "20":


                    User.userMain.listMailAdmin.Clear();
                    JArray listMail = JArray.Parse((string)jsonData["data"]);
                    for (int i = 0, l = listMail.Count; i < l; i++)
                    {
                        JObject data = new JObject((JObject)listMail[i]);
                        data["S"] = (bool)data["S"] ? 1 : 0;
                        User.userMain.listMailAdmin.Add(data);
                    }
                    User.userMain.listMailAdmin.Sort((a, b) =>
                    {
                        if ((int)a["S"] != (int)b["S"])
                        {
                            return (int)a["S"] - (int)b["S"];
                        }
                        else
                        {
                            if ((long)a["Time"] > (long)b["Time"]) return -1;
                            else return 1;
                        }
                    });
                    User.userMain.mailUnRead = User.userMain.listMailAdmin.FindAll(data => (int)data["S"] == 0).Count;

                    break;
                case "22":
                    JArray listFreeChip = JArray.Parse((string)jsonData["data"]);
                    foreach (JObject data in listFreeChip)
                    {
                        JObject item = new JObject();
                        if (data["DT"] != null)
                        {
                            item["moneyType"] = data["DT"];
                            item["idMsg"] = data["Id"];
                            item["t"] = data["T"];
                            item["vip"] = data["Vip"];
                            item["from"] = data["From"];
                            item["to"] = data["To"];
                            item["gold"] = data["AG"];
                            item["i"] = data["I"];
                            item["msg"] = data["Msg"];
                            item["time"] = data["Time"];
                            item["s"] = (bool)data["S"] ? 0 : 1;
                            item["d"] = data["D"];
                            if (((string)item["from"]).ToLower() == "admin")
                                Config.mail20.Add(item);
                        }
                    }
                    User.userMain.nmAg = listFreeChip.Count;
                    // Telegram: cứ có mail ở event này về là mở ra nhận hết
                    if (listFreeChip.Count > 0)
                    {
                        List<int> mailIds = new();
                        foreach (JToken item in listFreeChip) mailIds.Add((int)item["Id"]);
                        SocketSend.OpenMultipleMailsContainChip(mailIds);
                    }
                    if (Config.curGameId == (int)GAMEID.SLOT_SIXIANG) UIManager.instance.playVideoSiXiang();
                    SocketSend.sendSelectGame(Config.curGameId);
                    break;
                case "31":
                    User.userMain.AG = (long)jsonData["totalAG"];
                    break;
                case "messagelist":
                    if (jsonData.ContainsKey("data"))
                    {
                        List<JObject> listMes = new List<JObject>();
                        JArray listFriend = new JArray();
                        if ((string)jsonData["data"] != "")
                        {
                            listMes = JArray.Parse((string)jsonData["data"]).ToObject<List<JObject>>();
                        }
                        User.userMain.messageUnRead = listMes.FindAll(data => (int)data["count"] != 0).Count;
                    }

                    break;
                case "followfind":
                    {


                        break;
                    }
                case "messagedetail":

                    if ((string)jsonData["data"] != "")
                    {
                    }
                    else
                    {
                    }
                    break;
                case "message":
                    {
                        if (!((string)jsonData["data"]).Equals(""))
                        {
                            JObject dataMess = JObject.Parse((string)jsonData["data"]);
                            JObject messageData = new JObject();
                            messageData["Vip"] = dataMess["vip"];
                            messageData["Name"] = dataMess["fromname"];
                            messageData["Avatar"] = dataMess["avatar"];
                            messageData["Data"] = dataMess["msg"];
                            messageData["FaceID"] = dataMess["fid"];
                            messageData["time"] = dataMess["timemsg"];
                            messageData["ID"] = dataMess["fromid"];
                            if (UIManager.instance.lobbyView.gameObject.activeSelf && UIManager.instance.gameView == null)
                            {
                                UIManager.instance.showDialog(Config.getTextConfig("has_mail"), Config.getTextConfig("txt_ok"), () =>
                                {
                                    UIManager.instance.destroyAllPopup();
                                    // UIManager.instance.lobbyView.onShowChatWorld(true);
                                }, Config.getTextConfig("label_cancel"));
                            }
                            else
                            {
                                UIManager.instance.showToast(Config.getTextConfig("has_mail"));
                            }
                        }

                        break;
                    }

                case "ltv":
                    {
                        Debug.Log("ltv: ");
                        if (jsonData["data"] != null)
                        {
                            UIManager.instance.lobbyView.isClicked = false;
                            JArray listLtv = JArray.Parse((string)jsonData["data"]);
                            if (listLtv.Count <= 0) return;
                            if (Config.listGamePlaynow.Contains(Config.curGameId))
                            {

                            }
                            else
                            {
                            }
                        }
                        break;
                    }
                case "roomVip":
                    break;

                case "roomTable":
                    break;

                case "checkPass":


                    break;

                case "getChatWorld":
                    {
                        COMMON_DATA.ListChatWorld = JArray.Parse((string)jsonData["data"]);
                        break;
                    }

                case "getChatGame":
                    break;
                case "16": //chat world message
                    {
                        JObject messageData = new JObject();
                        messageData["Vip"] = jsonData["V"];
                        messageData["ID"] = jsonData["ID"];
                        messageData["Name"] = jsonData["N"];
                        messageData["Avatar"] = jsonData["Avatar"];
                        messageData["Data"] = jsonData["D"];
                        messageData["FaceID"] = jsonData["fbid"];
                        messageData["time"] = jsonData["time"];
                        messageData["level"] = jsonData["level"];
                        messageData["Ag"] = jsonData["Ag"];
                        COMMON_DATA.ListChatWorld.Add(messageData);
                        break;
                    }

                case "ivp":

                    break;
                case "topgamer_new":
                    {
                        JArray dataListTop = (JArray)jsonData["list"];
                        break;
                    }
                case "salert":
                    {


                        if (Config.show_new_alert)
                        {

                            if (Config.list_Alert.Count < 20)
                            {
                                if (jsonData.ContainsKey("data"))
                                {

                                    Config.list_Alert.Add(jsonData);
                                    UIManager.instance.showAlertMessage(jsonData);
                                }
                                if (jsonData.ContainsKey("content"))
                                {
                                    Config.list_AlertShort.Add(jsonData);
                                    await AlertShort.Instance.checkShowAlertShort();
                                }
                            }
                        }

                        break;
                    }
                case "SAON":
                    {

                        break;
                    }
                case "uag":
                    {

                        User.userMain.AG = (long)jsonData["ag"];
                        User.userMain.VIP = (int)jsonData["vip"];
                        User.userMain.LQ = (int)jsonData["lq"];
                        UIManager.instance.updateAG();
                        SocketIOManager.getInstance().emitUpdateInfo();
                        break;
                    }
                case "uvip":
                    {
                        //GameManager.getInstance().userUpVip(jsonData);
                        User.userMain.VIP = 1;
                        User.userMain.AG += (long)jsonData["AG"];
                        UIManager.instance.updateVip();

                        SocketIOManager.getInstance().emitUpdateInfo();
                        UIManager.instance.showMessageBox(Config.getTextConfig("archive_vip1"));


                        break;
                    }
                case "changepass":
                    {
                        if ((int)jsonData["error"] == 1)
                        {
                            UIManager.instance.showDialog(Config.getTextConfig("change_pass_succes"), Config.getTextConfig("ok"), () =>
                            {

                                Config.typeLogin = LOGIN_TYPE.PLAYNOW;
                                UIManager.instance.showLoginScreen(true);
                            });
                        }
                        else
                        {
                            UIManager.instance.showMessageBox(Config.getTextConfig("error_change_pass"));
                        }
                        break;
                    }
                case "RUF":
                    {
                        if (jsonData.ContainsKey("U"))
                        {
                            User.userMain.Username = (string)jsonData["U"];
                            User.userMain.displayName = (string)jsonData["U"];

                            UIManager.instance.showDialog(
                                Config.getTextConfig("change_name_success") + " " + jsonData["U"], Config.getTextConfig("ok"),
                                () =>
                                {
                                    UIManager.instance.showLoginScreen(true);
                                }
                            );
                            UIManager.instance.updateInfo();
                        }
                        break;
                    }

                case "updateObjectGame":
                    {
                        if ((bool)jsonData["status"])
                        {
                            if (UIManager.instance.gameView != null)
                            {
                                UIManager.instance.gameView.updateItemVip(jsonData);
                            }
                        }
                        else
                        {
                            UIManager.instance.showToast((string)jsonData["msg"]);
                        }
                        break;
                    }
                case "iapResult":
                    {
                        var chip = (int)jsonData["goldPlus"];
                        var msg = (string)jsonData["msg"];
                        var signature = (string)jsonData["signature"];

                        if (chip > 0)
                        {
                            User.userMain.AG += chip;
                            UIManager.instance.updateAG();
                            SocketSend.sendUAG();
                        }
                        UIManager.instance.showMessageBox(msg);

                        var key_iap = User.userMain.Userid + "_iap_count";
                        var countIAP = PlayerPrefs.GetInt(key_iap, 0);
                        for (var i = 0; i < countIAP; i++)
                        {
                            var key_signdata = User.userMain.Userid + "_signdata_" + i;
                            var key_signature = User.userMain.Userid + "_signature_" + i;
                            var _signature = PlayerPrefs.GetString(key_signature);

                            if (_signature == signature)
                            {
                                PlayerPrefs.DeleteKey(key_signdata);
                                PlayerPrefs.DeleteKey(key_signature);
                                countIAP--;
                                PlayerPrefs.SetInt(key_iap, countIAP);
                                break;
                            }
                        }

                        SocketIOManager.getInstance().emitUpdateInfo();
                        break;
                    }
                case "iap_ios":
                    {
                        var chip = (int)jsonData["goldPlus"];
                        var msg = (string)jsonData["msg"];
                        var receipt = (string)jsonData["receipt"];

                        if (chip > 0)
                        {
                            User.userMain.AG += chip;
                            UIManager.instance.updateAG();
                            SocketSend.sendUAG();
                        }
                        UIManager.instance.showMessageBox(msg);
                        var key_iap = User.userMain.Userid + "_iap_count";
                        var countIAP = PlayerPrefs.GetInt(key_iap, 0);
                        for (var i = 0; i < countIAP; i++)
                        {
                            var key_receipt = User.userMain.Userid + "_receipt_" + i;
                            var _receipt = PlayerPrefs.GetString(key_receipt);

                            if (_receipt == receipt)
                            {
                                PlayerPrefs.DeleteKey(_receipt);
                                countIAP--;
                                PlayerPrefs.SetInt(key_iap, countIAP);
                                break;
                            }
                        }
                        SocketIOManager.getInstance().emitUpdateInfo();
                        break;
                    }
                case "jackpot":
                    break;
                case "jackpotwin":
                    {
                        break;
                    }
                case "updatejackpot":
                    {
                        if (jsonData != null && jsonData.ContainsKey("M"))
                        {
                            UIManager.instance.PusoyJackPot = (long)jsonData["M"];
                        }
                        break;
                    }
                case "jackpothistory":
                    {
                        break;
                    }
                case "cashOutHistory":
                    {
                        UIManager.instance.hideWatting();
                        break;
                    }
                case "getgift":
                    {
                        break;
                    }
                case "rejectCashout":
                    {
                        if ((int)jsonData["status"] == 0)
                        {


                            UIManager.instance.showWaiting();
                            DOTween.Sequence().AppendInterval(2.5f).AppendCallback(() =>
                            {
                                SocketSend.getMail(12);
                                SocketSend.sendDTHistory();
                                if ((string)jsonData["msg"] != "")
                                    UIManager.instance.showMessageBox((string)jsonData["msg"]);
                            });
                        }

                        break;
                    }

                case "autoExit":
                    {
                        if (UIManager.instance.gameView != null)
                        {
                            UIManager.instance.gameView.handleAutoExit(jsonData);
                        }
                        break;
                    }
                case "shareImageFb":
                    {
                        break;
                    }
                case "getWalletInfo":
                    {
                        break;
                    }
                case "checkUpdateWallet":
                    {
                        break;
                    }
                case "payment_success":
                    break;

                /* LOTO */
                case "lottery_topgame":
                    break;
                case "lottery_lotos":
                    break;
                case "lottery_results":
                    break;
                case "lottery_create":
                    break;
                case "lottery_cancel":
                    break;
                case "lottery_history":
                    break;
                /* END LOTO */
                ///-------SLOT SIXIANG--------//
                case ACTION_SLOT_SIXIANG.getInfo:
                case ACTION_SLOT_SIXIANG.normalSpin:
                case ACTION_SLOT_SIXIANG.getBonusGames:
                case ACTION_SLOT_SIXIANG.scatterSpin:
                case ACTION_SLOT_SIXIANG.buyBonusGame:
                case ACTION_SLOT_SIXIANG.dragonPearlSpin:
                case ACTION_SLOT_SIXIANG.rapidPay:
                case ACTION_SLOT_SIXIANG.goldPick:
                case ACTION_SLOT_SIXIANG.luckyDraw:
                case ACTION_SLOT_SIXIANG.selectBonusGame:
                    handleGameSiXiang(jsonData);
                    break;
                case "farmInfo":
                    {
                        Config.dataVipFarm = jsonData;
                        float farmPercent = (float)jsonData["farmPercent"];
                        if (farmPercent >= 100f)
                        {
                            if (UIManager.instance.gameView != null) return;
                            if (Config.is_First_CheckVIPFarms)
                            {
                                Config.is_First_CheckVIPFarms = false;
                            }
                            else if (UIManager.instance.lobbyView.gameObject.activeSelf)
                            {
                            }
                        }
                        break;
                    }
                case "farmReward":
                    {
                        break;
                    }
                case "deleteAccount":
                    if ((bool)jsonData["isSuccess"])
                    {
                        //cc.sys.localStorage.setItem("isLogOut", "true");
                        if (SettingView.instance != null)
                        {
                            SettingView.instance.handleDeleteAcount();
                        }
                        // GameManager.getInstance().onShowConfirmDialog("true");
                    }
                    else
                    {
                        // GameManager.getInstance().onShowConfirmDialog("false");
                    }
                    break;
            }
        }
        else if (jsonData.ContainsKey("idevt"))
        {

            int idevt = (int)jsonData["idevt"];
            Logging.Log("------------------------------------------------>ID EVT:" + idevt + "<------------------------------------------->\n" + jsonData);
            switch (idevt)
            {
                case 300:
                    Logging.Log("-=- update bank   " + jsonData.ToString());
                    User.userMain.agSafe = (long)jsonData["chip"];
                    UIManager.instance.updateAGSafe();
                    break;
                case 301://send to safe {"idevt":301,"status":true,"chipbank":11558654,"chipuser":84600}

                    if ((bool)jsonData["status"] == true)
                    {
                        User.userMain.AG = (long)jsonData["chipuser"];
                        User.userMain.agSafe = (long)jsonData["chipbank"];

                        UIManager.instance.updateAG();
                        UIManager.instance.updateAGSafe();
                    }
                    else
                    {
                        UIManager.instance.showDialog((string)jsonData["msg"], Config.getTextConfig("ok"));
                    }
                    break;
                case 302://get from safe
                    if ((bool)jsonData["status"] == true)
                    {
                        User.userMain.AG = (long)jsonData["chipuser"];
                        User.userMain.agSafe = (long)jsonData["chipbank"];
                        UIManager.instance.updateAG();
                        UIManager.instance.updateAGSafe();
                    }
                    else
                    {
                        UIManager.instance.showToast((string)jsonData["msg"]);
                    }
                    break;

                case 303://{"idevt":303,"status":true,"msg":"Send gift to your friend successfully!","chipbank":10999801,"chipuser":645000}
                    break;

                case 304:
                    break;

                case 500://get his safe
                    UIManager.instance.hideWatting();
                    break;
                case 400:
                    break;
                case 200:
                    {
                        break;
                    }

                case 202: //change name
                    if ((bool)jsonData["status"])
                    {
                        User.userMain.displayName = Config.user_name_temp;

                        User.userMain.Username = Config.user_name_temp;
                        PlayerPrefs.SetInt("isReg", 1);
                        UIManager.instance.updateInfo();


                        SocketIOManager.getInstance().emitUpdateInfo();
                    }

                    UIManager.instance.showMessageBox((string)jsonData["msg"]);
                    break;
                case 201:
                    {
                        break;
                    }
                case 800:

                    bool status = (bool)jsonData["status"];
                    if (status)
                    {
                        User.userMain.AG = (long)jsonData["AG"];
                        UIManager.instance.hideWatting();
                        UIManager.instance.updateAG();
                    }

                    UIManager.instance.showMessageBox((string)jsonData["msg"]);
                    break;
                case 801:

                    break;


            }
        }
    }
    public static void handleGameSiXiang(JObject data)
    {
        JObject dataGame = JObject.Parse((string)data["data"]);
        if (SiXiangView.Instance == null)
        {
            Debug.Log("clm chua co game sixiang la sao");
        }
        switch ((string)data["evt"])
        {
            case ACTION_SLOT_SIXIANG.getInfo:
                //dataGame = JObject.Parse(SiXiangFakeData.Instance.getInfoDragonPearl);
                SiXiangView.Instance.handleGetInfo(dataGame);
                break;
            case ACTION_SLOT_SIXIANG.getBonusGames:
                SiXiangView.Instance.handleBonusInfo(dataGame);
                break;
            case ACTION_SLOT_SIXIANG.normalSpin:
                SiXiangView.Instance.handleNormalSpin(dataGame);
                break;
            case ACTION_SLOT_SIXIANG.scatterSpin:
                SiXiangView.Instance.handleScatterSpin(dataGame);
                break;
            case ACTION_SLOT_SIXIANG.buyBonusGame:
                SiXiangView.Instance.handleBuyBonusGame(dataGame);
                break;
            case ACTION_SLOT_SIXIANG.dragonPearlSpin:
                SiXiangView.Instance.handleDragonPealsSpin(dataGame);
                break;
            case ACTION_SLOT_SIXIANG.rapidPay:
                SiXiangView.Instance.handleRapidPay(dataGame);
                break;
            case ACTION_SLOT_SIXIANG.goldPick:
                SiXiangView.Instance.handleGoldPick(dataGame);
                break;
            case ACTION_SLOT_SIXIANG.luckyDraw:
                SiXiangView.Instance.handleLuckyDraw(dataGame);
                break;
            case ACTION_SLOT_SIXIANG.selectBonusGame:
                SiXiangView.Instance.handleSelectBonusGame(dataGame);
                break;
        }
    }
}
