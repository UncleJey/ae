using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : PCBase, ITouchReceiver
{
    [SerializeField] private int direction = 1;
    // Update is called once per frame
    void Update()
    {

        // если пола нет
        Vector3 pnt = new Vector3(direction > 0 ? 0.15f : -0.15f, -0.03f);
        if (!Map.GetCell(transform.position + pnt, out var tb, out var cell) )
        {
            direction = -direction;
            return;
        }
        
        // если впереди препятствие
        Vector3 pnt2 = new Vector3(direction > 0 ? 0.25f : -0.25f, 0.2f);
        if (!Map.Walkable(transform.position + pnt2))
        {
            direction = -direction;
            return;
        }

        Move(direction, 0);
        Rotate = direction > 0;
    }

    public void TouchAction(bool isHero)
    {
        GameController.DieByEnemy(this);
    }

}