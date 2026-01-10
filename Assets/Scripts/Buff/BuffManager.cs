using UnityEngine;
using System;
using System.Collections.Generic;


public static class BuffManager/* : MonoBehaviour*/
{
    /*#region 单例 instance
        public static BuffManager instance;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }
    #endregion*/
    

    // 生产效率加倍 Buff
    public static Buff CreatMultipleProductivityBuff(Building building, float multiple, int duration)
    {
        var bp = building.CurrentBlueprint;
        Action onAddBuff = () =>
        {
            bp.timeMultiplier *= multiple;
        };
        Action onDelBuff = () =>
        {
            bp.timeMultiplier /= multiple;
        };

        var buff = new Buff(duration, onAddBuff, onDelBuff);
        return buff;
    }
    
}
