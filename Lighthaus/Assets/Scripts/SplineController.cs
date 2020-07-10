using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SplineController : MonoBehaviour
{
	public GameObject SplineRoot;
	private const float STEP_DIVIDER = 0.1f;
	public eWrapMode WrapMode = eWrapMode.ONCE;
	public bool AutoStart = true;
	public bool AutoClose = true;
	public bool HideOnExecute = true;
	private SplineInterpolator mSplineInterp;
	[HideInInspector]
	public Transform[] mTransforms;
	public bool LookAtDestination = true;


	void Start()
	{
		mSplineInterp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;

		//mTransforms = GetTransforms();

		//Hide Soline Roots
		if (HideOnExecute)
			DisableTransforms();

		if (AutoStart)
			FollowSpline();
	}

	void SetupSplineInterpolator(SplineInterpolator interp, Transform[] trans)
	{
		interp.Reset();
		float Step;

		int Counter;
		float TotalLength = 0;
		float[] DistanceList = new float[trans.Length];

		if (AutoClose)
		{
			for (int i = 0; i < trans.Length; i++)
			{
				float Distance;
				
				if (i == trans.Length - 1)
				{
					Distance = Vector3.Distance(trans[i].position, trans[0].position);
					TotalLength += Distance;
				}
				else
				{
					Distance = Vector3.Distance(trans[i].position, trans[i + 1].position);
					TotalLength += Distance;
				}
				DistanceList[i] = Distance;
			}
		}
        else
		{
			for (int i = 0; i < trans.Length-1; i++)
			{
				float Distance;
				Distance = Vector3.Distance(trans[i].position, trans[i+1].position);
				TotalLength += Distance;
				DistanceList[i] = Distance;
			}
		}

		float CurrentDis = 0;
		for (Counter = 0; Counter < trans.Length; Counter++)
		{
			Quaternion rot;
			if (Counter != trans.Length - 1)
				rot = Quaternion.LookRotation(trans[Counter + 1].position - trans[Counter].position, trans[Counter].up);
			else if (AutoClose)
				rot = Quaternion.LookRotation(trans[0].position - trans[Counter].position, trans[Counter].up);
			else
				rot = trans[Counter].rotation;

			Step = CurrentDis / TotalLength / STEP_DIVIDER;
				
			interp.AddPoint(trans[Counter].position, rot, Step, new Vector2(0, 1));
			CurrentDis = CurrentDis + DistanceList[Counter];
		}

		

		if (AutoClose)
			interp.SetAutoCloseMode(CurrentDis/TotalLength/STEP_DIVIDER);
	}



	/// <summary>
	/// Disables the spline objects, we don't need them outside design-time.
	/// </summary>
	void DisableTransforms()
	{
		if (SplineRoot != null)
		{
			SplineRoot.SetActiveRecursively(false);

		}
	}


	/// <summary>
	/// Starts the interpolation
	/// </summary>
	void FollowSpline()
	{
		if (mTransforms.Length > 0)
		{
			SetupSplineInterpolator(mSplineInterp, mTransforms);
			mSplineInterp.StartInterpolation(null, LookAtDestination, WrapMode);
		}
	}
}