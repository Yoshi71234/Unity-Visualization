using BarGraph.VittorCloud;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroupFilterMode
{
    X_Filter,
    Z_Filter
}

public class NullFilter : IFilter
{
    public bool query(GameObject targetBar)
    {
        return true;
    }
}

public class GroupFilter : IFilter
{
    private GroupFilterMode mode;
    private GameObject filterSelectorBar;

    public GroupFilter(GroupFilterMode mode, GameObject filterSelectorBar)
    {
        this.mode = mode;
        this.filterSelectorBar = filterSelectorBar;
    }

    public bool query(GameObject targetBar)
    {
        if (filterSelectorBar != null && targetBar != null)
        {
            BarProperty filterProperty = filterSelectorBar.GetComponent<BarProperty>();
            BarProperty targetProperty = targetBar.GetComponent<BarProperty>();

            if (filterProperty != null && targetProperty != null)
            {
                if (mode == GroupFilterMode.X_Filter)
                {
                    return (filterProperty.dataSet.x_name == targetProperty.dataSet.x_name);
                }
                else if (mode == GroupFilterMode.Z_Filter)
                {
                    return (filterProperty.dataSet.groupName == targetProperty.dataSet.groupName);
                }

            } 
        }

        return false;
    }
}


public class ViewSetFilter : IFilter
{
    private ViewSet viewSet;

    public ViewSetFilter(ViewSet viewSet)
    {
        this.viewSet = viewSet;
    }

    public bool query(GameObject targetBar)
    {
        if (targetBar != null)
        {
            BarProperty property = targetBar.GetComponent<BarProperty>();

            if (property != null)
            {
                if(viewSet.View_X.Contains(property.dataSet.x_name))
                    return true;

                if (viewSet.View_Z.Contains(property.dataSet.groupName))
                    return true;
            } 
        }

        return false;
    }
}

public interface IFilter
{
    bool query(GameObject targetBar);
}
