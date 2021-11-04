using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq.Expressions;
using BarGraph.VittorCloud;

[Serializable]
class VisuData
{
    public string[] headers;
    public string data;
}

public class BarGraphCSV : MonoBehaviour
{
    public string parsingFile;
    public string testJson;
    public List<BarGraphDataSet> visuDataSet;
    BarGraphGenerator barGraphGenerator;
    public Camera sceneCam;

    void Start()
    {
        if(string.IsNullOrEmpty(parsingFile))
        {
            Debug.LogError("parsingFile path is empty!");
            return;
        }

        if (Resources.Load(parsingFile) == null)
            Debug.Log("Gibts nicht geht nicht!");

        List<Dictionary<string, object>> data = CSVReader.Read(parsingFile);

        //SetBarGraphDataSet(data);

        //List<Dictionary<string, object>> data = parseJson(testJson);

        SetBarGraphDataSet(data);

        barGraphGenerator = GetComponent<BarGraphGenerator>();

        barGraphGenerator.GeneratBarGraph(visuDataSet);



        //string logString = "[";

        //foreach (string key in parsed[0].Keys)
        //    logString += " " + key;
        //logString += "]";

        //Debug.Log("Headers: " + logString);
    }

    //void SetBarGraphDataSet(List<Dictionary<string, object>> data)
    //{
    //    Dictionary<string, object>.KeyCollection keyCollection = data[0].Keys;
    //    string[] keys = new string[keyCollection.Count];
    //    keyCollection.CopyTo(keys, 0);

    //    for(int i = 0; i < keys.Length;i++)
    //    {
    //        BarGraphDataSet dataSet = new BarGraphDataSet();
    //        dataSet.GroupName = keys[i];
    //        dataSet.barColor = Color.red;
    //        dataSet.ListOfBars = new List<XYBarValues>();

    //        for(int j = 0; j < data.Count; j++)
    //        {
    //            XYBarValues xyValue = new XYBarValues();
    //            xyValue.XValue = keys[i];
    //            xyValue.YValue = System.Convert.ToSingle(data[j][keys[i]]);

    //            dataSet.ListOfBars.Add(xyValue);
    //        }

    //        exampleDataSet.Add(dataSet);
    //    }
    //}

    private List<Dictionary<string, object>> parseJson(string path)
    {
        TextAsset asset = Resources.Load(path) as TextAsset;
        VisuData visuData = null;
        string json = asset.text.Replace('\\', ';');

        if (asset != null)
        {
            visuData = JsonUtility.FromJson<VisuData>(json);
        }

        string[] dataSets = visuData.data.Split(';');

        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

        int i = 0;

        bool @break = false;

        while (i < dataSets.Length && !@break)
        {
            Dictionary<string, object> dataSet = new Dictionary<string, object>();
            
            foreach(string header in visuData.headers)
            {
                dataSet.Add(header, dataSets[i]);

                i++;

                if(i >= dataSets.Length) // shouldn't happen usually
                {
                    @break = true;
                    break;
                }
            }

            data.Add(dataSet);
        }

        return data;
    }

    struct Country
    {
        public string name;
        public int[] monthCases;

        public Country(string name)
        {
            this.name = name;
            monthCases = new int[12];
        }

        public override string ToString()
        {
            string result = string.Empty;

            result += name + ":\n";

            for (int i = 0; i < monthCases.Length; i++)
                result += monthCases[i].ToString() + "\n";

            return result;
        }
    }

    struct Bezirk
    {
        public string name;
        public Dictionary<string, int> dayCases;

        public Bezirk(string name)
        {
            this.name = name;
            dayCases = new Dictionary<string, int>();
        }

        public void AddDay(string day, int value)
        {
            dayCases.Add(day, value);
        }
    }

    string parseToMonthName(int month)
    {
        switch(month)
        {
            case 1: return "January"; break;
            case 2: return "February"; break;
            case 3: return "March"; break;
            case 4: return "April"; break;
            case 5: return "May"; break;
            case 6: return "June"; break;
            case 7: return "July"; break;
            case 8: return "August"; break;
            case 9: return "September"; break;
            case 10: return "October"; break;
            case 11: return "November"; break;
            case 12: return "December"; break;
            default: return string.Empty;
        }
    }   

    void SetBarGraphDataSet(List<Dictionary<string, object>> data)
    {
        Dictionary<string, Country> countries = new Dictionary<string, Country>();

        for (int i = 0; i < data.Count; i++)
        {
            string country = data[i]["countriesAndTerritories"].ToString();
            int month = Convert.ToInt32(data[i]["month"]);
            int cases = Convert.ToInt32(data[i]["cases"]);

            if (!countries.ContainsKey(country))
            {
                countries.Add(country, new Country(country));
            }

            countries[country].monthCases[month - 1] += cases;
        }

        Dictionary<string, Color> colorPairs = new Dictionary<string, Color>();
        colorPairs.Add("Austria", Color.red);
        colorPairs.Add("Germany", Color.yellow);
        colorPairs.Add("France", Color.blue);
        colorPairs.Add("Italy", Color.green);

        foreach (string countrys in colorPairs.Keys)
        {
            Country country = countries[countrys];

            BarGraphDataSet dataSet = new BarGraphDataSet();
            dataSet.GroupName = country.name;
            dataSet.barColor = colorPairs[countrys];
            dataSet.ListOfBars = new List<XYBarValues>();

            for (int i = 0; i < country.monthCases.Length; i++)
            {
                XYBarValues xyValues = new XYBarValues();
                xyValues.XValue = parseToMonthName(i + 1);
                xyValues.YValue = country.monthCases[i];

                dataSet.ListOfBars.Add(xyValues);
            }

            visuDataSet.Add(dataSet);
        }
    }


    //void SetBarGraphDataSet(List<Dictionary<string, object>> data)
    //{
    //    const int Z_SETS = 20;

    //    Dictionary<string, Bezirk> cities = new Dictionary<string, Bezirk>();

    //    for (int i = 0; i < data.Count; i++)
    //    {
    //        string day = (string)data[i]["Time"];
    //        string bezirk = (string)data[i]["Bezirk"];
    //        int cases = Convert.ToInt32(data[i]["AnzahlFaelle"]);

    //        if (cities.ContainsKey(bezirk))
    //        {
    //            cities[bezirk].AddDay(day, cases);
    //        }
    //        else
    //        {
    //            cities.Add(bezirk, new Bezirk(bezirk));
    //            cities[bezirk].AddDay(day, cases);
    //        }
    //    }

    //    int counter = 0;

    //    foreach (Bezirk bez in cities.Values)
    //    {
    //        BarGraphDataSet dataSet = new BarGraphDataSet();
    //        dataSet.GroupName = bez.name;
    //        dataSet.barColor = Color.red;
    //        dataSet.ListOfBars = new List<XYBarValues>();

    //        foreach (string day in bez.dayCases.Keys)
    //        {
    //            XYBarValues xyValues = new XYBarValues();
    //            xyValues.XValue = day;
    //            xyValues.YValue = bez.dayCases[day];

    //            dataSet.ListOfBars.Add(xyValues);
    //        }

    //        visuDataSet.Add(dataSet);

    //        if (++counter >= Z_SETS)
    //            break;
    //    }
    //}
}


