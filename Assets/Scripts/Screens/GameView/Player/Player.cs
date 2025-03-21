using Globals;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public int id;
    public string namePl, displayName;
    public PlayerView playerView;
    public int _indexDynamic, vip = 0, avatar_id = 0;
    public long ag = 0;
    public string fid = "";

    public bool is_host = false, is_ready = false, is_turn = false;

    public int idVip = 0;

    public void updatePlayerView()
    {
        setName();
        setAvatar();
        setAg();
        playerView.isThisPlayer = Globals.User.userMain.Userid == id;
        playerView.setCallbackClick(() => { UIManager.instance.gameView.onClickInfoPlayer(this); });
    }

    public void clearAllCard()
    {
    }

    public void setHost(bool _isHost)
    {
        this.is_host = _isHost;
    }


    public void setReady(bool isReady)
    {
    }



    public void setName()
    {
        playerView.setName((displayName != null ? displayName : namePl));
    }

    public void setAvatar()
    {
        playerView.setAvatar(avatar_id, namePl, fid, vip);
    }

    public void updateItemVipFromSV(int id)
    {
        Debug.Log("-=-=updateItemVipFromSV " + id);
        idVip = id;
        playerView.updateItemVipFromSV(id);
    }

    public void updateItemVip(int vip, int idPosTongits = -1)
    {
        playerView.updateItemVip(idVip, vip, idPosTongits);
    }

    public void setAg()
    {
        playerView.setAg(ag);
    }
    public void updateMoney()
    {
        if (playerView != null)
            playerView.setAg(ag);
    }
}
