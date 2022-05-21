using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions 
{
    public static T Get<T>(this IList<T> _list, int _index, T _def = default(T))
    {
        if (_list == null || (uint)_index >= (uint)_list.Count)
            return _def;
        return _list[_index];
    }

}
