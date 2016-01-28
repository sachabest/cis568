using UnityEngine;
using System.Collections;

public class SkateboardPhysicsManager : MonoBehaviour {

	private Collider _skateboardCollider;

	// Use this for initialization
	void Start () {
		_skateboardCollider = GetComponent<BoxCollider>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision) {
    	foreach (ContactPoint point in collision.contacts) {
    		var thisCollider = point.thisCollider;
			if (thisCollider.GetType() == typeof(CapsuleCollider) && point.otherCollider.gameObject.name == "pPlane2") {
    			// now we have to detect whether or not this should be a valid landing
    			this.transform.up = Vector3.up;
    			break;
    		}
    	}
	}
}
