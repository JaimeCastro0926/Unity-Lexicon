using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FieldLines : MonoBehaviour {

	GameObject[] 	charges;
	float[] 		q;

	Transform[]	fieldLineObjects;
	float[]		qSignValue;

	public float delta = 0.05f;
	public int numItera = 500;
	public float lineEffectSpeed = 0.3f;

	float chargeRadio;
	Vector3[] vertexCoord;
	float t01;
	Transform lineObj;
	Color lineColor   = new Color (0.5f, 0.5f, 1, 0);
	Color effectColor = new Color (1, 0, 0, 1);
	Gradient grad = new Gradient();
	GradientColorKey startGrad;
	GradientColorKey endGrad;

// ------------------------------------------------------------------------------------------
	void Start () {




	// line vars
		startGrad = new GradientColorKey(lineColor, 0);
		endGrad = new GradientColorKey(lineColor, 1);

		lineObj = new GameObject ().transform;

		lineObj.gameObject.AddComponent<LineRenderer> ();
		lineObj.GetComponent<LineRenderer> ().material = new Material(Shader.Find("Sprites/Default")); // Add Material
		lineObj.GetComponent<LineRenderer> ().widthMultiplier = 0.002f;
		lineObj.GetComponent<LineRenderer> ().loop = false;
		lineObj.GetComponent<LineRenderer> ().numCapVertices = 0;
		lineObj.GetComponent<LineRenderer> ().numCornerVertices = 0;
		lineObj.GetComponent<LineRenderer> ().receiveShadows = false;
		lineObj.GetComponent<LineRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;


		LoadCharges ();

		vertexCoord = VertexCoordinates (2);


		chargeRadio = (charges [0].transform.localScale.x) / 2;

		fieldLineObjects = new Transform[charges.Length*vertexCoord.Length];
		qSignValue = new float[charges.Length*vertexCoord.Length];


		int numLines = 0;

		for (int k = 0; k < charges.Length; k++) {

			for (int i = 0; i < vertexCoord.Length; i++) {

				Transform newLine = Instantiate (lineObj, 
									1.001f*chargeRadio*vertexCoord[i] + charges [k].transform.position, 
									Quaternion.identity);
				newLine.parent = charges [k].transform;

				fieldLineObjects [numLines] = newLine;
				qSignValue [numLines] = Mathf.Sign(q[k]);
				numLines++;

			}
		}
		Destroy (lineObj.gameObject);
		UpdateFieldLines (fieldLineObjects, qSignValue );

	}
//--------------------------------------------------------------------------------------------
	void LoadCharges () {

		charges = GameObject.FindGameObjectsWithTag("Charge");
		q = new float[charges.Length];
		for (int k = 0; k < charges.Length; k++) {
			q[k] = Convert.ToSingle(charges[k].name);
		}

	}
//--------------------------------------------------------------------------------------------
	Vector3[] VertexCoordinates (int qValue) {

		Vector3[] vert = new Vector3[0];
		float phi = ((1 + Mathf.Sqrt (5)) / 2);

		switch (qValue) {
		case 1:
			
			// dipiramide
			vert = new Vector3[6];
				vert [0] = new Vector3 ( 0,  1,  0);
				vert [1] = new Vector3 ( 0, -1,  0);
				vert [2] = new Vector3 ( 1,  0,  0);
				vert [3] = new Vector3 (-1,  0,  0);
				vert [4] = new Vector3 ( 0,  0,  1);
				vert [5] = new Vector3 ( 0,  0, -1);
			break;
		case 2:
			
			// Icosaedro
			vert = new Vector3[12];
				vert [0]  = new Vector3 (0,  1,  phi);
				vert [1]  = new Vector3 (0,  1, -phi);
				vert [2]  = new Vector3 (0, -1,  phi);
				vert [3]  = new Vector3 (0, -1, -phi);
				vert [4]  = new Vector3 ( 1,  phi, 0);
				vert [5]  = new Vector3 ( 1, -phi, 0);
				vert [6]  = new Vector3 (-1,  phi, 0);
				vert [7]  = new Vector3 (-1, -phi, 0);
				vert [8]  = new Vector3 ( phi, 0,  1);
				vert [9]  = new Vector3 ( phi, 0, -1);
				vert [10] = new Vector3 (-phi, 0,  1);
				vert [11] = new Vector3 (-phi, 0, -1);

			break;
		case 3:

			// dodecaedro
			vert = new Vector3[20];
				float iphi = (1 / phi);
				vert [0] = new Vector3 ( 1,  1,  1);
				vert [1] = new Vector3 ( 1,  1, -1);
				vert [2] = new Vector3 ( 1, -1,  1);
				vert [3] = new Vector3 ( 1, -1, -1);
				vert [4] = new Vector3 (-1,  1,  1);
				vert [5] = new Vector3 (-1,  1, -1);
				vert [6] = new Vector3 (-1, -1,  1);
				vert [7] = new Vector3 (-1, -1, -1);
				vert [8]  = new Vector3 (0,  phi,  iphi);
				vert [9]  = new Vector3 (0,  phi, -iphi);
				vert [10] = new Vector3 (0, -phi,  iphi);
				vert [11] = new Vector3 (0, -phi, -iphi);
				vert [12] = new Vector3 ( iphi, 0, phi); 
				vert [13] = new Vector3 ( iphi, 0,-phi);
				vert [14] = new Vector3 (-iphi, 0, phi);
				vert [15] = new Vector3 (-iphi, 0,-phi);
				vert [16] = new Vector3 ( phi,  phi ,0); 
				vert [17] = new Vector3 ( phi, -phi ,0);
				vert [18] = new Vector3 (-phi,  phi ,0);
				vert [19] = new Vector3 (-phi, -phi ,0);

			break;
		case 4:

			// Girobicúpula cuadrada elongada
			vert = new Vector3[24];
				float root2 = Mathf.Sqrt (2);
				vert [0] = new Vector3 (1+root2 , 0,  root2 );
				vert [1] = new Vector3 (1+root2 , 0, -root2 );
				vert [2] = new Vector3 (1+root2 ,  root2, 0 );
				vert [3] = new Vector3 (1+root2 , -root2, 0 );

				vert [4]  = new Vector3 (  1,  1,  (1+root2));
				vert [5]  = new Vector3 (  1,  1, -(1+root2));
				vert [6]  = new Vector3 (  1, -1,  (1+root2));
				vert [7]  = new Vector3 (  1, -1, -(1+root2));
				vert [8]  = new Vector3 ( -1,  1,  (1+root2));
				vert [9]  = new Vector3 ( -1,  1, -(1+root2));
				vert [10] = new Vector3 ( -1, -1,  (1+root2));
				vert [11] = new Vector3 ( -1, -1, -(1+root2));

				vert [12] = new Vector3 (  1,  (1+root2),  1);
				vert [13] = new Vector3 (  1,  (1+root2), -1);
				vert [14] = new Vector3 (  1, -(1+root2),  1);
				vert [15] = new Vector3 (  1, -(1+root2), -1);
				vert [16] = new Vector3 ( -1,  (1+root2),  1);
				vert [17] = new Vector3 ( -1,  (1+root2), -1);
				vert [18] = new Vector3 ( -1, -(1+root2),  1);
				vert [19] = new Vector3 ( -1, -(1+root2), -1);

				vert [20] = new Vector3 ( -(1+root2),  1,  1);
				vert [21] = new Vector3 ( -(1+root2),  1, -1);
				vert [22] = new Vector3 ( -(1+root2), -1,  1);
				vert [23] = new Vector3 ( -(1+root2), -1, -1);

			break;
		default:
			Debug.Log ("no vertex");
			break;
		}

		for (int k = 0; k < vert.Length; k++) {
			vert [k].Normalize();
		}

		return vert;

	}
//--------------------------------------------------------------------------------------------
	void UpdateFieldLines (Transform[] funFieldLineObjects, float[] funQSignValue ) {

		for (int k = 0; k < funFieldLineObjects.Length; k++) {
			LineTrajectory ( funFieldLineObjects[k].gameObject, funQSignValue[k] );
		}

	}
// ------------------------------------------------------------------------------------------
	Vector3 FieldEval (GameObject[] funCharges, float[] funQ, Vector3 pointEval) {

		Vector3 parcialForce = Vector3.zero;

		for (int i = 0; i < funCharges.Length; i++) {

			float r = (charges[i].transform.position - pointEval).magnitude;
			Vector3 direction = (pointEval - charges [i].transform.position).normalized;
			parcialForce +=  q [i] / (r * r) * direction;

		}

		return parcialForce;
	}
// ------------------------------------------------------------------------------------------
	void LineTrajectory (GameObject lineObject, float qSign) {

		LineRenderer lineR = lineObject.GetComponent<LineRenderer> ();
		lineR.positionCount = 1;

		Vector3[] linePositions = new Vector3[numItera];
		linePositions[0] = lineObject.transform.position;

		int finalItera = numItera;
		for (int k = 1; k < numItera; k++) {

			Vector3 fEval = FieldEval (charges, q, linePositions[k-1]);
			linePositions [k] = linePositions [k - 1] + fEval.normalized * delta * qSign; 

			float dist = 100;
			for (int d = 0; d < charges.Length; d++) {
				dist = Mathf.Min (dist, (linePositions [k] - charges [d].transform.position).magnitude);
			}

			if (dist < (chargeRadio-delta) ){
				finalItera = k;
				break;
			}

		}
			
		Array.Resize(ref linePositions,finalItera);

		lineR.positionCount = linePositions.Length;
		lineR.SetPositions(linePositions);

		float tValue = (qSign == 1) ? t01 : 1-t01;  
		GradientColorKey progressGrad0 = new GradientColorKey(lineColor, tValue - 0.01f);
		GradientColorKey progressGrad1 = new GradientColorKey(effectColor, tValue);
		GradientColorKey progressGrad2 = new GradientColorKey(lineColor, tValue + 0.01f);
		
		grad.colorKeys = new GradientColorKey[] { startGrad, progressGrad0, progressGrad1, progressGrad2, endGrad };
		lineR.colorGradient = grad;

	}
// ------------------------------------------------------------------------------------------
	void Update () {

		float t = lineEffectSpeed * (Time.time);
		t01 = (t - Mathf.Floor (t));

		UpdateFieldLines (fieldLineObjects, qSignValue );

	}
}