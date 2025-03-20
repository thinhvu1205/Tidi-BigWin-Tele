using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Globals;

public class SettingView : BaseView
{
    public static SettingView instance;

    [SerializeField] private Transform deletion;
    [SerializeField] private Button btnMusic, btnSound, btnVibration, btnGroup, btnFanpage, btnLogout, btnDeleteAc;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        instance = this;

        CURRENT_VIEW.setCurView(CURRENT_VIEW.SETTING_VIEW);
    }

    private new void Start()
    {
        base.Start();
        //tglMusic.SetIsOnWithoutNotify(Config.isMusic);
        //tglSound.SetIsOnWithoutNotify(Config.isSound);

        btnMusic.transform.GetChild(0).gameObject.SetActive(Config.isMusic);
        btnSound.transform.GetChild(0).gameObject.SetActive(Config.isSound);
        btnVibration.transform.GetChild(0).gameObject.SetActive(Config.isVibration);

        btnGroup.gameObject.SetActive(Config.is_bl_fb);
        btnFanpage.gameObject.SetActive(Config.is_bl_fb);
        btnLogout.gameObject.SetActive(false);
        if (UIManager.instance.gameView != null && UIManager.instance.gameView.gameObject.activeSelf)
        {
            btnDeleteAc.interactable = false;
            deletion.Find("iconDelAc").GetComponent<Image>().color = Color.gray;
            deletion.Find("lbDelAc").GetComponent<TextMeshProUGUI>().color = Color.gray;
            deletion.Find("btnDeleAc").GetComponent<Image>().color = Color.gray;
        }
        else
        {
            btnDeleteAc.interactable = true;
            deletion.Find("iconDelAc").GetComponent<Image>().color = Color.white;
            deletion.Find("lbDelAc").GetComponent<TextMeshProUGUI>().color = Color.white;
            deletion.Find("btnDeleAc").GetComponent<Image>().color = Color.white;
        }
    }
    public void onClickSound()
    {
        Config.isSound = !Config.isSound;
        if (Config.isSound)
        {
            SoundManager.instance.soundClick();
        }
        Config.updateConfigSetting();
        btnSound.transform.GetChild(0).gameObject.SetActive(Config.isSound);

    }
    public void onClickMusic()
    {
        Config.isMusic = !Config.isMusic;
        SoundManager.instance.soundClick();
        Config.updateConfigSetting();
        SoundManager.instance.playMusic();
        btnMusic.transform.GetChild(0).gameObject.SetActive(Config.isMusic);
    }


}
