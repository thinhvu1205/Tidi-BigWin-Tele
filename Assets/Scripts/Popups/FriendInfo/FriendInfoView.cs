using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
public class FriendInfoView : BaseView
{

    public static FriendInfoView instance = null;
    [SerializeField] TextMeshProUGUI lbNameUser, lbChips, lbUserId, lbStatus;

    [SerializeField] Avatar avatar;

    [SerializeField] VipContainer vipContainer;
    [HideInInspector] public string idFriend;
    JObject dataFriend = new JObject();

    [SerializeField] GameObject btnMessage, btnSendGift;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        instance = this;
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void setInfo(JObject jsonData)
    {
        dataFriend = jsonData;
        string name = (string)jsonData["name"];
        int avatarId = (int)jsonData["avatar"];
        string userId = (string)jsonData["uid"];
        string fbId = (string)jsonData["fbid"];
        int chip = (int)jsonData["ag"];
        int vip = (int)jsonData["vip"];
        lbNameUser.text = name;
        lbUserId.text = "ID: " + userId;
        idFriend = userId;
        lbChips.text = Globals.Config.FormatNumber(chip);
        avatar.loadAvatar(avatarId, name, fbId);
        avatar.setVip(vip);
        vipContainer.setVip(vip);
        lbStatus.text = (string)jsonData["status"];

        btnMessage.SetActive(Globals.User.userMain.Userid.ToString() != userId);
        btnSendGift.SetActive(Globals.User.userMain.Userid.ToString() != userId);
        if (UIManager.instance.gameView != null)
        {
            btnMessage.SetActive(false);
            btnSendGift.SetActive(false);
        }
    }
    public void onClickSendMessage()
    {
        JObject dataChat = new JObject();
        dataChat["Name"] = dataFriend["name"];
        dataChat["ID"] = dataFriend["uid"];
        dataChat["Avatar"] = dataFriend["avatar"];
        dataChat["FaceID"] = dataFriend["fbid"];
        dataChat["vip"] = dataFriend["vip"];
        hide();
    }
    public void onClickSendGift()
    {
        // UIManager.instance.openSendGift(idFriend);
    }
}
