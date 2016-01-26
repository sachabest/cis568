using UnityEngine;

public class GameManager : MonoBehaviour {

	public KeyCode Toggle = KeyCode.LeftShift;
	public KeyCode AltToggle = KeyCode.RightShift;
	public KeyCode[] Gravity = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D };
    public Canvas DeadCanvas;

    private float _gravity = 4.0f;
	public Avatar MainAvatar;

	// Use this for initialization
	void Start () {
        DeadCanvas.enabled = false;
	}
	
    public void ShowDeadUI() {
        DeadCanvas.enabled = true;
    }

	// Update is called once per frame
	void Update () {
        if (DeadCanvas.enabled && Input.GetKeyUp("space"))
        {
            Application.LoadLevel(Application.loadedLevel);
        }
		if (Input.GetKey(Toggle) || Input.GetKey(AltToggle)) {
			if (Input.GetKeyDown(Gravity[0])) {
				MainAvatar.CurrentGravityChange = GravityChange.X;
				Physics.gravity = new Vector3(0, _gravity, 0);
			}
			if (Input.GetKeyDown(Gravity[1])) {
				MainAvatar.CurrentGravityChange = GravityChange.nX;
				Physics.gravity = new Vector3(0, -_gravity, 0);
			}
			if (Input.GetKeyDown(Gravity[2])) {
				MainAvatar.CurrentGravityChange = GravityChange.Y;
				Physics.gravity = new Vector3(0, 0, -_gravity);
			}
			if (Input.GetKeyDown(Gravity[3])) {
				MainAvatar.CurrentGravityChange = GravityChange.nY;
				Physics.gravity = new Vector3(0, 0, _gravity);
			}
		}
	}
}
