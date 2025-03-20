using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using Globals;

public class SocketIOManager
{
    string REGINFO = "reginfo";
    string LOGIN = "login";
    string BEHAVIOR = "behavior";
    string UPDATE = "update";

    //SocketIO clientIO = null;
    private SocketIOUnity clientIO;
    static SocketIOManager instance = null;

    ConnectionStatus connectionStatus = ConnectionStatus.NONE;

    List<string> packetDetail = new List<string>(); //evt nào có trong array này thì bắn đủ data (bắn lên "packetDetail")
    List<string> blackListBehaviorIgnore = new List<string>(); //behaviorI: (behavior Ignore) evt nào có trong đây thì ko bắn lên  (bắn lên "behavior")
    List<string> whiteListOnlySendEvt = new List<string>(); //packet: evt nào có trong array này thì bắn evt, isSend, timestamp.. (bắn lên "packet")
    List<string> listResendData = new List<string>();
    List<JObject> listDataResendForPacket = new List<JObject>();

    public bool isSendFirst = false;
    bool isGetedListFillter = false;
    bool isEmitReginfo = false;
    public JObject DATAEVT0 = null;

    public static SocketIOManager getInstance()
    {
        if (instance == null)
        {
            instance = new SocketIOManager();
        }

        return instance;
    }

    public SocketIOManager()
    {
    }

    public void intiSml()
    {
        try
        {
            var _blackList = PlayerPrefs.GetString("dataFilter", "");
            if (!_blackList.Equals(""))
            {
                var blackList = JObject.Parse(PlayerPrefs.GetString("dataFilter"));
                if (blackList != null)
                {
                    packetDetail = ((JArray)blackList["packetDetail"]).ToObject<List<string>>();
                    blackListBehaviorIgnore = ((JArray)blackList["behaviorI"]).ToObject<List<string>>();
                    whiteListOnlySendEvt = ((JArray)blackList["packet"]).ToObject<List<string>>();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

    }
    string url_old = "";
    public void startSIO()
    {
        try
        {
            //Config.u_SIO = "https://sio.jakartagames.net/diamond.domino.slots";
            Debug.Log("-=-== startSIO " + Config.u_SIO);

            if (!url_old.Equals(Config.u_SIO))
            {
                url_old = Config.u_SIO;
                stopIO();
            }
            if (connectionStatus == ConnectionStatus.CONNECTED || connectionStatus == ConnectionStatus.CONNECTING) return;

            Debug.Log("-=-== start Connect " + Config.u_SIO);
            //IO.Options options = new IO.Options();
            SocketIOOptions options = new SocketIOOptions();
            options.IgnoreServerCertificateValidation = true;
            var uri = new Uri(Config.u_SIO);
            //clientIO = IO.Socket(uri, options);
            clientIO = new SocketIOUnity(uri, options);
            clientIO.JsonSerializer = new NewtonsoftJsonSerializer();
            connectionStatus = ConnectionStatus.CONNECTING;
            clientIO.OnConnected += (sender, e) =>
            {
                Debug.Log("-=-== CONNECTED SIO ");
                connectionStatus = ConnectionStatus.CONNECTED;
                if (!isEmitReginfo)
                {

                    emitReginfo();
                    isEmitReginfo = true;
                }


                if (isSendFirst)
                {
                    if (Config.isLoginSuccess)
                    {
                        emitLogin();
                    }
                }


                if (DATAEVT0 != null)
                {

                    if (Config.isLoginSuccess)
                    {
                        emitSIOWithValue(DATAEVT0, "LoginPacket", false);
                    }
                }

                for (var i = 0; i < listResendData.Count; i++)
                {
                    emitSIO(listResendData[i]);
                }

                listResendData.Clear();
            };

            clientIO.OnDisconnected += (sender, e) =>
            {
                Debug.Log("SML DISCONNECTED");
                isSendFirst = false;
                isEmitReginfo = false;
                connectionStatus = ConnectionStatus.DISCONNECTED;
            };
            clientIO.OnError += (sender, e) =>
            {
                Debug.Log("SML Connect Error:" + e.ToString());
                isSendFirst = false;
                isEmitReginfo = false;
                connectionStatus = ConnectionStatus.DISCONNECTED;
            };

            clientIO.On("event", data =>
            {
                Debug.Log("SML===============> event:" + data.ToString());
                UnityMainThread.instance.AddJob(() =>
                {
                    string dataStr = data.ToString();
                    handleEvent(dataStr);
                });
            });
            clientIO.Connect();
        }

        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void stopIO()
    {
        if (clientIO != null)
        {
            clientIO.Disconnect();
        }
        clientIO = null;
    }

    void handleEvent(string strData)
    {
        var dataArr = JArray.Parse(strData);
        var data = dataArr[0];
        //{ "event":"filter","packetDetail":["0","LoginPacket","JoinPacket","LeavePacket"],"packet":["0","ltv","pctable","selectG2","uag"],"behaviorI":[],"valueGet":[]}
        var evt = (string)data["event"];
        Debug.Log("===============> SIO: handleEvent la " + strData);
        try
        {
            switch (evt)
            {
                case "filter":
                    {
                        Debug.Log("-=-= filter");
                        //PlayerPrefs.SetString("dataFilter", strData);
                        packetDetail = ((JArray)data["packetDetail"]).ToObject<List<string>>();
                        blackListBehaviorIgnore = ((JArray)data["behaviorI"]).ToObject<List<string>>();
                        whiteListOnlySendEvt = ((JArray)data["packet"]).ToObject<List<string>>();

                        //Debug.Log("packetDetail");
                        //Debug.Log("blackListBehaviorIgnore");
                        //Debug.Log(blackListBehaviorIgnore.ToString());
                        //Debug.Log("whiteListOnlySendEvt");
                        //Debug.Log(whiteListOnlySendEvt.ToString());
                        isGetedListFillter = true;
                        while (listDataResendForPacket.Count > 0)
                        {
                            var resend = listDataResendForPacket[0];
                            emitSIOWithValuePacket((JObject)resend["strData"], (string)resend["namePackage"], (bool)resend["isSend"], (bool)resend["isPacketDetai"], (long)resend["timestamp"]);
                            listDataResendForPacket.RemoveAt(0);
                        }
                        break;
                    }
                case "banner":
                    {
                        JArray arrData = (JArray)data["data"];
                        JArray arrOnlistFalse = new JArray();
                        JArray arrOnlistTrue = new JArray();
                        JArray arrBannerLobby = new JArray();

                        for (var i = 0; i < arrData.Count; i++)
                        {
                            var item = (JObject)arrData[i];
                            if (item.ContainsKey("urlImg") && !((string)item["urlImg"]).Equals(""))
                            {
                                if (item.ContainsKey("showByActionType") && (int)item["showByActionType"] == 9)
                                {
                                    arrBannerLobby.Add(item);
                                }
                                else if (item.ContainsKey("isOnList") && (bool)item["isOnList"])
                                {
                                    arrOnlistTrue.Add(item);
                                }
                                else
                                {
                                    arrOnlistFalse.Add(item);
                                }
                            }
                        }

                        if (arrBannerLobby.Count > 0)
                        {
                            Config.arrBannerLobby = arrBannerLobby;
                        }
                        //UIManager.instance.preLoadBaner(data.data);
                        Config.arrOnlistTrue.Merge(arrOnlistTrue);
                        break;
                    }
                case "getcf":
                    {
                        break;
                    }
            }

        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    void emitSIO(string strData)
    {
        if (clientIO != null && connectionStatus == ConnectionStatus.CONNECTED)
        {
            Debug.Log("-=-=SML emitSIO  data: " + strData);
            if (!IsJSON(strData))
            {
                clientIO.Emit("event", strData);
            }
            else
            {
                clientIO.EmitStringAsJSON("event", strData);
            }
        }
        else
        {
            //listResendEvent.Add(eventName);

            if (listResendData.Count < 100)
            {
                listResendData.Add(strData);
            }

        }
    }
    public static bool IsJSON(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) { return false; }
        str = str.Trim();
        if ((str.StartsWith("{") && str.EndsWith("}")) || //For object
            (str.StartsWith("[") && str.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JToken.Parse(str);
                return true;
            }
            catch (Exception ex) //some other exception
            {
                Debug.LogError(ex.ToString());
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    void emitSIOWithMapData(string evtName, Dictionary<string, string> mapData)
    {
        var objectVL = new JObject();
        foreach (var kvp in mapData)
        {
            objectVL[kvp.Key] = kvp.Value;
        }
        objectVL["event"] = evtName;
        objectVL["timestamp"] = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        emitSIO(objectVL.ToString());
    }

    public void emitSIOWithValue(JObject objectVL, string namePackage, bool isSend)
    {
        ////packetDetail: evt nào có trong array này thì bắn đủ data (bắn lên "packetDetail")
        emitSIOWithValuePacket(objectVL, namePackage, isSend, true);

        ////packet: evt nào có trong array này thì bắn evt, isSend, timestamp.. (bắn lên "packet")
        emitSIOWithValuePacket(objectVL, namePackage, isSend, false);
    }

    public void emitSIOCCCNew(string strData)
    {
        try
        {
            if (blackListBehaviorIgnore.Contains(strData) || blackListBehaviorIgnore.Contains("all_sio"))
            {
                // cc.NGWlog("SIO: emitSIOCCC EVT NAY THUOC DIEN CHINH SACH KO DUOC GUI DI :( -  evt: " + strData);
                return;
            }
            var mapDM = new Dictionary<string, string>();
            mapDM.Add(BEHAVIOR, strData);
            emitSIOWithMapData(BEHAVIOR, mapDM);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    void emitSIOWithValuePacket(JObject packetValue, string namePackage, bool isSend, bool isPacketDetai, long timeStamp = 0)
    {
        try
        {
            var timestamp = System.DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var objectVV = packetValue; //packetValue.slice();

            if (connectionStatus != ConnectionStatus.CONNECTED || !isGetedListFillter)
            {
                var objSave = new JObject();
                objSave["strData"] = packetValue;
                objSave["isSend"] = isSend;
                objSave["isPacketDetai"] = isPacketDetai;
                objSave["namePackage"] = namePackage;
                objSave["timestamp"] = timestamp;

                listDataResendForPacket.Add(objSave);
                return;
            }
            var evtt = "";

            if (objectVV.ContainsKey("evt"))
            {
                evtt = (string)objectVV["evt"];
            }
            else if (objectVV.ContainsKey("idevt"))
            {
                evtt = (string)objectVV["idevt"];
            }
            else
            {
                evtt = namePackage;
                objectVV["evt"] = evtt;

            }
            if (isPacketDetai)
            {
                if (packetDetail.Contains(evtt) || packetDetail.Contains("all_sio"))
                {
                    objectVV["event"] = "packetDetail";
                    if ((string)packetValue["evt"] == "0")
                    {
                        DATAEVT0 = packetValue;
                    }
                }
                else
                {
                    //cc.NGWlog("SIO: EVT NAY THUOC DIEN CHINH SACH KO DUOC GUI DI :( -  evt: " + evtt);
                    return;
                }
            }
            else
            {
                if (whiteListOnlySendEvt.Contains(evtt) || whiteListOnlySendEvt.Contains("all_sio"))
                {
                    objectVV = new JObject();
                    objectVV["evt"] = evtt;
                    objectVV["event"] = "packet";
                }
                else
                {
                    //cc.NGWlog("SIO: =-=-=-=-==== CHIM CUT");
                    return;
                }
            }
            objectVV["packetData"] = namePackage;
            objectVV["isSendData"] = isSend;
            objectVV["timestamp"] = (timeStamp == 0 ? System.DateTimeOffset.Now.ToUnixTimeMilliseconds() : timeStamp);
            emitSIO(objectVV.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    //Gui sau' khi connect success --> gui thong tin device
    void emitReginfo()
    {
        //try
        //{
        JObject objectVL = new JObject();
        objectVL["event"] = REGINFO;
        //var osName = "web";
        var osName = "Android";
        if (Application.platform == RuntimePlatform.Android)
            osName = "Android";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            osName = "iOS";

        objectVL["location"] = "WHERE";
        objectVL["pkgname"] = Config.package_name;
        objectVL["versionCode"] = Config.versionGame;
        objectVL["versionName"] = Config.versionNameOS;
        objectVL["versionDevice"] = Config.versionDevice;
        objectVL["os"] = osName;
        objectVL["language"] = Config.language;
        objectVL["model"] = Config.model;
        objectVL["brand"] = Config.brand;

        //JArray jArray = new JArray();
        //jArray.Add(Screen.currentResolution.width);
        //jArray.Add(Screen.currentResolution.height);
        //objectVL["resolution"] = jArray;
        objectVL["time_start"] = Config.TimeOpenApp;
        objectVL["devID"] = Config.deviceId;
        objectVL["operatorID"] = Config.OPERATOR;
        emitSIO(objectVL.ToString());
        //}
        //catch (System.Exception e)
        //{

        //    Debug.LogException(e);
        //}
    }

    public void emitLogin()
    {
        //// isSendFirst = false;
        ////tracking io khi login success
        var mapDataLogin = new Dictionary<string, string>();
        mapDataLogin.Add("event", LOGIN);
        mapDataLogin.Add("gameIP", Config.curServerIp);
        mapDataLogin.Add("verHotUpdate", Config.versionGame);
        mapDataLogin.Add("id", User.userMain.Userid.ToString());
        mapDataLogin.Add("name", User.userMain.Username);
        mapDataLogin.Add("ag", User.userMain.AG + "");
        mapDataLogin.Add("vip", User.userMain.VIP + "");
        mapDataLogin.Add("lq", User.userMain.LQ + "");
        mapDataLogin.Add("curView", CURRENT_VIEW.getCurrentSceneName());
        mapDataLogin.Add("gameID", Config.curGameId + "");
        mapDataLogin.Add("disID", Config.disID + "");
        emitSIOWithMapData(LOGIN, mapDataLogin);

    }

    public void emitUpdateInfo()
    {
        var mapData = new Dictionary<string, string>();
        mapData.Add("id", User.userMain.Userid + "");
        mapData.Add("name", User.userMain.Username);
        mapData.Add("ag", User.userMain.AG + "");
        mapData.Add("vip", User.userMain.VIP + "");
        mapData.Add("lq", User.userMain.LQ + "");
        mapData.Add("curView", CURRENT_VIEW.getCurrentSceneName());
        mapData.Add("gameID", Config.curGameId + "");

        emitSIOWithMapData(UPDATE, mapData);
    }
}