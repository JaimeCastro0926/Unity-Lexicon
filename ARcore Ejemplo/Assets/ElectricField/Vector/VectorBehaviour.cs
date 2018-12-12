using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorBehaviour : MonoBehaviour {

	Transform origin;
	Transform target;

	Transform cylinder;
	Transform cone;

	Vector3 cylinderScale;
	float  coneScale;

//--------------------------------------------------------------------------------------------
	void Start () {

	}
//--------------------------------------------------------------------------------------------
	public void UpdateVector (Vector3 newPosition, Vector3 newTarget) {

		cone = transform.GetChild (0);
		cylinder = transform.GetChild (1);

		cylinderScale 	= cylinder.localScale;
		coneScale 		= cone.localScale.y * 2.5f;


		transform.position = newPosition;
		transform.LookAt (newTarget);

		float dist = (newTarget - newPosition).magnitude;

		cylinder.localScale = new Vector3 (cylinderScale.x, dist/2 - coneScale/2, cylinderScale.z);
		cylinder.localPosition = new Vector3 (0, 0, dist/2 - coneScale/2);
		cone.localPosition = new Vector3 (0, 0, dist - coneScale);

	}
//--------------------------------------------------------------------------------------------
	void Update () {


//		UpdateVector (origin.position, target.position);

	}
//--------------------------------------------------------------------------------------------
}
