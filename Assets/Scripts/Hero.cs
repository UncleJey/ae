using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hero : PCBase
{
    [SerializeField] Vector3 dn = new Vector3(0, -0.03f, 0);
    [SerializeField] Vector3 up = new Vector3(0, 0.35f, 0);
    [SerializeField] Vector3 lf = new Vector3(-0.2f, 0.2f, 0);
    [SerializeField] Vector3 rt = new Vector3(0.2f, 0.2f, 0);

    Vector3 delta = new Vector3(-0.15f, 0, 0);

    public Vector3 position
    {
        get => transform.position + delta;
        set => transform.position = value - delta;
    }

    private void Start()
    {
        position = Map.Trim(position);
        Debug.Log(Map.CellSize.x);
    }

    public override void Move(int moveX, int moveY, float pSpeed = 0)
    {
        TileBase tb = null;
        Vector3Int cell = Vector3Int.zero;
        if (pSpeed < 0.01f && pSpeed > -0.01f)
            pSpeed = speed;
        
        float actionSpeed = pSpeed;
        
        Map.GetCell(transform.position + dn, out tb, out cell);
        if (tb == null) // падаем
        {
            moveY = 1;
            moveX = 0;
            actionSpeed = pSpeed + pSpeed;
        }
        else
        {
            if (moveX > 0)
            {
                Map.GetCell(transform.position + rt, out tb, out cell);
                if (!Map.Walkable(tb))
                    moveX = 0;
            }
            else if (moveX < 0)
            {
                Map.GetCell(transform.position + lf, out tb, out cell);
                if (!Map.Walkable(tb))
                    moveX = 0;
            }
            else if (moveY < 0)
            {
                Map.GetCell(transform.position + up, out tb, out cell);
                if (!Map.Walkable(tb, false))
                    moveY = 0;
            }
            else if (moveY > 0)
            {
                Map.GetCell(transform.position + dn, out tb, out cell);
                if (!Map.Walkable(tb, false))
                    moveY = 0;
            }
        }

        // поправка позиции при переходе с вертикального перемещения на горизонтальное
        if (moveY == 0 && moveX != 0)
        {
            Vector3 pos = position;
            Vector3 cellpos = Map.CellToWorld(cell);

            float deltaY = pos.y - cellpos.y;
            if (deltaY > 0.04f)
            {
                moveY = 1;
                moveX = 0;
            }
            else if (deltaY < -0.04f)
            {
                moveY = -1;
                moveX = 0;
            }
            else
                position = new Vector3(pos.x, cellpos.y);
        }
        // поправка позиции при переходе с горизонтального перемещения на вертикальное
        else if (moveX == 0 && moveY != 0)
        {
            Vector3 pos = position;
            Vector3 cellpos = Map.CellToWorld(cell);

            float deltaX = pos.x - cellpos.x;
            if (deltaX > 0.04f)
                moveX = -1;
            else if (deltaX < -0.04f)
                moveX = 1;
            else
                position = new Vector3(cellpos.x, pos.y);
        }
        base.Move(moveX, moveY, actionSpeed);
    }

#if UNITY_EDITOR
    public void FixedUpdate()
    {
        Debug.DrawLine(transform.position + dn, transform.position + up, Color.red);
        Debug.DrawLine(transform.position + lf, transform.position + rt, Color.red);
    }
#endif
}