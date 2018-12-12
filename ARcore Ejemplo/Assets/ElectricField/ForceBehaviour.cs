using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ForceBehaviour : MonoBehaviour {

	public GameObject arrowPrefab;

	GameObject[] 	charges;
	GameObject[]	arrows;
	Vector3[] 		forces;
	float[]			q;

//--------------------------------------------------------------------------------------------
	void Start () {

		arrows = new GameObject[0];

//		Debug.Log ( arrows.Length );

		LoadVectors ();
		VectorsUpdate ();


	}
//--------------------------------------------------------------------------------------------
	void LoadVectors () {


//		Debug.Log ( arrows.Length );

		for (int k = 0; k < arrows.Length; k++) {
			Destroy (arrows [k]);
		}

		charges = GameObject.FindGameObjectsWithTag("Charge");
		arrows = new GameObject[charges.Length];
			for (int k = 0; k < charges.Length; k++) {
				arrows[k] = Instantiate (arrowPrefab, charges [k].transform.position, Quaternion.identity);
			}

		q = new float[charges.Length];
			for (int k = 0; k < charges.Length; k++) {

	//			q[k] = Convert.ToSingle(charges[k].name);
				if (charges [k].name == "+1") {
					q [k] = 1;
				} else {
					q [k] = -1;
				}
			}

//		Debug.Log (charges.Length);

	}
//--------------------------------------------------------------------------------------------
	void VectorsUpdate () {

		forces = new Vector3[charges.Length];

		for (int c = 0; c < charges.Length; c++) {

			Vector3 parcialForce = Vector3.zero;

			for (int k = 0; k < charges.Length; k++) {
				if(c == k) {
					continue;
				}

				float r = (charges[k].transform.position - charges[c].transform.position).magnitude;
				Vector3 normal = (charges [c].transform.position - charges [k].transform.position).normalized;

				parcialForce +=  Mathf.Sign(q [c] ) * q [k] / (r * r) * normal;

			}

			forces [c] = 0.001f*parcialForce;
		}
			
		for (int k = 0; k < charges.Length; k++) {
			arrows[k].GetComponent<VectorBehaviour> ().UpdateVector (charges [k].transform.position, charges [k].transform.position+forces [k]);
		}


	}
//--------------------------------------------------------------------------------------------
	void Update () {

		LoadVectors ();
		VectorsUpdate ();

	}
//--------------------------------------------------------------------------------------------
}
