using BarGraph.VittorCloud;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarComparer : IComparer<GameObject>
{
    public int Compare(GameObject obj1, GameObject obj2)
    {

        BarProperty property_1 = obj1.transform.GetComponent<BarProperty>();
        BarProperty property_2 = obj2.transform.GetComponent<BarProperty>();

        return (property_1.dataSet.y_value > property_2.dataSet.y_value ? 1 :
                    property_1.dataSet.y_value < property_2.dataSet.y_value ? -1 :
                    0);
    }
}
