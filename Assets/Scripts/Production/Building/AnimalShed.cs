using UnityEngine;

public class AnimalShed : Building ,IDayNightBonus
{
    [Header("Buff数据")] 
    public float daytimeBonus = 1;
    public float nighttimeBonus = 1;
    
    public float GetDayBonus() => daytimeBonus;
    public float GetNightBonus()  => nighttimeBonus;

}