/// <summary>
/// ВЗаимодействие с героем
/// </summary>
public interface ITouchReceiver
{
    /// <summary>
    /// Запустить взаимодействие
    /// </summary>
    /// <param name="isHero">Герой или альтер</param>
    void TouchAction(bool isHero);
}