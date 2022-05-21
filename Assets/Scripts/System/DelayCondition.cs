/// <summary>
/// Отложенный запуск
/// </summary>
public class DelayCondition : Delay.Condition
{
    public override bool IsValid => Clocks.time >= StartTime;

    public override void Subscribe()
    {
        Clocks.OnTick += CheckCondition;
    }

    public override void Unsubscribe()
    {
        Clocks.OnTick -= CheckCondition;
    }

    void CheckCondition()
    {
        if (IsValid)
        {
            CallConditionModified();
        }
    }

    /// <summary>
    /// время запуска
    /// </summary>
    public long StartTime;
}