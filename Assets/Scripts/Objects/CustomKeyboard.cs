using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomKeyboard : MonoBehaviour
{
    [Serializable]
    public struct Key
    {
        public Button m_KeyBtn;
        public TextMeshProUGUI m_KeyTMPUI;
        public Key SetKeyText(string text) { m_KeyTMPUI.SetText(text); return this; }
        public Key SetOnClickCb(UnityAction cb) { m_KeyBtn.onClick.AddListener(cb); return this; }
    }
    [SerializeField] private List<Key> m_KeyKs;
    [SerializeField] private Key m_ToUpperK, m_SymbolsK, m_SpaceK, m_DeleteK, m_EnterK;
    [SerializeField] private TextMeshProUGUI m_PreviewTextTMPUI, m_PreviewPasswordTMPUI;
    private List<string> _NormalKeys = new() {
        "q", "w", "e", "r", "t", "y", "u", "i", "o", "p",
        "a", "s", "d", "f", "g", "h", "j", "k", "l",
        "z", "x", "c", "v", "b", "n", "m",
        "[", "(", ",", ".", ")", "]"
    },
    _SymbolsAndNumbers = new()
    {
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "!", "@", "#", "|", "%", "^", "&", "-", "+",
        "~", "\\", "/", ":", ";", "?", "*",
        "_", "<", "{", "}", ">", "="
    };
    private Transform _ButtonsTf;
    private TMP_InputField _TargetIF;
    private bool _IsUpperCase, _IsSymbolsAndNumbers;
    #region Button
    public void DoClickCancelTyping()
    {
        StartCoroutine(hideKeyboard());
        IEnumerator hideKeyboard()
        {
            _ButtonsTf.GetChild(3).DOLocalMoveY(-250f, .1f).SetEase(Ease.InQuad);
            yield return new WaitForSeconds(.025f);
            _ButtonsTf.GetChild(2).DOLocalMoveY(-325f, .1f).SetEase(Ease.InQuad);
            yield return new WaitForSeconds(.025f);
            _ButtonsTf.GetChild(1).DOLocalMoveY(-400f, .1f).SetEase(Ease.InQuad);
            yield return new WaitForSeconds(.025f);
            _ButtonsTf.GetChild(0).DOLocalMoveY(-475f, .1f).SetEase(Ease.InQuad).OnComplete(() => gameObject.SetActive(false));
        }
    }
    #endregion

    public void Show(TMP_InputField inputIF, bool isPassword = false)
    {
        gameObject.SetActive(true);
        _TargetIF = inputIF;
        m_PreviewTextTMPUI.text = _TargetIF.text;
        m_PreviewPasswordTMPUI.text = "";
        for (int i = 0; i < m_PreviewTextTMPUI.text.Length; i++) m_PreviewPasswordTMPUI.text += "*";
        m_PreviewTextTMPUI.gameObject.SetActive(!isPassword);
        m_PreviewPasswordTMPUI.gameObject.SetActive(isPassword);
        _IsUpperCase = _IsSymbolsAndNumbers = false;
        m_SymbolsK.SetKeyText("!#1");
        for (int i = 0; i < m_KeyKs.Count; i++) m_KeyKs[i].SetKeyText(_NormalKeys[i]);
        StartCoroutine(showKeyboard());
        IEnumerator showKeyboard()
        {
            _ButtonsTf.GetChild(0).DOLocalMoveY(132.5f, .1f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(.025f);
            _ButtonsTf.GetChild(1).DOLocalMoveY(57.5f, .1f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(.025f);
            _ButtonsTf.GetChild(2).DOLocalMoveY(-17.5f, .1f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(.025f);
            _ButtonsTf.GetChild(3).DOLocalMoveY(-92.5f, .1f).SetEase(Ease.OutQuad);
        }
    }
    private void _TypeAKey(string text)
    {
        m_PreviewTextTMPUI.text += text;
        m_PreviewPasswordTMPUI.text += "*";
    }
    private void _DeleteLastKey()
    {
        if (m_PreviewTextTMPUI.text.Length <= 0) return;
        int removedId = m_PreviewTextTMPUI.text.Length - 1;
        m_PreviewTextTMPUI.text = m_PreviewTextTMPUI.text[..removedId];
        m_PreviewPasswordTMPUI.text = m_PreviewPasswordTMPUI.text[..removedId];
    }

    void Awake()
    {
        _ButtonsTf = m_KeyKs[0].m_KeyBtn.transform.parent.parent;
        for (int i = 0; i < m_KeyKs.Count; i++)
        {
            int keyId = i;
            m_KeyKs[i].SetKeyText(_NormalKeys[i]).SetOnClickCb(() => { _TypeAKey(m_KeyKs[keyId].m_KeyTMPUI.text); });
        }
        m_ToUpperK.SetKeyText("↑").SetOnClickCb(() =>
        {
            if (_IsSymbolsAndNumbers) return;
            _IsUpperCase = !_IsUpperCase;
            foreach (Key key in m_KeyKs) key.SetKeyText(_IsUpperCase ? key.m_KeyTMPUI.text.ToUpper() : key.m_KeyTMPUI.text.ToLower());
        });
        m_DeleteK.SetOnClickCb(() => { _DeleteLastKey(); });
        m_SpaceK.SetOnClickCb(() => { _TypeAKey(" "); });
        m_SymbolsK.SetKeyText("!#1").SetOnClickCb(() =>
        {
            _IsSymbolsAndNumbers = !_IsSymbolsAndNumbers;
            m_SymbolsK.SetKeyText(_IsSymbolsAndNumbers ? "abc" : "!#1");
            if (_IsSymbolsAndNumbers)
                for (int i = 0; i < m_KeyKs.Count; i++) m_KeyKs[i].SetKeyText(_SymbolsAndNumbers[i]);
            else
                for (int i = 0; i < m_KeyKs.Count; i++) m_KeyKs[i].SetKeyText(_NormalKeys[i]);
        });
        m_EnterK.SetOnClickCb(() =>
        {
            _TargetIF.text = m_PreviewTextTMPUI.text;
            DoClickCancelTyping();
        });
    }
}
