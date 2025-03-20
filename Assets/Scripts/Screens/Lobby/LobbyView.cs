using UnityEngine;
using Globals;
using System.Collections;


public class LobbyView : BaseView
{
    private Coroutine _GetInfoPusoyJackPotC;

    protected override void OnEnable()
    {
        CURRENT_VIEW.setCurView(CURRENT_VIEW.GAMELIST_VIEW);
        SoundManager.instance.playMusic();
        if (Config.curGameId == (int)GAMEID.PUSOY)
            _GetInfoPusoyJackPotC = StartCoroutine(_GetJackpotPusoy());
        if (Config.isChangeTable)
        {
            Config.isChangeTable = false;
            if (Config.listGamePlaynow.Contains(Config.curGameId)) SocketSend.sendPlayNow(Config.curGameId);
            else SocketSend.sendChangeTable(Config.tableMark, Config.tableId);
        }
    }
    private void OnDisable()
    {
        if (_GetInfoPusoyJackPotC != null) StopCoroutine(_GetInfoPusoyJackPotC);
    }
    private IEnumerator _GetJackpotPusoy()
    {
        while (User.userMain == null)
        {
            yield return new WaitForSeconds(.2f);
        }
        while (true)
        {
            SocketSend.sendUpdateJackpot((int)GAMEID.PUSOY);
            yield return new WaitForSeconds(5);
        }
    }


    public bool isClicked = false;

}
