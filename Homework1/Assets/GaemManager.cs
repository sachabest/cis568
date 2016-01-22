using UnityEngine;
using System.Collections;

public class GaemManager : MonoBehaviour {

	public KeyCode Toggle = KeyCode.LeftShift;
	public KeyCode AltToggle = KeyCode.RightShift;
	public KeyCode[] Gravity = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D };

	public Avatar MainAvatar;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(Toggle) || Input.GetKey(AltToggle)) {
			if (Input.GetKeyDown(Gravity[0])) {
				MainAvatar.CurrentGravityChange = GravityChange.X;
				Physics.gravity = new Vector3(0, 9.8f, 0);
			}
			if (Input.GetKeyDown(Gravity[1])) {
				MainAvatar.CurrentGravityChange = GravityChange.nX;
				Physics.gravity = new Vector3(0, -9.8f, 0);
			}
			if (Input.GetKeyDown(Gravity[2])) {
				MainAvatar.CurrentGravityChange = GravityChange.Y;
				Physics.gravity = new Vector3(0, 0, -9.8f);
			}
			if (Input.GetKeyDown(Gravity[3])) {
				MainAvatar.CurrentGravityChange = GravityChange.nY;
				Physics.gravity = new Vector3(0, 0, 9.8f);
			}
		}
	}
}
