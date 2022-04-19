using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private Text worldText, levelText, livesText, quadsText, turnsText;
    public Hero hero;

    public static Hero Hero;

    private static int _world=1, _level=1, _lives=3, _quads = 2, _turns=3;

    private static GameController instance;

    /// <summary>
    /// Сколько квадратов на поле осталось собрать
    /// </summary>
    public static int Quads
    {
        get => _quads;
        set
        {
            _quads = value;
            instance.quadsText.text = "Quads: "+value;
        }
    }

    /// <summary>
    /// Сколько перемещений осталось
    /// </summary>
    public static int Turns
    {
        get => _turns;
        set
        {
            _turns = value;
            instance.turnsText.text = "Turns: " + value;
            TurnsSwitched();
        }
    }

    static async void TurnsSwitched()
    {
        instance.turnsText.color = Color.green;
        await Task.Delay(300);
        instance.turnsText.color = Color.white;
    }
    
    public static async void HighLightTurns()
    {
        for (int i = 0; i < 3; i++)
        {
            instance.turnsText.color = Color.red;
            await Task.Delay(100);
            instance.turnsText.color = Color.white;
            await Task.Delay(100);
        }
    }
    
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.maxQueuedFrames = 3;
        Hero = hero;
    }

    public static void DieByEnemy(PCBase pToucher)
    {
        
    }

}