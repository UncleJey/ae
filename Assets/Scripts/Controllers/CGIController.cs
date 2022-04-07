using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CGIController : MonoBehaviour
{
    /// <summary>
    /// Проиграть эффект
    /// </summary>
    public static void PlayEffect(GameObject obj, Vector3 pos, float time)
    {
        effector(obj, pos, time);
    }

    static async void effector(GameObject obj, Vector3 pos, float time)
    {
        GameObject o = Instantiate(obj, Map.MapTransform, true);
        o.transform.position = pos;
        await Task.Delay((int) (1000f * time));
        GameObject.Destroy(o);
    }
    
}