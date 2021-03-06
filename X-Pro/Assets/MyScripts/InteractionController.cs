using BarGraph.VittorCloud;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

[Serializable]
public class ViewSet
{
    public List<string> View_X = new List<string>();
    public List<string> View_Z = new List<string>();
}

public class InteractionController : MonoBehaviour
{
    /// <summary>
    /// Stores inital data for each bar 
    /// </summary>
    struct BarDataSet
    {
        public GameObject bar;
        public BarProperty property;
        public Vector3 position;
        public Color color;
        public string text;
    }

    #region Private Members

    private BarGraphGenerator generator;
    private GameObject focused;
    bool selected = false;
    private bool init = false;
    private bool moving;
    bool start = false;
    private Dictionary<GameObject, BarDataSet> barDatasets = new Dictionary<GameObject, BarDataSet>();

    #endregion

    #region Public Members

    public ViewSet viewSet;
    public GameObject viewer; 

    #endregion

    /// <summary>
    /// Checks if a specific <see cref="Vector3"/> point is within a certain range to another <see cref="Vector3"/> point
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <param name="distance">The maximum range between the two points</param>
    /// <returns></returns>

    private bool Around(Vector3 point1, Vector3 point2, float distance)
    {
        return (point1 - point2).magnitude < distance;

        
    }

    private void TestItOut()
    {
        int[] numbers = new int[11];
        for (int i = 0; i <= 10; i++)
            numbers[i] = i;

        string lambda = "number => number % 2 == 0"; // get all odd numbers

        
    }

    // Start is called before the first frame update
    void Start()
    {

        TestItOut();

        if(viewer == null)
        {
            Debug.LogError("Error no viewer was selected!");
            return;
        }

        generator = GetComponent<BarGraphGenerator>();

        if(generator == null)
        {
            Debug.LogError("Error There was no BarGraphGenerator instance found!");
            return;
        }

        //viewer.transform.position = new Vector3(18.7088f, 29.20664f, -84.86042f);
        //viewer.transform.rotation = Quaternion.Euler(21.685f, 0.511f, 0);
        setEventsFromGenerator();
        registerAll();
    }

    #region BarCallbacks

    private void onBarPointerDown(GameObject bar)
    {
        focusBar(bar);
    }

    private void onBarPointerUp(GameObject bar)
    {

    }

    private void onBarHoverEnter(GameObject bar)
    {
        
    }

    private void onBarHoverExit(GameObject bar)
    {

    }

    #endregion

    #region Private Functions

    private Transform GetRoot(Transform child)
    {
        Transform root = child;

        while (root.parent != null)
            root = root.parent;

        return root;
    }

    /// <summary>
    /// register all initial bars to the set
    /// </summary>
    private void registerAll()
    {
        if (!init)
            init = true;
        else
            throw new Exception("Error bars are already registered");

        foreach(BarGroup group in generator.Graph.ListOfGroups)
        {
            foreach(GameObject bar in group.ListOfBar)
            {
                registerToSet(bar);
            }
        }
    }

    /// <summary>
    /// Register a specific bar to the set
    /// </summary>
    /// <param name="bar"></param>
    private void registerToSet(GameObject bar)
    {
        if(!barDatasets.ContainsKey(bar))
        {
            BarProperty property = bar.GetComponent<BarProperty>();

            if (property != null)
            {
                BarDataSet barSet = new BarDataSet();

                barSet.bar = bar;
                barSet.color = property.barMesh.material.color;
                barSet.position = property.BarLabel.transform.position;
                barSet.property = property;
                barSet.text = string.Copy(property.BarLabel.text);

                barDatasets.Add(bar, barSet);
            }
        }
    }

    /// <summary>
    /// restores a bar to it's default based on it's registration from the set
    /// </summary>
    /// <param name="bar"></param>
    private void restoreBarDefault(GameObject bar)
    {
        BarDataSet focusedSet = barDatasets[bar];                               // get the data set for the corresponding bar

        focusedSet.property.SetBarLabel(string.Copy(focusedSet.text), 1);           // reset the bar label text
        focusedSet.property.BarLabel.transform.position = focusedSet.position;      // reset the bar labels y position
        focusedSet.property.SetLabelEnabel();                                       // reset the bar label
    }

    /// <summary>
    /// Sets a specific bar as focused and restores the default for every other bar
    /// </summary>
    /// <param name="bar"></param>
    private void setFocused(GameObject bar)
    {
        if (!barDatasets.ContainsKey(bar))
            registerToSet(bar);

        if (focused == null)
            focused = bar;
        else if(focused != bar && barDatasets.ContainsKey(focused))
        {
            restoreBarDefault(focused);
                
            focused = bar;                                                              // set the new bar now as focused
        }   
    }

    /// <summary>
    /// Greys all bars except a given one
    /// </summary>
    /// <param name="bar"></param>
    private void grey_bars_without(GameObject bar)
    {
        foreach(BarGroup group in generator.Graph.ListOfGroups)
        {
            foreach(GameObject curr in group.ListOfBar)
            {
                if (!barDatasets.ContainsKey(curr))
                    registerToSet(curr);

                BarDataSet barSet = barDatasets[curr];

                if (barSet.bar != bar) // grey it
                {
                    BarProperty property = barSet.bar.GetComponent<BarProperty>();

                    if (property != null)
                    {
                        property.barMesh.material.color = Color.gray;
                        property.BarLabel.text = string.Empty;
                    }
                }
                else // reset it's color based on it's set
                {
                    BarProperty property = bar.GetComponent<BarProperty>();

                    if (property != null)
                    {
                        property.barMesh.material.color = barSet.color;
                        property.BarLabel.text = barSet.text;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Realizing a motion effect which is flying the camera to a certain given object
    /// </summary>
    /// <param name="targetObject">The target object</param>
    /// <param name="relCamVector">A relative vector in which the camerea should be at the end of the movement relative to the target object</param>
    /// <param name="viewer">The camera object</param>
    /// <param name="seconds">The time in which the movement should be finished</param>
    /// <param name="lookObject">Sets where the camera should be looking while the movement and at the end</param>
    private void moveFocused(GameObject targetObject, Vector3 relCamVector, GameObject viewer, int seconds, GameObject lookObject = null)
    {
        IEnumerator stepMoveFocused(GameObject targetObject, Vector3 targetPosition, float period, float resolution, GameObject lookObject = null)
        {
            Vector3 step = (targetPosition - targetObject.transform.position).normalized * resolution;

            while (!Around(targetObject.transform.position, targetPosition, 0.1f))
            {
                targetObject.transform.position += step;

                if (lookObject != null)
                    viewer.transform.LookAt(lookObject.transform.position);

                yield return new WaitForSeconds(period);
            }

            moving = false;
        }
        

        if (!moving)
            moving = true;

        GameObject rootObject = GetRoot(targetObject.transform).gameObject;

        Vector3 diffVector = targetObject.transform.position - rootObject.transform.position;

        Vector3 targetPosition = viewer.transform.position - relCamVector;

        Vector3 rootTargetPosition = targetPosition - diffVector;

        const int frameRate = 25;
        float period = 1f / (float)frameRate;
        int frameCount = frameRate * seconds;
        float distance = (targetObject.transform.position - targetPosition).magnitude;
        float resolution = distance / (float)frameCount;

        StartCoroutine(stepMoveFocused(rootObject, rootTargetPosition,period,resolution,lookObject));
    }

    /// <summary>
    /// Sets the text label for a highlighted bar
    /// </summary>
    /// <param name="bar"></param>
    private void setBarHighlightText(GameObject bar)
    {
        BarProperty property = bar.GetComponent<BarProperty>();

        if(property != null)
        {
            string focusedText = string.Empty;
            focusedText += property.dataSet.groupName + '\n';
            focusedText += property.dataSet.x_name + '\n';
            focusedText += property.dataSet.y_value + '\n';

            property.SetBarLabel(focusedText, 1);
            property.SetLabelEnabel();
            property.BarLabel.transform.position = new Vector3(property.BarLabel.transform.position.x, property.BarLabel.transform.position.y + 1, property.BarLabel.transform.position.z);
        }
    }

    /// <summary>
    /// Sets a certain bar highlight
    /// </summary>
    /// <param name="bar"></param>

    private void setBarHighlighted(GameObject bar)
    {
        setBarHighlightText(bar);
    }

    /// <summary>
    /// Focus a specific bar
    /// </summary>
    /// <param name="bar"></param>
    /// <returns></returns>
    private bool focusBar(GameObject bar)
    {
        if (moving) // Don't react if we are already moving
            return false;

        if (viewer != null)
        {
            BarProperty property = bar.GetComponent<BarProperty>(); // get the bars property

            if (property != null)
            {
                if (focused != bar)
                    setFocused(bar);
                else // reset the focus
                {
                    setBarsDefault();
                    focused = null;
                    return false;
                }


                //Vector3 relCamVec = (bar.transform.up * bar.transform.localScale.y) - bar.transform.right * 7 + bar.transform.up * 7 - bar.transform.forward * 7;

                //grey_bars_without(bar);

                //moveFocused(bar, relCamVec, viewer, 5,bar);
                //applyFilter(new ViewSetFilter(viewSet));

                //viewer.transform.position = camPosition;
                //viewer.transform.LookAt(lookPosition);

                //setBarHighlighted(bar);


                return true;
            }

        }

        return false;

    }


    private void setBarsDefault()
    {
        foreach(BarGroup group in generator.Graph.ListOfGroups)
        {
            foreach(GameObject bar in group.ListOfBar)
            {
                GameObject cube = bar.transform.GetChild(0).gameObject;
                BarProperty property = bar.GetComponent<BarProperty>();

                cube.SetActive(true);
                property.BarLabel.gameObject.SetActive(true);
                restoreBarDefault(bar);
            }
        }
    }

    /// <summary>
    /// Applies a filter <see cref="IFilter"/> on all bars
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    private GameObject[] applyFilter(IFilter filter)
    {
        List<GameObject> filtered = new List<GameObject>();

        foreach (BarGroup group in generator.Graph.ListOfGroups)
        {
            foreach (GameObject bar in group.ListOfBar)
            {
                if (filter.query(bar))
                {
                    //setBarHighlighted(bar);
                    filtered.Add(bar);
                }
                else
                {
                    GameObject cube = bar.transform.GetChild(0).gameObject;
                    BarProperty property = bar.GetComponent<BarProperty>();

                    cube.SetActive(false);
                    property.BarLabel.gameObject.SetActive(false);
                }
            }
        }

        return filtered.ToArray();
    }

    private void sort(GameObject bar)
    {
        BarProperty property = bar.GetComponent<BarProperty>();
        List<GameObject> bars = new List<GameObject>(barDatasets.Keys);
        bars.Sort(new BarComparer());


        if(property != null)
        {
            string x_name = property.dataSet.x_name;
            string groupName = property.dataSet.groupName;
        }
    }

    /// <summary>
    /// register all handlers
    /// </summary>
    private void setEventsFromGenerator()
    {
        generator.OnBarPointerDown.AddListener(onBarPointerDown);
        generator.OnBarPointerUp.AddListener(onBarPointerUp);
        generator.OnBarHoverEnter.AddListener(onBarHoverEnter);
        generator.OnBarHoverExit.AddListener(onBarHoverExit);
    }

    /// <summary>
    /// update the movement of the camera based on the user input
    /// </summary>
    private void updateMovement()
    {
        const float rotSpeed = 30;
        const float horizontalSpeed = 5;
        const float verticalSpeed = 5;

        float rot_X = Input.GetAxis("Mouse X");
        float rot_Y = Input.GetAxis("Mouse Y");
        float move_X = Input.GetAxis("Horizontal");
        float move_Y = Input.GetAxis("Vertical");

        if (moving)
            return;

            if (!start) // ignore first input for avoiding bad start
        {
            rot_X = 0;
            rot_Y = 0;
            move_X = 0;
            move_Y = 0;

            start = true;
        }

        if (rot_X != 0)
        {
            viewer.transform.RotateAround(viewer.transform.position, new Vector3(0, 1, 0), rotSpeed * rot_X * Time.deltaTime);
        }

        if (rot_Y != 0)
        {
            viewer.transform.RotateAround(viewer.transform.position, new Vector3(1, 0, 0), -rotSpeed * rot_Y * Time.deltaTime);
        }

        if (move_X != 0)
        {
            viewer.transform.position = viewer.transform.position + viewer.transform.right * (move_X * horizontalSpeed * Time.deltaTime);
        }

        if (move_Y != 0)
        {
            viewer.transform.position = viewer.transform.position + viewer.transform.forward * (move_Y * verticalSpeed * Time.deltaTime);
        }

        Vector3 curr = viewer.transform.rotation.eulerAngles;

        viewer.transform.rotation = Quaternion.Euler(curr.x, curr.y, 0);
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        updateMovement();
    }
}
