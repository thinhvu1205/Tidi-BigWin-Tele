using Globals;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GroupMenuView : BaseView
{
    public static GroupMenuView instance;
    [SerializeField] Button btnSetting;
    [SerializeField] Button btnChangeTable;
    [SerializeField] Button btnFightTongits;
    [SerializeField] Button btnMusic;
    [SerializeField] Button btnSound;
    [SerializeField] Button btnRule;
    [SerializeField] List<Sprite> listCheck = new List<Sprite>();

    public void onClickSwitchTable()
    {
        SoundManager.instance.soundClick();
        if (UIManager.instance.gameView.stateGame == STATE_GAME.PLAYING)
        {
            UIManager.instance.showToast(Config.getTextConfig("txt_intable"));
        }
        else
        {
            //Global.MainView._isClickGame = false;
            Config.isChangeTable = true;
            onClickBack();
        }
        hide();
    }
    protected override void Start()
    {
        GroupMenuView.instance = this;
        base.Start();
        var curGameId = Config.curGameId;
        if (curGameId == (int)GAMEID.KEANG || curGameId == (int)GAMEID.DUMMY)
        {
            btnSetting.gameObject.SetActive(false);
        }
        if (curGameId == (int)GAMEID.SLOT20FRUIT || curGameId == (int)GAMEID.SLOTNOEL || (curGameId == (int)GAMEID.SLOTTARZAN) || (curGameId == (int)GAMEID.SLOT_JUICY_GARDEN) || (curGameId == (int)GAMEID.SLOT_SIXIANG) || (curGameId == (int)GAMEID.SLOT_INCA))
        {
            btnChangeTable.gameObject.SetActive(false);
        }
        if (curGameId == (int)GAMEID.RONGHO)
        {
            btnRule.gameObject.SetActive(false);
        }
        if (curGameId == (int)GAMEID.TONGITS || curGameId == (int)GAMEID.TONGITS_OLD || curGameId == (int)GAMEID.TONGITS11 || curGameId == (int)GAMEID.TONGITS_JOKER)
        {
            btnFightTongits.transform.Find("on").GetComponent<Image>().sprite = TongitsView.IsFight ? listCheck[0] : listCheck[1];
            btnSetting.gameObject.SetActive(false);
            btnFightTongits.gameObject.SetActive(true);
            btnMusic.gameObject.SetActive(true);
            btnSound.gameObject.SetActive(true);
            btnSound.transform.Find("on").GetComponent<Image>().sprite = Config.isSound ? listCheck[0] : listCheck[1];
            btnMusic.transform.Find("on").GetComponent<Image>().sprite = Config.isMusic ? listCheck[0] : listCheck[1];
        }

        background.GetComponent<LayoutSizeControl>().updateSizeContent();
        var sizee2 = background.GetComponent<RectTransform>().sizeDelta;
        setOriginPosition(-transform.parent.GetComponent<RectTransform>().rect.width * .5f + sizee2.x * .5f + 10f, 720.0f * .5f - sizee2.y * .5f - 30f);
        show();
    }

    public void onClickRule()
    {
        SoundManager.instance.soundClick();
        hide();
        var curGameId = Config.curGameId;
        var urlRule = Config.url_rule.Replace("%gameid%", curGameId + "");
        //var langLocal = cc.sys.localStorage.getItem("language_client");
        //var language = langLocal == LANGUAGE_TEXT_CONFIG.LANG_EN ? "en" : "thai"
        var language = "thai";
        urlRule = urlRule.Replace("%language%", language);
        // https://conf.topbangkokclub.com/rule/index.html?gameid=%gameid%&language=%language%&list=true
        List<int> listGameOther = new List<int> { (int)GAMEID.SLOT20FRUIT, (int)GAMEID.SLOT_SIXIANG, (int)GAMEID.SLOT_INCA, (int)GAMEID.SLOTNOEL, (int)GAMEID.SLOTTARZAN, (int)GAMEID.LUCKY9, (int)GAMEID.SICBO, (int)GAMEID.SABONG, (int)GAMEID.SLOT_INCA, (int)GAMEID.GAOGEA, (int)GAMEID.SLOT_JUICY_GARDEN, (int)GAMEID.BANDAR_QQ, (int)GAMEID.LUCKY9 };
        UIManager.instance.gameView.onClickRule();
    }

    public void onClickSetting()
    {
        SoundManager.instance.soundClick();
        hide();
        UIManager.instance.openSetting();
    }

    public void onClickFightConfirm()
    {
        SoundManager.instance.soundClick();
        TongitsView.IsFight = !TongitsView.IsFight;
        btnFightTongits.transform.Find("on").GetComponent<Image>().sprite = TongitsView.IsFight ? listCheck[0] : listCheck[1];
    }
    public void onClickSound()
    {
        Config.isSound = !Config.isSound;
        if (Config.isSound)
        {
            SoundManager.instance.soundClick();
        }
        Config.updateConfigSetting();
        btnSound.transform.Find("on").GetComponent<Image>().sprite = Config.isSound ? listCheck[0] : listCheck[1];
    }
    public void onClickMusic()
    {
        Config.isMusic = !Config.isMusic;
        SoundManager.instance.soundClick();
        Config.updateConfigSetting();
        SoundManager.instance.playMusic();
        btnMusic.transform.Find("on").GetComponent<Image>().sprite = Config.isMusic ? listCheck[0] : listCheck[1];
    }

    public void onClickBack()
    {
        SoundManager.instance.soundClick();
        if (Config.curGameId == (int)GAMEID.SLOTNOEL || Config.curGameId == (int)GAMEID.SLOTTARZAN || Config.curGameId == (int)GAMEID.SLOT_SIXIANG) //cac game playnow
        {

            hide();
            if (Config.curGameId == (int)GAMEID.SLOT_SIXIANG)
            {
                SocketSend.sendExitSlotSixiang(ACTION_SLOT_SIXIANG.exitGame);
                //string dataLTable = "{\"evt\":\"ltable\",\"Name\":\"${User.userMain.displayName}\",\"errorCode\":0}";
                JObject dataLTable = new JObject();

                dataLTable["evt"] = "ltable";
                dataLTable["Name"] = User.userMain.displayName;
                dataLTable["errorCode"] = 0;
                JObject dataLeave = new JObject();
                dataLeave["tableid"] = Config.tableId;
                dataLeave["curGameID"] = (int)GAMEID.SLOT_SIXIANG;
                dataLeave["stake"] = 0;
                dataLeave["reason"] = 0;
                //UIManager.instance.gameView.dataLeave=dataLeave;
                HandleGame.processData(dataLTable);
                JObject dataLeavePackage = new JObject();
                dataLeavePackage["tableid"] = Config.tableId;
                dataLeavePackage["status"] = "OK";
                dataLeavePackage["code"] = 0;
                dataLeavePackage["classId"] = 37;
                HandleData.handleLeaveResponsePacket(dataLeavePackage.ToString());

            }
            else
            {
                SocketSend.sendExitGame();
            }
            return;
        }
        else
        {
            Logging.Log("Chay vao day!! gameView.stateGame=" + UIManager.instance.gameView.stateGame);
            if (UIManager.instance.gameView.stateGame == STATE_GAME.PLAYING)
            {
                Config.isBackGame = !Config.isBackGame;
                UIManager.instance.gameView.thisPlayer.playerView.setExit(Config.isBackGame);
                string msg = Config.isBackGame ? Config.getTextConfig("wait_game_end_to_leave") : Config.getTextConfig("minidice_unsign_leave_table");
                UIManager.instance.showToast(msg);
                Debug.Log("back game " + Config.isBackGame);

            }
            else//con moi 1 minh minh thi cung cho thoat
            {
                SocketSend.sendExitGame();
            }
            hide();
        }
    }
    public void onClickQuit()
    {
        SoundManager.instance.soundClick();
        onClickBack();
    }
}
