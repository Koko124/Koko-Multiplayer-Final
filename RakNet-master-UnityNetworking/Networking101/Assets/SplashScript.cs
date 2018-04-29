using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScript : MonoBehaviour {
	public float gameTimer;

	// Use this for initialization
	void Start () {
		gameTimer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		gameTimer += Time.deltaTime;

		if (gameTimer >= 2.5f) {
			SceneManager.LoadScene ("Networking");
		}
	}
}
