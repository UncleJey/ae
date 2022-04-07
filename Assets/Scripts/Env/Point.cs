using System;
using UnityEngine;

public class Point : MonoBehaviour, ITouchReceiver
{
    /// <summary>
    /// Эффект при цничтожении
    /// </summary>
    [SerializeField] private GameObject effectPrefab;

    /// <summary>
    /// Время эффекта
    /// </summary>
    [SerializeField] private float time;

    /// <summary>
    /// С кем может взаимодействовать
    /// </summary>
    [SerializeField] private CanTouch touch;

    private void Start()
    {
        GameController.Quads++;
    }

    /// <summary>
    /// Взаимодействие
    /// </summary>
    /// <param name="isHero"></param>
    public void TouchAction(bool isHero)
    {
        if ((isHero && touch == CanTouch.Hero) || (!isHero && touch == CanTouch.Alter))
        {
            GameController.Quads--;
            CGIController.PlayEffect(effectPrefab, transform.position, time);
            Map.DestoryObj(this.gameObject);
        }
    }
}