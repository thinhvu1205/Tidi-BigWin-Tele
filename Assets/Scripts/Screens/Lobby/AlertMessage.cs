using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Newtonsoft.Json.Linq;

public class AlertMessage : MonoBehaviour
{
    public static AlertMessage instance;
    [SerializeField] TextMeshProUGUI lbAlert;
    [SerializeField] RectTransform rectTfParent;
    private List<JObject> listData = new List<JObject>();

    private bool isRunning = false;
    private Rect parentRect;
    void Awake()
    {
        instance = this;
        rectTfParent = transform.parent.GetComponent<RectTransform>();
        parentRect = rectTfParent.rect;
        lbAlert.transform.localPosition = new Vector2(parentRect.width / 2, 17);
    }

    public void addAlertMessage(JObject data)
    {

        listData.Add(data);
        if (!isRunning)
        {
            showAlertMessage();
        }
    }
    public void showAlertMessage()
    {

        if (listData.Count > 0 && !UIManager.instance.isLoginShow())
        {
            if (UIManager.instance.gameView == null)
            {
                if (!gameObject.activeSelf)
                {
                }
            }
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            isRunning = true;
            JObject data = listData[0];
            listData.RemoveAt(0);
            Globals.Config.list_Alert.Remove(data);
            lbAlert.text = (string)data["data"];
            Vector2 posEnd = Vector2.zero;
            if (transform.localEulerAngles.z == 0)
            {
                lbAlert.transform.localPosition = new Vector2(parentRect.width / 2, 17);
                posEnd = new Vector2(-parentRect.width / 2 - lbAlert.preferredWidth, 17);
            }
            else
            {
                lbAlert.transform.localPosition = new Vector2(parentRect.height / 2 + 17, 17);
                posEnd = new Vector2(-parentRect.height / 2 - lbAlert.preferredWidth, 17);
            }
            lbAlert.transform.DOLocalMoveX(posEnd.x, 7.5f).OnComplete(() =>
            {

                isRunning = false;
                DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
                {
                    showAlertMessage();
                });

            });
        }
        else
        {
            DOTween.Kill(lbAlert.transform);
            isRunning = false;
            gameObject.SetActive(false);
        }
    }

}
