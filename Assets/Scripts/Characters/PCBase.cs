using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class PCBase : MonoBehaviour
{
    private static readonly int MoveName = Animator.StringToHash("move");
    private static readonly int SideUp = Animator.StringToHash("up");
    private static readonly int Attack = Animator.StringToHash("attack");
    private static readonly int Die = Animator.StringToHash("die");

    public SpriteRenderer sprite;
    [SerializeField] protected float speed = 1f;
    [SerializeField] protected Animator animator;
    [SerializeField] protected bool flipped = false;

    protected bool canMove = true;
    
    public bool Rotate
    {
        get => sprite.flipX;
        set
        {
            if (flipped)
                value = !value;

            if (sprite.flipX != value)
                sprite.flipX = value;
        }
    }

    /// <summary>
    /// Атакует
    /// </summary>
    protected bool Atack
    {
        get => animator.GetBool(Attack);
        set
        {
            animator.SetBool(Attack, value);
            canMove = !value;
        }
    }

    /// <summary>
    /// Мёртв ли
    /// </summary>
    public bool Dead
    {
        get => animator.GetBool(Die);
        set
        {
            canMove = !value;
            animator.SetBool(Die, value);
        }
    }
    
    public virtual void Move(int moveX, int moveY, float pSpeed = 0, bool pFall = false)
    {
        if (!canMove)
            return;
        
        if (pSpeed < 0.01f && pSpeed > -0.01f)
            pSpeed = speed;

        animator.SetBool(MoveName, moveX != 0 || moveY != 0);

        if (moveX != 0 || moveY != 0)
            animator.SetBool(SideUp, moveX == 0 && moveY != 0 && !pFall);

        if (moveX > 0)
        {
            transform.localPosition += Vector3.right * (Time.deltaTime * pSpeed);
            Rotate = false;
        }
        else if (moveX < 0)
        {
            transform.localPosition += Vector3.left * (Time.deltaTime * pSpeed);
            Rotate = true;
        }
        else if (moveY > 0)
        {
            transform.localPosition += Vector3.down * (Time.deltaTime * pSpeed);
        }
        else if (moveY < 0)
        {
            transform.localPosition += Vector3.up * (Time.deltaTime * pSpeed);
        }
    }
}