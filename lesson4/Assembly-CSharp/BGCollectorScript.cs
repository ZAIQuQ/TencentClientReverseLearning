using System;
using UnityEngine;

// Token: 0x02000004 RID: 4
public class BGCollectorScript : MonoBehaviour
{
	// Token: 0x06000010 RID: 16 RVA: 0x0000242C File Offset: 0x0000062C
	private void Awake()
	{
		this.backgrounds = GameObject.FindGameObjectsWithTag("Background");
		this.grounds = GameObject.FindGameObjectsWithTag("Ground");
		this.lastBGX = this.backgrounds[0].transform.position.x;
		this.lastGroundX = this.grounds[0].transform.position.x;
		for (int i = 1; i < this.backgrounds.Length; i++)
		{
			if (this.lastBGX < this.backgrounds[i].transform.position.x)
			{
				this.lastBGX = this.backgrounds[i].transform.position.x;
			}
		}
		for (int j = 1; j < this.grounds.Length; j++)
		{
			if (this.lastGroundX < this.grounds[j].transform.position.x)
			{
				this.lastGroundX = this.grounds[j].transform.position.x;
			}
		}
	}

	// Token: 0x06000011 RID: 17 RVA: 0x00002560 File Offset: 0x00000760
	private void OnTriggerEnter2D(Collider2D target)
	{
		if (target.tag == "Background")
		{
			Vector3 position = target.transform.position;
			float x = ((BoxCollider2D)target).size.x;
			position.x = this.lastBGX + x;
			target.transform.position = position;
			this.lastBGX = position.x;
		}
		else if (target.tag == "Ground")
		{
			Vector3 position2 = target.transform.position;
			float x2 = ((BoxCollider2D)target).size.x;
			position2.x = this.lastGroundX + x2;
			target.transform.position = position2;
			this.lastGroundX = position2.x;
		}
	}

	// Token: 0x04000010 RID: 16
	private GameObject[] backgrounds;

	// Token: 0x04000011 RID: 17
	private GameObject[] grounds;

	// Token: 0x04000012 RID: 18
	private float lastBGX;

	// Token: 0x04000013 RID: 19
	private float lastGroundX;
}
