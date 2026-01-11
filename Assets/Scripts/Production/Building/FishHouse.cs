using UnityEngine;

public class FishHouse : Building ,IDayNightBonus
{
    [Header("Buff数据")] 
    public float daytimeBonus = 1;
    public float nighttimeBonus = 1;
    
    public float GetDayBonus() => daytimeBonus;
    public float GetNightBonus()  => nighttimeBonus;

}