using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomMsgType
{
    public static short Transform = MsgType.Highest + 1;
};


public class PlayerController : NetworkBehaviour
{
    public float m_linearSpeed = 5.0f;
    public float m_angularSpeed = 3.0f;
    public float m_jumpSpeed = 5.0f;

	public bool spedUp;
	public float speedTimer;

    private Rigidbody m_rb = null;

	public float gameTimer;
	public Text timerText;

	public float score;
	public Text scoreText;

	public Text speedText;

	public bool gameOver;
	public Text playAgain;

	public int speedPowerUps;


	public GameObject speed1;
	public GameObject speed2;
	public GameObject speed3;
	public GameObject speed;

    [SyncVar]
    private bool m_hasFlag = false;

    public bool HasFlag() {
        return m_hasFlag;
    }

    [Command]
    public void CmdPickUpFlag()
    {
        m_hasFlag = true;
    }

    [Command]
    public void CmdDropFlag()
    {
        m_hasFlag = false;
    }

    bool IsHost()
    {
        return isServer && isLocalPlayer;
    }

    // Use this for initialization
    void Start() {
        m_rb = GetComponent<Rigidbody>();
        //Debug.Log("Start()");
        Vector3 spawnPoint;
		Vector3 spawnPt;
		spawnPt = new Vector3(Random.Range(-10f, 10f), 1, Random.Range(-10f, 10f));
		ObjectSpawner.RandomPoint(spawnPt, 10f, out spawnPoint);
        this.transform.position = spawnPoint;

		speed = GameObject.Find ("Speed");
		speed1 = GameObject.Find ("Speed1");
		speed2 = GameObject.Find ("Speed2");
		speed3 = GameObject.Find ("Speed3");

		m_hasFlag = false;

		speedPowerUps = 3;

        TrailRenderer tr = GetComponent<TrailRenderer>();
        tr.enabled = false;

		spedUp = false;
		speedTimer = 0;

		gameOver = false;

		timerText = GameObject.Find ("Time").GetComponent<Text> ();
		scoreText = GameObject.Find ("Points").GetComponent<Text> ();
		playAgain = GameObject.Find ("PlayAgain").GetComponent<Text> ();
		speedText = GameObject.Find ("Speeds").GetComponent<Text> ();

    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        //Debug.Log("OnStartAuthority()");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        //Debug.Log("OnStartClient()");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        //Debug.Log("OnStartLocalPlayer()");
		GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0, 250), Random.Range(0, 250), Random.Range(0, 250), 1f);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        //Debug.Log("OnStartServer()");
    }

    public void Jump()
    {
        Vector3 jumpVelocity = Vector3.up * m_jumpSpeed;
        m_rb.velocity += jumpVelocity;
        TrailRenderer tr = GetComponent<TrailRenderer>();
        tr.enabled = true;
    }

    [ClientRpc]
    public void RpcJump()
    {
        Jump();
    }

    [Command]
    public void CmdJump()
    {
        Jump();
        RpcJump();
    }

    // Update is called once per frame
    void Update() {
		gameTimer += Time.deltaTime;
		timerText.text = gameTimer.ToString ();
		scoreText.text = score.ToString ();
		speedText.text = speedPowerUps.ToString ();

		if (speed != null) {
			PickupSpeed ();
		}

		if (speed1 != null) {
			PickupSpeed1 ();
		}

		if (speed2 != null) {
			PickupSpeed2 ();
		}

		if (speed3 != null) {
			PickupSpeed3 ();
		}

        if (!isLocalPlayer)
        {
            return;
        }

		if (gameOver == true) {
			CheckPlayAgain ();
		}

		if (m_hasFlag == true) {
			score += Time.deltaTime;
		}

		if (score >= 7.5f) {
			WinGame ();
		}

        if (m_rb.velocity.y < Mathf.Epsilon) {
            TrailRenderer tr = GetComponent<TrailRenderer>();
            tr.enabled = false;
        }

		if (spedUp == true) {
			m_linearSpeed = 10f;
			speedTimer += Time.deltaTime;
		}

		if (speedTimer >= 4f) {
			m_linearSpeed = 5f;
			spedUp = false;
			speedTimer = 0;
		}

        float rotationInput = Input.GetAxis("Horizontal");
        float forwardInput = Input.GetAxis("Vertical");

        Vector3 linearVelocity = this.transform.forward * (forwardInput * m_linearSpeed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdJump();
        }

		if (Input.GetKeyDown(KeyCode.LeftShift)){
			SpeedUp();
		}

        float yVelocity = m_rb.velocity.y;


        linearVelocity.y = yVelocity;
        m_rb.velocity = linearVelocity;

        Vector3 angularVelocity = this.transform.up * (rotationInput * m_angularSpeed);
        m_rb.angularVelocity = angularVelocity;
    }

    [Command]
    public void CmdPlayerDropFlag()
    {
        Transform childTran = this.transform.GetChild(this.transform.childCount - 1);
        Flag flag = childTran.gameObject.GetComponent<Flag>();
		if (flag) {
			flag.CmdDropFlag();
		}
    }
	public void OnCollisionEnter(Collision other)
	{
		if(!isLocalPlayer || other.collider.tag != "Player")
		{
			return;
		}
			
		if (HasFlag()) {
			Transform childTran = this.transform.GetChild (this.transform.childCount - 1);
			if (childTran.gameObject.tag == "Flag") {
                CmdPlayerDropFlag();
            }
		}

		if (other.gameObject.name == "Speed") {
			speedPowerUps = 3;
			//Destroy (other.gameObject);
		}
			
	}
   
	void SpeedUp(){
		if (speedPowerUps > 0 && spedUp == false) {
			spedUp = true;
			speedPowerUps--;
		}
	}

	/*public void OnTrigerStay(Collider col){
		if (col.gameObject.tag == "Flag") {
			score += Time.deltaTime;
		}
			
		if (col.gameObject.tag == "Speed") {
			spedUp = true;
			Destroy (col.gameObject);
		}
	}*/

	public void PickupSpeed(){
		if (Vector3.Distance (speed.transform.position, this.transform.position) < 1.5f) {
			speedPowerUps = 3;
			Destroy (speed.gameObject);
		}
	}

	public void PickupSpeed1(){
		if (Vector3.Distance (speed1.transform.position, this.transform.position) < 1.5f) {
			speedPowerUps = 3;
			Destroy (speed1.gameObject);
		}
	}

	public void PickupSpeed2(){
		if (Vector3.Distance (speed2.transform.position, this.transform.position) < 1.5f) {
			speedPowerUps = 3;
			Destroy (speed2.gameObject);
		}
	}

	public void PickupSpeed3(){
		if (Vector3.Distance (speed3.transform.position, this.transform.position) < 1.5f) {
			speedPowerUps = 3;
			Destroy (speed3.gameObject);
		}
	}


	public void WinGame(){
		gameOver = true;
		playAgain.text = "You Win! Would you like to play again? Y)Yes / N)No";
	}

	public void CheckPlayAgain(){
		if (Input.GetKeyDown(KeyCode.Y)){
			gameOver = false;
			playAgain.text = " ";
			gameTimer = 0;
			speedPowerUps = 3;
			score = 0;
		}

		if (Input.GetKeyDown(KeyCode.N)){
			Application.Quit();
		}
	}
}
