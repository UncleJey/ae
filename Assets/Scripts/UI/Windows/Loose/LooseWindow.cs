using UnityEngine;
using UnityEngine.UI;

public class LooseWindow : WindowBase
{
    [SerializeField] private Text retryStr;
    [SerializeField] private Image videoSprite;
    [SerializeField] private Button btnExit, btnRetry;
    [SerializeField] private Sprite freeSprite, paidSprite;
    public override void OnOpen()
    {
        base.OnOpen();
        int turns = GameController.Turns;
        if (turns > 0)
        {
            retryStr.text = $"Continue ({turns})";
            videoSprite.sprite = freeSprite;
        }
        else
        {
            retryStr.text = "Continue";
            videoSprite.sprite = paidSprite;
        }
        
        btnRetry.onClick.AddListener(() =>
        {
            
        });
        
        btnExit.onClick.AddListener(() =>
        {
            
        });
        
    }

    public override void OnClose()
    {
        base.OnClose();
        btnExit.onClick.RemoveAllListeners();
        btnRetry.onClick.RemoveAllListeners();
    }
}