using Foranj.SDK.GUI;
using UnityEngine;
using UnityEngine.UI;

public class LooseWindow : WindowBase
{
    [SerializeField] private Text retryStr;
    [SerializeField] private GameObject videoSprite;
    [SerializeField] private Button btnExit, btnRetry;
    
    public override void OnOpen()
    {
        base.OnOpen();
        int turns = GameController.Turns;
        if (turns > 0)
        {
            retryStr.text = $"Retry ({turns})";
            videoSprite.gameObject.SetActive(false);
        }
        else
        {
            retryStr.text = "Retry";
            videoSprite.gameObject.SetActive(true);
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