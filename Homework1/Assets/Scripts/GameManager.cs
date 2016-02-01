using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public KeyCode Toggle = KeyCode.LeftShift;
	public KeyCode AltToggle = KeyCode.RightShift;
	public KeyCode[] Gravity = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D };
    public Canvas DeadCanvas;

    private float _gravity = 4.0f;
    private float _score = 0.0f;

    public Text ScoreTest;
	public Avatar MainAvatar;

	public static GameManager instance;

	// Use this for initialization
	void Awake () {
        DeadCanvas = GameObject.Find("DeadUI").GetComponent<Canvas>();
        DeadCanvas.enabled = false;
        if (!GameManager.instance) {
        	instance = this;
        } // destory handled by permanentdata
	}
	
    public void ShowDeadUI() {
        DeadCanvas.enabled = true;
    }

    public void AddScore(int toAdd) {
    	_score += toAdd;
    	ScoreTest.text = _score.ToString();
    }

	// Update is called once per frame
	void Update () {
        if (DeadCanvas.enabled && Input.GetKeyUp("space"))
        {
            Application.LoadLevel(Application.loadedLevel);
            DeadCanvas.enabled = false;
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
