using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Globals;
using System.Collections;
using Cysharp.Threading.Tasks;

public class HandleData
{
    public static float DelayHandleLeave = 0;
    public static void handleLoginResponse(string strData)
    {
        Logging.LogWarning("-=- =handleLoginResponse:  " + strData);
        LoginResponsePacket packet = JsonUtility.FromJson<LoginResponsePacket>(strData);

        if (packet.status == CMD.OK)
        {
            if (Config.typeLogin == LOGIN_TYPE.NORMAL) Config.setDataUser();
            else if (Config.typeLogin == LOGIN_TYPE.PLAYNOW)
            {
                PlayerPrefs.SetString("USER_PLAYNOW", UIManager.instance.loginView.accPlayNow);
                PlayerPrefs.SetString("PASS_PLAYNOW", UIManager.instance.loginView.passPlayNow);
                PlayerPrefs.Save();
            }

            PlayerPrefs.SetInt("type_login", (int)Config.typeLogin);

            string data = Config.Base64Decode(packet.credentials);
            Logging.LogWarning("-=- =dang nhap thanh cong:  " + data);
            JObject obj = JObject.Parse(data);
            string strUser = (string)obj["data"];
            Logging.LogWarning(strUser);
            JObject objUser = JObject.Parse(strUser);

            User.userMain = new User();
            User.userMain.Userid = (int)objUser["Userid"];
            User.userMain.Username = (string)objUser["Username"];
            User.userMain.Tinyurl = (string)objUser["Tinyurl"];
            User.userMain.AG = (long)objUser["AG"];
            User.userMain.LQ = (long)objUser["LQ"];
            User.userMain.VIP = (int)objUser["VIP"];
            User.userMain.MVip = (int)objUser["MVip"];
            User.userMain.markLevel = (int)objUser["markLevel"];
            User.userMain.PD = (int)objUser["PD"];
            User.userMain.OD = (int)objUser["OD"];
            User.userMain.Avatar = (int)objUser["A"];
            User.userMain.NM = (int)objUser["NM"] % 100;
            User.userMain.nmAg = (long)objUser["NM"] / 100;
            User.userMain.ListDP = (string)objUser["ListDP"];
            if (objUser["NewAccFBInDevice"] != null)
                User.userMain.NewAccFBInDevice = (int)objUser["NewAccFBInDevice"];

            if (objUser.ContainsKey("lastPlay"))
            {
                Config.lastGameIDSave = (int)objUser["lastPlay"];
            }
            User.userMain.agSafe = (long)objUser["chipbank"];
            User.userMain.NumFriendMail = (int)objUser["NumFriendMail"];
            User.userMain.gameNo = (int)objUser["gameNo"];
            if (objUser["Diamond"] != null)
                User.userMain.Diamond = (int)objUser["Diamond"];

            User.userMain.vippoint = (int)objUser["vippoint"];
            User.userMain.vippointMax = (int)objUser["vippointMax"];
            User.userMain.FacebookName = (string)objUser["FacebookName"];
            User.userMain.displayName = (string)objUser["displayName"];
            User.userMain.LQ0 = (float)objUser["LQ0"];
            User.userMain.CO = (float)objUser["CO"];
            User.userMain.CO0 = (float)objUser["CO0"];
            User.userMain.LQSMS = (float)objUser["LQSMS"];
            User.userMain.LQIAP = (float)objUser["LQIAP"];
            User.userMain.LQOther = (float)objUser["LQOther"];
            User.userMain.BLQ1 = (float)objUser["BLQ1"];
            User.userMain.BLQ3 = (float)objUser["BLQ3"];
            User.userMain.BLQ5 = (float)objUser["BLQ5"];
            User.userMain.BLQ7 = (float)objUser["BLQ7"];
            User.userMain.AVG7 = (float)objUser["AVG7"];
            User.userMain.Group = (float)objUser["Group"];
            User.userMain.CreateTime = (long)objUser["CreateTime"];
            if (objUser["keyObjectInGame"] != null)
                User.userMain.keyObjectInGame = (int)objUser["keyObjectInGame"];

            User.userMain.UsernameLQ = (string)objUser["UsernameLQ"];
            User.userMain.isShowMailAg = true;
            if (objUser["uidInvite"] != null)
                User.userMain.uidInvite = (int)objUser["uidInvite"];
            if (objUser["canInputInvite"] != null)
                User.userMain.canInputInvite = (bool)objUser["canInputInvite"];
            if (objUser["timeInputInvite"] != null)
                User.userMain.timeInputInvite = (int)objUser["timeInputInvite"];

            User.userMain.timeInputInviteRemain = DateTimeOffset.Now.ToUnixTimeMilliseconds() + User.userMain.timeInputInvite * 1000;
            User.userMain.lastGameID = (int)objUser["gameid"];

            PlayerPrefs.SetInt("isFirstOpen", 1);
            PlayerPrefs.Save();

            //SocketIOManager.getInstance().startSIO();
            SocketIOManager.getInstance().isSendFirst = true;
            Config.isLoginSuccess = true;
            JObject objLogin = new JObject();
            objLogin["evt"] = "0";
            objLogin["data"] = obj.ToString(Formatting.None);

            SocketIOManager.getInstance().DATAEVT0 = objLogin;
            SocketIOManager.getInstance().emitLogin();
            SocketIOManager.getInstance().emitSIOWithValue(objLogin, "LoginPacket", false);

            // if (Config.curGameId == 0) Config.curGameId = (int)objUser["gameid"];
            LoadConfig.instance.getConfigInfo();
            LoadConfig.instance.isLoadedConfig = false;
            LoadConfig.instance.getInfoUser(strUser);

            if (Config.typeLogin == LOGIN_TYPE.NORMAL) Config.saveLoginAccount();
            //Logging.Log("emit update info o day nua");
            SocketIOManager.getInstance().emitUpdateInfo();
            Dictionary<string, object> tags = new Dictionary<string, object>();
            SocketSend.sendRef();
            // SocketSend.getMail(10);
            SocketSend.getMail(12);
            SocketSend.getInfoSafe();
            SocketSend.sendPromotion();
            SocketSend.getMessList();
        }
        else
        {
            Logging.Log(packet.message);

            UIManager.instance.showMessageBox(packet.message);
            //{ "screenname":null,"pid":0,"status":"DENIED","code":-3,"message":"Username and Password do not match!","credentials":"","classId":11}

            var objData = new JObject();
            objData["codeError"] = packet.code;
            objData["MsgError"] = packet.message;
            SocketIOManager.getInstance().emitSIOWithValue(objData, "LoginPacket", false);
        }
    }

    public static void handleServiceTransportPacket(string strData)
    {
        ServiceTransportPacket packet = JsonUtility.FromJson<ServiceTransportPacket>(strData);
        packet.str_servicedata = (string)JObject.Parse(strData)["servicedata"];
        string data = Config.Base64Decode(packet.str_servicedata);
        HandleService.processData(JObject.Parse(data));
    }

    public static void handleGameTransportPacket(string strData)
    {
        //Logging.Log("handleGameTransportPacket   " + strData);
        GameTransportPacket packet = JsonUtility.FromJson<GameTransportPacket>(strData);
        packet.str_gamedata = (string)JObject.Parse(strData)["gamedata"];
        string data = Config.Base64Decode(packet.str_gamedata);
        HandleGame.processData(JObject.Parse(data));
    }
    public static void handleForcedLogoutPacket(string strData)
    {
        JObject data = JObject.Parse(strData);
        string message = (string)data["message"];

    }
    public static void handleJoinResponsePacket(string strData)
    {
        //{ "tableid":14,"seat":0,"status":"OK","code":0,"classId":31}
        Debug.Log("handleJoinResponsePacket:" + strData);
        JObject data = JObject.Parse(strData);
        //string message = (string)data["message"];

        if ((string)data["status"] == "OK")
        {

            Config.tableId = (int)data["tableid"];
            Debug.Log("tableId2=" + Config.tableId);
            JObject dataJson = new JObject();
            dataJson["tableid"] = Config.tableId;
            dataJson["curGameID"] = Config.curGameId;
            SocketIOManager.getInstance().emitSIOWithValue(dataJson, "JoinPacket", false);
            UIManager.instance.showGame();
        }
        else
        {
            string _str = "";
            switch ((int)data["code"])
            {
                case -4:
                    _str = "";
                    break;
                case -5:
                    _str = Config.getTextConfig(
                        "err_table_another_table"
                    );
                    break;
                case -6:
                    _str = Config.getTextConfig("err_table_full");
                    break;
                case -7:
                    // _str = Config.getTextConfig("err_table_vip");
                    break;
                case -8:
                    _str = Config.getTextConfig("txt_not_enough_money_gl");
                    break;
            }


            JObject dataJson = new JObject();
            dataJson["codeError"] = data["code"];
            dataJson["msgError"] = _str;
            SocketIOManager.getInstance().emitSIOWithValue(dataJson, "JoinPacket", false);
            if (_str != "")
                UIManager.instance.showMessageBox(_str);
        }
    }

    public static async void handleLeaveResponsePacket(string strData)
    {
        Logging.Log("handleLeaveResponsePacket  " + Config.curGameId + " / " + strData);

        if (DelayHandleLeave > 0)
        {
            await UniTask.Delay((int)DelayHandleLeave * 1000);
            DelayHandleLeave = 0f;
        }
        JObject packet = JObject.Parse(strData);
        //string message = (string)data["message"];
        if ((string)packet["status"] == "OK")
        {
            if (UIManager.instance.gameView != null)
            {
                if (!Config.listGamePlaynow.Contains(Config.curGameId)) HandleGame.handleLeave();
                else
                {
                    UIManager.instance.showMessageBox("You have been inactive for a while, please reconnect.",
                        () =>
                        {
                            SocketSend.sendSelectGame(Config.curGameId);
                            UIManager.instance.gameView.dataLeave = null;
                            UIManager.instance.gameView.destroyThis();
                            UIManager.instance.gameView = null;
                        }
                    );
                }
            }
        }
        else
        {
            JObject dataJson = new JObject();
            dataJson["codeError"] = packet["code"];
            dataJson["msgError"] = packet["status"];
            SocketIOManager.getInstance().emitSIOWithValue(dataJson, "LeavePacket", false);
        }
    }
}

