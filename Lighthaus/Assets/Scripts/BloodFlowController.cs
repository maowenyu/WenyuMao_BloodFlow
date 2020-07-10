using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodFlowController : MonoBehaviour
{
    [Range(0.0f, 50.0f)]
    public int CellAmount;
    [Range(1.0f, 10.0f)]
    public int Speed = 5;
    [Range(1.0f, 10.0f)]
    public int Density = 3;
    [ColorUsage(false)]
    public Color CellColor;
    [ColorUsage(true)]
    public Color VesselColor;
    //public enum eColor { Red, Blue };
    //public eColor CellColor = eColor.Red;
    //public eColor VesselColor = eColor.Red;
    private const float DURATION = 10;
    public GameObject CellPrefab;
    public GameObject SplineRoot;
    private Transform[] RootTransform;
    public eWrapMode WrapMode = eWrapMode.ONCE;
    public bool AutoClose = true;
    public bool LookAtDestination = true;
    private LineRenderer lineRenderer;
    Vector3[] PositionsList = new Vector3[100];

    void OnDrawGizmos()
    {
        Transform[] trans = GetTransforms();
        if (trans.Length < 2)
            return;

        SplineInterpolator interp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;
        SetupSplineInterpolator(interp, trans);
        interp.StartInterpolation(null, false, WrapMode);

        Vector3 prevPos = trans[0].position;
        for (int c = 1; c <= 100; c++)
        {
            float currTime = c * DURATION / 100;
            Vector3 currPos = interp.GetHermiteAtTime(currTime);
            float mag = (currPos - prevPos).magnitude * 2;
            Gizmos.color = new Color(mag, 0, 0, 1);
            Gizmos.DrawLine(prevPos, currPos);
            prevPos = currPos;

        }

    }

    void Start()
    {
        RootTransform = GetTransforms();
        if (CellAmount>0)
        {
            StartCoroutine(InstantiateCells());
        }

        Transform[] trans = GetTransforms();
        if (trans.Length < 2)
            return;


        SplineInterpolator interp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;
        SetupSplineInterpolator(interp, trans);
        interp.StartInterpolation(null, false, WrapMode);
        Vector3 prevPos = trans[0].position;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 100;
        for (int c = 1; c <= 100; c++)
        {
            float currTime = c * DURATION / 100;
            Vector3 currPos = interp.GetHermiteAtTime(currTime);
            float mag = (currPos - prevPos).magnitude * 2;
            
            prevPos = currPos;

            PositionsList[c - 1] = currPos;

        }
        lineRenderer.SetPositions(PositionsList);
        lineRenderer.SetWidth(2.5f, 2.5f);
        lineRenderer.sortingOrder = 6000;
        lineRenderer.materials[0].color = VesselColor;

        
    }

    IEnumerator InstantiateCells()
    {
        for (int i = 0; i < CellAmount; i++)
        {
            //Instantiate cells and assign attributes
            GameObject NewCell = Instantiate(CellPrefab, RootTransform[0].position, Quaternion.identity);
            NewCell.GetComponent<SplineController>().mTransforms = RootTransform;
            NewCell.transform.SetParent(this.transform);
            NewCell.GetComponent<SplineInterpolator>().Speed = Speed;
            NewCell.GetComponent<SplineController>().AutoClose = AutoClose;
            NewCell.transform.rotation = Random.rotation;
            NewCell.GetComponent<SplineController>().LookAtDestination = LookAtDestination;
            NewCell.transform.GetChild(0).position = NewCell.transform.GetChild(0).position + Random.insideUnitSphere;
            NewCell.transform.GetChild(0).GetComponent<MeshRenderer>().materials[0].color = CellColor;
            yield return new WaitForSeconds((11 - Density)/10.0f);
        }
        
    }


    /// <summary>
    /// Returns children transforms, sorted by name.
    /// </summary>
    Transform[] GetTransforms()
    {
        if (SplineRoot != null)
        {
            List<Component> components = new List<Component>(SplineRoot.GetComponentsInChildren(typeof(Transform)));
            List<Transform> transforms = components.ConvertAll(c => (Transform)c);

            transforms.Remove(SplineRoot.transform);
            transforms.Sort(delegate (Transform a, Transform b)
            {
                return a.name.CompareTo(b.name);
            });

            return transforms.ToArray();
        }
        else Debug.Log("Get Transform root is null");

        return null;
    }

    void SetupSplineInterpolator(SplineInterpolator interp, Transform[] trans)
    {
        interp.Reset();

        float step = (AutoClose) ? DURATION / trans.Length :
            DURATION / (trans.Length - 1);

        int c;
        for (c = 0; c < trans.Length; c++)
        {
           
            Quaternion rot;
            if (c != trans.Length - 1)
                rot = Quaternion.LookRotation(trans[c + 1].position - trans[c].position, trans[c].up);
            else if (AutoClose)
                rot = Quaternion.LookRotation(trans[0].position - trans[c].position, trans[c].up);
            else
                rot = trans[c].rotation;

            interp.AddPoint(trans[c].position, rot, step * c, new Vector2(0, 1));
        }

        if (AutoClose)
            interp.SetAutoCloseMode(step * c);
    }
}
