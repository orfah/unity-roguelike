using UnityEngine;
using System.Collections;

public class Enemy : MovingObject {
	public int playerDamage;
	public AudioClip attackSound1;
	public AudioClip attackSound2;

	private Animator animator;
	private Transform target;
	private bool skipMove;

	protected override void Start () {
		GameManager.instance.AddEnemyToList (this);
		animator = GetComponent<Animator> ();
		target = GameObject.FindGameObjectWithTag ("Player").transform;
		base.Start ();
	}

	public void MoveEnemy() {
		if (skipMove) {
			skipMove = false;
			return;
		}

		int xDir = 0;
		int yDir = 0;

		bool moved = false;
		if (Mathf.Abs (target.position.y - transform.position.y) > float.Epsilon) {
			yDir = target.position.y > transform.position.y ? 1 : -1;
			moved = AttemptMove<Player> (xDir, yDir);
		}
		
		if (!moved && Mathf.Abs (target.position.x - transform.position.x) > float.Epsilon) {
			xDir = target.position.x > transform.position.x ? 1 : -1;
			AttemptMove<Player> (xDir, 0);
		}

		skipMove = true;
	}

	protected override void OnCantMove<T> (T component) {
		Player hitPlayer = component as Player;
		hitPlayer.LoseFood (playerDamage);
		animator.SetTrigger ("enemyAttack");
		SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
	}
}
