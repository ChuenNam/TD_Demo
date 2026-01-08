using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public GridObjectData objectData;

    public int level = 1;
    public List<Blueprint> blueprints;

    public void Initialize(string id, GridManager manager)
    {
        objectData = manager.GetObjectData(id);
        blueprints = objectData.blueprintConfig.blueprints;
    }
}
