using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public HelpPanel helpPanel;
    public ObjectInfoPanel objectInfoPanel;
    public ItemPanel itemPanel;
    public EventChosePanel eventChosePanel;
    public BpListPanel bpListPanel;
    public RestaurantPanel restaurantPanel;
    public TradePanel tradePanel;
    public TaskPanel taskPanel;

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
}
