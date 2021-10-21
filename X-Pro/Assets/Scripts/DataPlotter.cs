using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class DataPlotter : MonoBehaviour
{
    public string inputfile;

    private List<Dictionary<string, object>> pointList;

    // Indices for columns to be assigned
    public int columnX = 0;
    public int columnY = 1;
    public int columnZ = 2;

    // Full column names
    public string xName;
    public string yName;
    public string zName;

    public float plotScale = 10;

    // The prefab for the data points that will be instantiated
    public GameObject PointPrefab;

    // Object which will contain instantiated prefabs in hiearchy
    public GameObject PointHolder;

    private float FindMaxValue(string columnName)
    {
        //set initial value to first value
        float maxValue = Convert.ToSingle(pointList[0][columnName]);
        float value;

        //Loop through Dictionary, overwrite existing maxValue if new value is larger
        for (var i = 0; i < pointList.Count; i++)
        {
            value = Convert.ToSingle(pointList[i][columnName]);

            if (maxValue < value)
                maxValue = value;
        }

        //Spit out the max value
        return maxValue;
    }

    private float FindMinValue(string columnName)
    {

        float minValue = Convert.ToSingle(pointList[0][columnName]);
        float value;

        //Loop through Dictionary, overwrite existing minValue if new value is smaller
        for (var i = 0; i < pointList.Count; i++)
        {
            value = Convert.ToSingle(pointList[i][columnName]);

            if (value < minValue)
                minValue = value;
        }

        return minValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        pointList = CSVReader.Read(inputfile);

        Debug.Log(pointList);

        // Declare list of strings, fill with keys (column names)
        List<string> columnList = new List<string>(pointList[1].Keys);

        // Print number of keys (using .count)
        Debug.Log("There are " + columnList.Count + " columns in CSV");

        foreach (string key in columnList)
            Debug.Log("Column name is " + key);

        // Assign column name from columnList to Name variables
        xName = columnList[columnX];
        yName = columnList[columnY];
        zName = columnList[columnZ];

        // Get maxes of each axis
        float xMax = FindMaxValue(xName);
        float yMax = FindMaxValue(yName);
        float zMax = FindMaxValue(zName);

        // Get minimums of each axis
        float xMin = FindMinValue(xName);
        float yMin = FindMinValue(yName);
        float zMin = FindMinValue(zName);


        //Loop through Pointlist
        for (var i = 0; i < pointList.Count; i++)
        {
            // Get value in poinList at ith "row", in "column" Name, normalize
            float x =
            (System.Convert.ToSingle(pointList[i][xName]) - xMin) / (xMax - xMin);

            float y =
            (System.Convert.ToSingle(pointList[i][yName]) - yMin) / (yMax - yMin);

            float z =
            (System.Convert.ToSingle(pointList[i][zName]) - zMin) / (zMax - zMin);

            // Instantiate as gameobject variable so that it can be manipulated within loop
            GameObject dataPoint = Instantiate(
                    PointPrefab,
                    new Vector3(x, y, z) * plotScale,
                    Quaternion.identity);

            // Make dataPoint child of PointHolder object 
            dataPoint.transform.parent = PointHolder.transform;

            // Assigns original values to dataPointName
            string dataPointName =
            pointList[i][xName] + " " + (Convert.ToDouble(pointList[i][xName]) == xMax ? "[HIGHEST X]  " : "")
            + pointList[i][yName] + " " + (Convert.ToDouble(pointList[i][yName]) == yMax ? "[HIGHEST Y]  " : "")
            + pointList[i][zName] + (Convert.ToDouble(pointList[i][zName]) == zMax ? "[HIGHEST Z]  " : "");

            // Assigns name to the prefab
            dataPoint.transform.name = dataPointName;

            // Gets material color and sets it to a new RGBA color we define
            dataPoint.GetComponent<Renderer>().material.color =
            new Color(x, y, z, 1.0f);
        }   
    }
}
