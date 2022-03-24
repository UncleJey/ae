using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PCBase : MonoBehaviour
{
    private static readonly int MoveName = Animator.StringToHash("move");

    [SerializeField] protected SpriteRenderer sprite;
    [SerializeField] protected float speed = 1f;
    [SerializeField] protected Animator animator;

    private bool _lookLeft = false;

    public bool Rotate
    {
        get => _lookLeft;
        set
        {
            if (_lookLeft != value)
            {
                _lookLeft = value;
                sprite.flipX = value;
            }
        }
    }
    

    public virtual void Move(int moveX, int moveY)
    {
        animator.SetBool(MoveName, moveX != 0);

        if (moveX > 0)
        { 
            transform.localPosition += Vector3.right * (Time.deltaTime * speed);
            Rotate = false;
        }
        else if (moveX < 0)
        {
            transform.localPosition += Vector3.left * (Time.deltaTime * speed);
            Rotate = true;
        }
        else if (moveY > 0)
        {
            transform.localPosition += Vector3.down * (Time.deltaTime * speed);
        }
        else if (moveY < 0)
        {
            transform.localPosition += Vector3.up * (Time.deltaTime * speed);
        }
    }
}