using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Player : MovingObject {
	public static Player instance = null;

	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;

	public Text foodText;

	private Animator animator; 
	private int food;

	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;

	private Vector2 touchOrigin = -Vector2.one;

	protected override void Start () {
		animator = GetComponent<Animator> ();
		food = GameManager.instance.playerFoodPoints;

		foodText = GameObject.Find ("FoodText").GetComponent<Text> ();
		foodText.text = "Food: " + food;
		base.Start ();
	}

	public void Food(int f) {
		food = f;
	}

	private void OnDisable() {
		GameManager.instance.playerFoodPoints = food;
	}
	
	void Update () {
		if (!GameManager.instance.playersTurn) {
			return;
		}

		int horizontal = 0;
		int vertical = 0;

	#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER

		horizontal = (int)Input.GetAxisRaw ("Horizontal");
		vertical = (int)Input.GetAxisRaw ("Vertical");

		if (horizontal != 0) {
			vertical = 0;
		}

	#else
		if (Input.touchCount > 0) {
			Touch myTouch = Input.touches[0];

			if (myTouch.phase == TouchPhase.Began) {
				touchOrigin = myTouch.position;
			} else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
				Vector2 touchEnd = myTouch.position;
				float x = touchEnd.x - touchOrigin.x;
				float y = touchEnd.y - touchOrigin.y;
				touchOrigin.x = -1;

				if (Mathf.Abs(x) > Mathf.Abs(y)) {
					horizontal = x > 0 ? 1 : -1;
				} else {
					vertical = y > 0 ? 1 : -1;
				}
			}
		}
	#endif

		if (horizontal != 0 || vertical != 0) {
			AttemptMove<Wall> (horizontal, vertical);
		}
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Exit") {
			GameManager.instance.Restart ();
			enabled = false;
		} else if (other.tag == "Food") {
			food += pointsPerFood;
			foodText.text = "+" + pointsPerFood + " Food: " + food;
			SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);
			other.gameObject.SetActive(false);
		} else if (other.tag == "Soda") {
			food += pointsPerSoda;
			foodText.text = "+" + pointsPerSoda + " Food: " + food;
			SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);
			other.gameObject.SetActive(false);
		}
	}

	protected override bool AttemptMove<T> (int xDir, int yDir)
	{
		food--; 
		foodText.text = "Food: " + food;
		RaycastHit2D hit;

		if (Move (xDir, yDir, out hit)) {
			SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
		}
		base.AttemptMove<T> (xDir, yDir);

		CheckIfGameOver ();
		GameManager.instance.playersTurn = false;
		return true;
	}

	protected override void OnCantMove<T> (T component)
	{
		Wall hitWall = component as Wall;
		hitWall.DamageWall (wallDamage);
		animator.SetTrigger ("playerChop");
	}

	public void LoseFood(int loss) {
		animator.SetTrigger ("playerHit");
		food -= loss;
		foodText.text = "-" + loss + " Food: " + food;
		CheckIfGameOver ();
	}

	private void CheckIfGameOver() {
		if (food <= 0) {
			GameManager.instance.GameOver ();
		}
	}
}
