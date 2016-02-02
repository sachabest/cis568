using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public KeyCode Toggle = KeyCode.LeftShift;
	public KeyCode AltToggle = KeyCode.RightShift;
	public KeyCode[] Gravity = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D };
    public Canvas DeadCanvas;
    public Canvas HUD;
    public Canvas GameOverCanvas;

    private float _gameTimer;
    private float _gravity = 4.0f;
    private float _score = 0.0f;
    private bool _inEndgame;

    private AudioSource _mainAudio;

    public Text ScoreTest;
    public Text TimeText;
    public Text GameOverText;

	public Avatar MainAvatar;

	public static GameManager instance;

	// Use this for initialization
	void Awake () {
		_mainAudio = GetComponent<AudioSource>();
		Reset();
        if (!GameManager.instance) {
        	GameManager.instance = this;
        } // destory handled by permanentdata
	}
	
	public void Reset() {
		_inEndgame = false;
        DeadCanvas.enabled = false;
        GameOverCanvas.enabled = false;
        HUD.enabled = true;
        _mainAudio.Stop();
        _mainAudio.Play();
        _gameTimer = 59f;
	}

    public void ShowDeadUI() {
        DeadCanvas.enabled = true;
    }

    public void AddScore(int toAdd) {
    	_score += toAdd;
    	ScoreTest.text = _score.ToString();
    }

    void FinalEndGame() {
    	_inEndgame = true;
    	HUD.enabled = false;
    	DeadCanvas.enabled = false;
    	GameOverText.text = "Game Over! Score: " + _score.ToString();
    	GameOverCanvas.enabled = true;
    }

	// Update is called once per frame
	void Update () {
		_gameTimer -= Time.deltaTime;
		if (_gameTimer <= 0 && !_inEndgame) {
			FinalEndGame();
			return;
		} else if (_inEndgame && Input.GetKey(KeyCode.Return)) {
			Application.LoadLevel(1);
		}
		TimeText.text = "0:" + ((int) _gameTimer).ToString();
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
