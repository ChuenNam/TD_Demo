using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemGroup
{
    public BaseItem item;
    public int count;
}

[Serializable]
public class Blueprint
{
    public bool isLocked;
    public float time;
    public List<ItemGroup> useGroup;
    public List<ItemGroup> productGroup;
}

[CreateAssetMenu(fileName = "New BlueprintConfig", menuName = "Blueprint/New BlueprintConfig")]
public class BlueprintConfig : ScriptableObject
{
    public List<Blueprint> blueprints;
}