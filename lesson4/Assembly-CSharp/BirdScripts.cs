using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000002 RID: 2
public class BirdScripts : MonoBehaviour
{
	// Token: 0x06000002 RID: 2
	private void Awake()
	{
		if (BirdScripts.instance == null)
		{
			BirdScripts.instance = this;
		}
		this.isAlive = true;
		this.score = 0;
		this.flapButton = GameObject.FindGameObjectWithTag("FlapButton").GetComponent<Button>();
		this.flapButton.onClick.AddListener(delegate()
		{
			this.flapTheBird();
		});
		this.CameraX();
	}

	// Token: 0x06000003 RID: 3
	private void Start()
	{
	}

	// Token: 0x06000004 RID: 4
	private void FixedUpdate()
	{
		if (this.isAlive)
		{
			Vector3 position = base.transform.position;
			position.x += this.forwardSpeed * Time.deltaTime;
			base.transform.position = position;
			if (this.didFlap)
			{
				this.didFlap = false;
				this.myRigidBody.velocity = new Vector2(0f, this.bounceSpeed);
				this.audioSource.PlayOneShot(this.flapClick);
				this.anim.SetTrigger("Flap");
			}
			if (this.myRigidBody.velocity.y >= 0f)
			{
				base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				return;
			}
			float z = Mathf.Lerp(0f, -70f, -this.myRigidBody.velocity.y / 7f);
			base.transform.rotation = Quaternion.Euler(0f, 0f, z);
		}
	}

	// Token: 0x06000005 RID: 5
	private void CameraX()
	{
		CameraScript.offsetX = Camera.main.transform.position.x - base.transform.position.x - 1f;
	}

	// Token: 0x06000006 RID: 6
	public float GetPositionX()
	{
		return base.transform.position.x;
	}

	// Token: 0x06000007 RID: 7
	public void flapTheBird()
	{
		this.didFlap = true;
	}

	// Token: 0x06000008 RID: 8
	private void OnCollisionEnter2D(Collision2D target)
	{
		if (target.gameObject.tag == "Flag" && this.isAlive)
		{
			this.isAlive = false;
			this.audioSource.PlayOneShot(this.cheerClip);
			GamePlayController.instance.finishGame();
		}
	}

	// Token: 0x06000009 RID: 9
	private void OnTriggerEnter2D(Collider2D target)
	{
		if (target.tag == "PipeHolder")
		{
			this.audioSource.PlayOneShot(this.pointClip);
			this.score += 114514;
			GamePlayController.instance.setScore(this.score);
		}
	}

	// Token: 0x04000001 RID: 1
	public static BirdScripts instance;

	// Token: 0x04000002 RID: 2
	[SerializeField]
	public Rigidbody2D myRigidBody;

	// Token: 0x04000003 RID: 3
	[SerializeField]
	private Animator anim;

	// Token: 0x04000004 RID: 4
	private float forwardSpeed = 3f;

	// Token: 0x04000005 RID: 5
	private float bounceSpeed = 4f;

	// Token: 0x04000006 RID: 6
	private bool didFlap;

	// Token: 0x04000007 RID: 7
	public bool isAlive;

	// Token: 0x04000008 RID: 8
	private Button flapButton;

	// Token: 0x04000009 RID: 9
	[SerializeField]
	private AudioSource audioSource;

	// Token: 0x0400000A RID: 10
	[SerializeField]
	private AudioClip flapClick;

	// Token: 0x0400000B RID: 11
	[SerializeField]
	private AudioClip pointClip;

	// Token: 0x0400000C RID: 12
	[SerializeField]
	private AudioClip diedClip;

	// Token: 0x0400000D RID: 13
	[SerializeField]
	private AudioClip cheerClip;

	// Token: 0x0400000E RID: 14
	public int score;
}
