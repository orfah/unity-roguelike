using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
	public static GameManager instance = null;
	public BoardManager boardScript;

	public int playerFoodPoints = 100;
	[HideInInspector] public bool playersTurn = true;

	public float levelStartDelay = 2f;
	public float gameOverRestartDelay = 2f;
	public float turnDelay = .1f;

	private List<Enemy> enemies;
	private bool enemiesMoving;

	private int level = 1;
	private Text levelText;
	private GameObject levelImage;
	private bool doingSetup;

	public AudioClip gameOverSound;

	// Use this for initialization
	void Awake () {
		if (instance == null) {
			instance = this;
		} else {
			Destroy (gameObject);
		}

		enemies = new List<Enemy> ();
		DontDestroyOnLoad (gameObject);
		boardScript = GetComponent<BoardManager> ();
		initGame ();
	}

	private void OnLevelWasLoaded(int index) {
		level++;
		initGame ();
	}

	void initGame() {
		doingSetup = true;
		levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text> ();
		levelText.text = "Day " + level;
		levelImage.SetActive (true);

		Invoke ("HideLevelImage", levelStartDelay);

		boardScript.SetupScene (level);
		enemies.Clear ();
	}

	void HideLevelImage() {
		doingSetup = false;
		levelImage.SetActive (false);
	}

	public void GameOver() {
		levelText.text = "After surviving " + level + " days, you starved.";
		levelImage.SetActive (true);
		enabled = false;

		SoundManager.instance.PlaySingle (gameOverSound);
		SoundManager.instance.musicSource.Stop ();
		Invoke ("ResetGame", gameOverRestartDelay);
	}

	public void ResetGame() {
		level = 0;
		playerFoodPoints = 100;
		playersTurn = true;
		enemiesMoving = false;

		SoundManager.instance.musicSource.Play ();
		enabled = true;

		Player p = GameObject.Find ("Player").GetComponent<Player>();
		p.Food(playerFoodPoints);
		
		Restart ();
	}

	public void Restart() {
		SceneManager.LoadScene (0);
	}
	
	void Update() {
		if (playersTurn || enemiesMoving || doingSetup) {
			return;
		}
		StartCoroutine (MoveEnemies());
	}

	public void AddEnemyToList(Enemy script) {
		enemies.Add (script);
	}

	IEnumerator MoveEnemies() {
		enemiesMoving = true;
		yield return new WaitForSeconds(turnDelay);

		if (enemies.Count == 0) {
			yield return new WaitForSeconds (turnDelay);
		}

		for (int i = 0; i < enemies.Count; i++) {
			enemies [i].MoveEnemy ();
			yield return new WaitForSeconds(enemies[i].moveTime);
		}

		playersTurn = true;
		enemiesMoving = false;
	}
}
