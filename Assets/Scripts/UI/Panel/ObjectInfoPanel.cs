using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectInfoPanel : BasePanel
{
    [SerializeField]private GridManager gridManager;
    public GridObjectData data;
    
    public Text nameInfo;
    public Text descriptionText;
    public Text productInfoText;
    public Button productButton;
    public Image productButtonImage;
    public Text productButtonText;
    public Button chooseBPButton;
    public BpListPanel blueprintListPanel;
    
    protected override void Init()
    {
        base.Init();
        closeButton.onClick.AddListener(()=> gridManager.currentObject = null);
        productButton.onClick.AddListener(() =>
        {
            var building = data.instance.GetComponent<Building>();
            building.inProduction = !building.inProduction;
            WriteInfo(data);
        });
        chooseBPButton.onClick.AddListener(() =>
        {
            if (!blueprintListPanel.isActive)
            {
                blueprintListPanel.ShowPanel();
                blueprintListPanel.InitBpInfo(data,data.instance.GetComponent<Building>());
            }
            else
            {
                blueprintListPanel.ClosePanel();
                blueprintListPanel.building = null;
            }
        });
    }

    public void WriteInfo(GridObjectData objectData)
    {
        this.data = objectData;
        nameInfo.text = objectData.name;
        descriptionText.text = data.description;
        
        var building = data.instance.GetComponent<Building>();
        productInfoText.text = building.inProduction ? "生产中：\n" + building.CurrentBlueprint.Info(): "休息中";
        productButtonImage.color = building.inProduction ? 
            new Color(1f,.7f,.7f) : new Color(.7f,1f,.7f);
        productButtonText.text = building.inProduction ? "停止制造" : "开始制造";
    }
    
}
