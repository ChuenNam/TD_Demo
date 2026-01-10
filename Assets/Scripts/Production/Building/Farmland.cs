using UnityEngine;

public class Farmland : Building ,IDayNightBonus
{
    [Header("Buff数据")] 
    public float daytimeBonus;
    public float nighttimeBonus;
    
    public float GetDayBonus() => daytimeBonus;
    public float GetNightBonus()  => nighttimeBonus;

}
