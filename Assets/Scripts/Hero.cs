using UnityEngine;
using UnityEngine.Tilemaps;

public class Hero : PCBase
{
    [SerializeField] Vector3 dn = new Vector3(0, -0.03f, 0);
    [SerializeField] Vector3 up = new Vector3(0, 0.35f, 0);
    [SerializeField] Vector3 lf = new Vector3(-0.2f, 0.2f, 0);
    [SerializeField] Vector3 rt = new Vector3(0.2f, 0.2f, 0);

    public override void Move(int moveX, int moveY)
    {
        TileBase leftCell, rightCell, vertCell = null;
        if (moveX > 0)
        {
            rightCell = Map.GetCell(transform.position + rt);
            if (!Map.Walkable(rightCell))
                moveX = 0;
        }
        else if (moveX < 0)
        {
            leftCell = Map.GetCell(transform.position + lf);
            if (!Map.Walkable(leftCell))
                moveX = 0;
        }
        else if (moveY < 0)
        {
            vertCell = Map.GetCell(transform.position + up);
            if (!Map.Walkable(vertCell, false))
                moveY = 0;
        }
        else if (moveY > 0)
        {
            vertCell = Map.GetCell(transform.position + dn);
            if (!Map.Walkable(vertCell, false))
                moveY = 0;
        }

        base.Move(moveX, moveY);
    }
#if UNITY_EDITOR
    public void FixedUpdate()
    {
        Debug.DrawLine(transform.position + dn, transform.position + up, Color.red);
        Debug.DrawLine(transform.position + lf, transform.position + rt, Color.red);
    }
#endif
}