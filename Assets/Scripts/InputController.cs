using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField]
    private Hero hero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            hero.Move(1, 0);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            hero.Move(-1, 0);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            hero.Move(0, -1);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            hero.Move(0, 1);
        }
        else
        {
            hero.Move(0,0);
        }


    }
}
