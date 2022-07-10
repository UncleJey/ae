using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    [SerializeField] private Hero hero;

    [SerializeField] private Button flipButton;
    
    // Start is called before the first frame update
    Vector3 _mousePos = Vector3.zero;

    float scale = 400;
    float spriteIntervalChange = 0.01f;
    float _spriteIntervalChange = 0;

    float h, v;

    bool _haveSword = false;
    int _hh = 0;
    int _vv = 0;
    bool _space = false;

    private static readonly KeyCode[] Joystics = new KeyCode[]
    {
        KeyCode.JoystickButton0 // треугольник
        ,
        KeyCode.JoystickButton1 // круг
        ,
        KeyCode.JoystickButton2 // квадрат
        ,
        KeyCode.JoystickButton3 // крестик
        ,
        KeyCode.JoystickButton4 // LD
        ,
        KeyCode.JoystickButton5 // RD
        ,
        KeyCode.JoystickButton6 // LU
        ,
        KeyCode.JoystickButton7 // RU
        ,
        KeyCode.JoystickButton8 // select
        ,
        KeyCode.JoystickButton9 // start
        ,
        KeyCode.JoystickButton10 // Left pad Click
        ,
        KeyCode.JoystickButton11 // Right pad Click
        ,
        KeyCode.Space // Пробел
    };

    private void Awake()
    {
        flipButton.onClick.AddListener(()=>hero.Flip());
    }

    private void OnDestroy()
    {
        flipButton.onClick.RemoveAllListeners();
    }

    void CheckMove()
    {
        if (Input.GetMouseButtonDown(0))
            _mousePos = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {
            _hh = 0;
            _vv = 0;
        }

        if (Input.GetMouseButton(0)) // GetMouseButtonUp(0))
        {
            float dx = Input.mousePosition.x - _mousePos.x;
            float dy = Input.mousePosition.y - _mousePos.y;

            float adx = dx < 0 ? 0 - dx : dx;
            float ady = dy < 0 ? 0 - dy : dy;

            if (adx + ady > 25)
            {
                _mousePos = Input.mousePosition;
                if (adx > ady)
                {
                    if (dx > 0)
                        _hh = 1; //moveRight();
                    else
                        _hh = -1; //moveLeft();
                }
                else
                {
                    if (dy < 0)
                        _vv = -1; //moveBack();
                    else
                        _vv = 1; //	boost();
                }
            }
        }
    }

    void Update()
    {
        foreach (KeyCode c in Joystics)
            if (Input.GetKeyDown(c))
                _space = true;

        if (_space)
        {
            hero.Flip();
            return;
        }

        _space = false;

        h = Input.GetAxisRaw("Horizontal"); // left -1 right +1
        v = Input.GetAxisRaw("Vertical"); // up + 1 down -1
            if (v.NearZero() && h.NearZero())
        {
            CheckMove();
            h = _hh;
            v = _vv;
            if (v.NearZero() && h.NearZero())
            {
                hero.Move(0, 0);
                return;
            }
        }

        if (h > 0)
            hero.Move(1, 0);
        else if (h < 0)
            hero.Move(-1, 0);
        else if (v > 0)
            hero.Move(0, -1);
        else if (v < 0)
            hero.Move(0, 1);
        else
            hero.Move(0, 0);
    }
}