using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GUIElement<TElementType> : GUIElementBase
    where TElementType : GUIElement<TElementType>
{
    public static event Action<TElementType> OnOpened;
    public static event Action<TElementType> OnClosed;

    public static readonly Dictionary<Type, TElementType> Existing = new Dictionary<Type, TElementType>();
    public static readonly List<TElementType> Opened = new List<TElementType>();

    public static bool IsInitializing { get; private set; } = true;

    public static Coroutine InitElements( MonoBehaviour _parent )
    {
        return _parent.StartCoroutine(Initor(_parent));
    }

    private static IEnumerator Initor(MonoBehaviour _parent)
    {
        IsInitializing = true;
        var children = _parent.GetComponentsInChildren<TElementType>(true);
        foreach (var elem in children)
        {
            var type = elem.GetType();
#if UNITY_EDITOR || DEBUG
            if (Existing.ContainsKey(type))
            {
                TElementType wnd = Existing[type];
                Debug.LogError( "Window with type " + type + " already exists (" + wnd.name + "). Replacing with "+elem.name );
                Existing.Remove( type );
            }
#endif
            Existing.Add( type, elem );
            elem.Initialize();
            elem.gameObject.SetActive(true);
        }

        yield return null;
        foreach (var elem in children)
        {
            if (!Opened.Contains(elem))
                elem.gameObject.SetActive(false);           
        }

        IsInitializing = false;
    }

    public static T GetElement<T>()
        where T : TElementType
    {
        return (T) GetElement(typeof(T));
    }

    private static TElementType GetElement( Type type )
    {
        if (!Existing.TryGetValue(type, out var elem))
        {
#if UNITY_EDITOR || DEBUG
            Debug.LogError("GUI Element of type " + type.Name + " not found!");
#endif
        }
        return elem;
    }

    protected virtual void Initialize()
    {
        
    }

    protected void MarkOpened()
    {
        AddOpened();
        CallOpened();
    }

    protected void MarkClosed()
    {
        AddClosed();
        CallClosed();
    }

    protected bool AddOpened()
    {
        var self = (TElementType)this;
        if (!Opened.Contains(self))
        {
            Opened.Add(self);
#if DEBUG_UI
            Debug.Log("Opened: " + name + " (" + GetType() + ")");
#endif
            return true;
        }
        return false;
    }

    protected bool AddClosed()
    {
        var self = (TElementType)this;
        if (Opened.Remove(self))
        {
#if DEBUG_UI
            Debug.Log("Closed: " + name + " (" + GetType() + ")");
#endif
            return true;
        }
        return false;
    }

    protected void CallOpened()
    {
        var self = (TElementType)this;
        onThisOpened?.Invoke();
        OnOpened?.Invoke(self);
    }

    protected void CallClosed()
    {
        var self = (TElementType)this;
        onThisClosed?.Invoke();
        OnClosed?.Invoke(self);
    }
}

public abstract class GUIElementBase : MonoBehaviour
{
    protected Action onThisOpened;
    protected Action onThisClosed;

    public event Action OnThisOpened
    {
        add { onThisOpened += value; }
        remove { onThisOpened -= value; }
    }
    public event Action OnThisClosed
    {
        add { onThisClosed += value; }
        remove { onThisClosed -= value; }
    }
}
