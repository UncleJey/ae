using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hero : PCBase
{
    [SerializeField] Vector3 dn = new Vector3(0, -0.03f, 0);
    [SerializeField] Vector3 up = new Vector3(0, 0.35f, 0);
    [SerializeField] Vector3 lf = new Vector3(-0.2f, 0.2f, 0);
    [SerializeField] Vector3 rt = new Vector3(0.2f, 0.2f, 0);
    [SerializeField] Vector3 center = new Vector3(0,0,0);
    
    Vector3 delta = new Vector3(-0.15f, 0, 0);

    public Vector3 CurrentCenter => transform.position + center;
    
    public Vector3 position
    {
        get => transform.position + delta;
        set => transform.position = value - delta;
    }

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        position = Map.Trim(position);
    }

    /// <summary>
    /// В какой последней ячейке продолжалось падение
    /// </summary>
    private Vector3Int _fallFlow = Vector3Int.zero;
    public override void Move(int moveX, int moveY, float pSpeed = 0, bool pFall = false)
    {
        if (_flipping)
            return;
        
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
            pFall = true;
            _fallFlow = cell;
        }
        else
        {
            if (!_fallFlow.Equals(Vector3Int.zero)) // выравниваем позицию на платформе после падения
                position = Map.CellToWorld(_fallFlow);
            _fallFlow = Vector3Int.zero;

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
        base.Move(moveX, moveY, actionSpeed, pFall);
    }

    /// <summary>
    /// В настоящий момент происходит обмен телами
    /// </summary>
    private bool _flipping = false;

    /// <summary>
    /// Когда последний раз был обмен
    /// </summary>
    private float lastFlipTime = 0;
    
    /// <summary>
    /// Процесс обмена телами
    /// </summary>
    IEnumerator FlipToPos(Vector3 pPosition)
    {
        Time.timeScale = 0.1f;
//        bool mirrored = false;
        while ((transform.position - pPosition).sqrMagnitude > 0.01f)
        {
            /* не работает по какой-то причине
            if (!mirrored && (transform.position - AlterHero.position).sqrMagnitude < 1.5f)
            {
                Rotate = !Rotate;
                mirrored = true;
            }
            */
            if (transform == null)
            {
                Time.timeScale = 1;
                _flipping = false;
                yield break;                
            }
            transform.position = Vector3.MoveTowards(transform.position, pPosition, Time.deltaTime * 50 * speed);
            yield return null;
        }
        yield return null;
        transform.position = pPosition;
        Time.timeScale = 1;
        _flipping = false;
    }

    /// <summary>
    /// Может ли обменяться телами если алтер находится в этом тайле
    /// </summary>
    bool CanFlip()
    {
        Map.GetCell(AlterHero.position + center, out var tb, out var cell);
        return Map.Walkable(tb);
    }
    
    /// <summary>
    /// Обменяться телами
    /// </summary>
    public void Flip()
    {
        if (_flipping || !CanFlip())
            return;

        if (Time.time - lastFlipTime < 0.5f) // не меняться слишком часто
            return;

        lastFlipTime = Time.time;
        
        _flipping = true;
        StartCoroutine(FlipToPos(AlterHero.position));
    }

#if UNITY_EDITOR
    public void FixedUpdate()
    {
        Debug.DrawLine(transform.position + dn, transform.position + up, Color.red);
        Debug.DrawLine(transform.position + lf, transform.position + rt, Color.blue);

        Debug.DrawLine(transform.position + center + new Vector3(-0.01f, -0.01f), transform.position + center + new Vector3(0.01f, 0.01f), Color.black);
        Debug.DrawLine(transform.position + center + new Vector3(0.01f, -0.01f), transform.position + center + new Vector3(-0.01f, 0.01f), Color.black);
    }
#endif
}