using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlterHero : MonoBehaviour
{
    [SerializeField] private Hero hero;
    [SerializeField] private SpriteRenderer sprite;
    private SpriteRenderer _heroSprite;

    public Vector3 axis;
    
    public bool flipX, flipY;

    public static Vector3 position;
    
    private void Start()
    {
        _heroSprite = hero.sprite;
    }

    private void LateUpdate()
    {
        var x = _heroSprite.flipX;

        sprite.sprite = _heroSprite.sprite;
        sprite.flipX = flipX ? !x : x;
        sprite.flipY = flipY;

        Vector3 hPos = hero.transform.position;
        
        if (flipX)
        {
            transform.position = position = new Vector3
            (
             axis.x - hPos.x + axis.x,
                hPos.y,
                hPos.z
            );
        }
    }
}
