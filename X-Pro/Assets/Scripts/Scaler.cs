using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scaler : MonoBehaviour
{
    [Range(0, 5)]
    public float stepY = 1;
    public GameObject barPrefrab;
    public GameObject grapHolder;

    // Start is called before the first frame update
    void Start()
    {
        if (barPrefrab == null || grapHolder == null)
            return;

        float currx = 0;

        float currScaleY = 0;

        int colorIndex = 0;

        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };

        for(int i = 0; i < 10; i++)
        {
            GameObject bar = Instantiate<GameObject>(barPrefrab, new Vector3(currx, 0, 0), Quaternion.identity);
            bar.transform.parent = grapHolder.transform;
            bar.transform.localScale = new Vector3(bar.transform.localScale.x, currScaleY, bar.transform.localScale.z);

            Transform barGeometry = bar.transform.Find("Bar");

            barGeometry.GetComponent<Renderer>().material.color = colors[colorIndex % 4];

            if (barGeometry == null)
                return;


            currScaleY += 0.1f;
            currx += 1.5f;
            colorIndex++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
