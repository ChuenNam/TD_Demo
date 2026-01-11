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
    public RestaurantPanel restaurantPanel;

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
