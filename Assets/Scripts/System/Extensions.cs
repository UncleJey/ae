using System;
using System.Collections;
using System.Collections.Generic;

public static class Extensions 
{
    public static T Get<T>(this IList<T> _list, int _index, T _def = default(T))
    {
        if (_list == null || (uint)_index >= (uint)_list.Count)
            return _def;
        return _list[_index];
    }


    /// <summary>
    /// Вызывает делегат с предварительной проверкой на null, а так же try/except.
    /// Возвращает null если не было ошибок
    /// </summary>
    public static string TryExecute(this Action _action)
    {
        if (_action == null)
            return null;

        string errorMessage = null;
        Delegate[] list = _action.GetInvocationList();
        for (int i = 0; i < list.Length; i++)
        {
            try { list[i].DynamicInvoke(); }
            catch (Exception _e)
            {
                if (errorMessage == null)
                    errorMessage = _e.ToString();
                else
                    errorMessage = "\n" + _e.ToString();
            }
        }
        return errorMessage;
    }

    public static int IndexOf<T>(this IList<T> _array, T _value)
    {
        if (_array == null)
            return -1;
        for (int i = _array.Count - 1; i >= 0; i--)
        {
            if (Equals(_array[i], _value))
                return i;
        }
        return -1;
    }

    public static int IndexOf<T>(this IList<T> _array, Func<T, bool> _value)
    {
        if (_array == null)
            return -1;
        for (int i = _array.Count - 1; i >= 0; i--)
        {
            if (_value(_array[i]))
                return i;
        }
        return -1;
    }


    public static bool IsInRange(this IList _list, int _index)
    {
        return _list != null && ((uint)_index) < _list.Count;
    }

    public static bool IsInRange<T>(this T[,] _array, int _pos1, int _pos2)
    {
        return _array != null && ((uint)_pos1) < _array.GetLength(0) && ((uint)_pos2) < _array.GetLength(1);
    }


    public static void Execute(this Action _action)
    {
        _action?.Invoke();
    }

}
