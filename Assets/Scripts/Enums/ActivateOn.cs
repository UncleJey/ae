/// <summary>
/// В который момент активировать объект на карте
/// </summary>
public enum ActivateOn : byte
{
    None = 0,
    OnStart = 1,
    OnStepIn = 2,
    OnStepOut = 3,
    Hero = 4
}