using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectInfoPanel : BasePanel
{
    public GridObjectData data;
    
    public Text nameInfo;
    public Text descriptionText;
    
    protected override void Init()
    {
        base.Init();
        //targetPanel = gameObject;
    }

    public void WriteInfo(GridObjectData objectData)
    {
        this.data = objectData;
        nameInfo.text = objectData.name;
        descriptionText.text = data.description;
    }
    
}
