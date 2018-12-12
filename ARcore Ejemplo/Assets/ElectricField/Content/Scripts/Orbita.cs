using UnityEngine;
using System.Collections;
 
public class Orbita : MonoBehaviour {

    public Transform target;
	Transform oldTarget;
    float dist;
    float xSpeed = 1.0f;
    float ySpeed = 1.0f;
	float ZSpeed = 10.0f;

    float yMinLimit = -80f;
    float yMaxLimit = 80f;

    float distMin = 0.1f;
    float distMax = 4f;

    float x;
    float y;

	float startTime;
	float inputSpeed = 1f;

// ========================================================================================================
	void Start () {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
 
		dist = (transform.position - target.position).magnitude;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;

		oldTarget = target;
	}
// ========================================================================================================
    public static float ClampAngle(float angle, float min, float max){
    
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;

		if (angle > 180)
			angle-=360;

        return Mathf.Clamp(angle, min, max);
    }
// ========================================================================================================
	void InputAction () {

	//salto entre targets
		RaycastHit hitt;

		if (Input.GetMouseButtonDown(0)) {

			if( Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out  hitt, 100 ) ) {

//				if(hitt.transform.name.Equals("charge")){
				if(hitt.transform.tag=="Charge"){
					oldTarget = target;
					target = hitt.transform;
					startTime = Time.time;
					dist = (transform.position - target.position).magnitude;

				}
			}
		}

		transform.LookAt(Vector3.Lerp(oldTarget.position,target.position,(Time.time - startTime) ));
		x = transform.eulerAngles.y;
		y = transform.eulerAngles.x;


	//Control de orbita
		//Zoom
		if(Input.GetAxis("Mouse ScrollWheel")!=0){

			//transform.Translate(0,0,Input.GetAxis("Mouse ScrollWheel")*ZSpeed);
			//		x = transform.eulerAngles.y;
			//		y = transform.eulerAngles.x;

			dist = Mathf.Clamp(dist - Input.GetAxis("Mouse ScrollWheel")*ZSpeed, distMin, distMax);
			Vector3 negDistance = new Vector3(0.0f, 0.0f, -dist);
			transform.position = Quaternion.Euler(y, x, 0) * negDistance + target.position;

		}

		//Orbita
		if (target&Input.GetMouseButton(1)) {

			if(Input.GetAxis("Mouse X")!=0|Input.GetAxis("Mouse Y")!=0){

				dist = (transform.position - target.position).magnitude;

				x += Input.GetAxis("Mouse X") * xSpeed * dist;
				y -= Input.GetAxis("Mouse Y") * ySpeed;
				y = ClampAngle(y, yMinLimit, yMaxLimit);
				//y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
				transform.rotation = Quaternion.Euler(y, x, 0);
				Vector3 negDist = new Vector3(0.0f, 0.0f, -dist);	
				transform.position = Quaternion.Euler(y, x, 0) * negDist + target.position;

			}
		}

		// movimiento de las cargas con el teclado
		if (Input.GetAxis ("Horizontal") != 0) {
			target.position += new Vector3 (inputSpeed*Input.GetAxis ("Horizontal")*Time.deltaTime, 0, 0);
		}
		if (Input.GetAxis ("Vertical") != 0) {
			target.position += new Vector3 (0, inputSpeed*Input.GetAxis ("Vertical")*Time.deltaTime, 0);
		}
		if (Input.GetAxis ("Profundidad") != 0) {
			target.position += new Vector3 (0, 0, inputSpeed*Input.GetAxis ("Profundidad")*Time.deltaTime);
		}


	}
// ========================================================================================================
    void Update () {
	
		InputAction ();

	}
// ========================================================================================================
}