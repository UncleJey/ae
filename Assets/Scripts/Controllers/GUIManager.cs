using System;
using System.Collections;
using System.Collections.Generic;
using Foranj.SDK.GUI;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    public static RectTransform RectTransform;
    public static Canvas Canvas;
    public static Camera canvasCamera;
    private static GUIManager _instance;

    public static event Action<WindowBase> OnWindowOpened
    {
        add => WindowBase.OnOpened += value;
        remove => WindowBase.OnOpened -= value;
    }

    private void Awake()
    {
        _instance = this;
        StartCoroutine(Init());
    }

    protected virtual IEnumerator Init()
    {
        Canvas = GetComponent<Canvas>();
        RectTransform = (RectTransform) Canvas.transform;
        Canvas.enabled = false;
        if (canvasCamera == null)
            canvasCamera = Canvas.worldCamera;

        yield return GUIElement<WindowBase>.InitElements(this);
        yield return new WaitForSeconds(0.1f);
        Canvas.enabled = true;
//        OnRectTransformDimensionsChange();
    }


    public static void UIReset(WindowBase pWindow)
    {
    }

    public static TWindowBase GetWindow<TWindowBase>() where TWindowBase : WindowBase
    {
        return GUIElement<WindowBase>.GetElement<TWindowBase>();
    }

    public static void CloseAllWindows(bool _forced = false)
    {
        List<WindowBase> lWnd = GUIElement<WindowBase>.Opened;
        for (int i = lWnd.Count - 1; i >= 0; i--)
        {
            //Используем Get на случай, если окно само закроет что-то и список изменится
            var wnd = lWnd.Get(i);
            if (wnd != null && wnd.IsOpened)
            {
                wnd.CloseAll();
            }
        }
    }

    public static void UIShow(WindowBase pWindow, string[] uiParts)
    {
    }

    public static void UIHideAll(WindowBase pWindow, string[] uiParts)
    {
    }
}