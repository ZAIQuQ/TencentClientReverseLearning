using System;
using UnityEngine;

// Token: 0x02000005 RID: 5
public class PipeCollectorScript : MonoBehaviour
{
	// Token: 0x06000013 RID: 19 RVA: 0x00002658 File Offset: 0x00000858
	private void Awake()
	{
		this.pipeHolders = GameObject.FindGameObjectsWithTag("PipeHolder");
		for (int i = 0; i < this.pipeHolders.Length; i++)
		{
			Vector3 position = this.pipeHolders[i].transform.position;
			position.y = UnityEngine.Random.Range(this.pipeMin, this.pipeMax);
			this.pipeHolders[i].transform.position = position;
		}
		this.lastPipeX = this.pipeHolders[0].transform.position.x;
		for (int j = 0; j < this.pipeHolders.Length; j++)
		{
			if (this.lastPipeX < this.pipeHolders[j].transform.position.x)
			{
				this.lastPipeX = this.pipeHolders[j].transform.position.x;
			}
		}
	}

	// Token: 0x06000014 RID: 20 RVA: 0x0000274C File Offset: 0x0000094C
	private void OnTriggerEnter2D(Collider2D target)
	{
		if (target.tag == "PipeHolder")
		{
			Vector3 position = target.transform.position;
			position.x = this.lastPipeX + this.distance;
			position.y = UnityEngine.Random.Range(this.pipeMin, this.pipeMax);
			target.transform.position = position;
			this.lastPipeX = position.x;
		}
	}

	// Token: 0x04000014 RID: 20
	private GameObject[] pipeHolders;

	// Token: 0x04000015 RID: 21
	private float distance = 3f;

	// Token: 0x04000016 RID: 22
	private float lastPipeX;

	// Token: 0x04000017 RID: 23
	private float pipeMin = -1f;

	// Token: 0x04000018 RID: 24
	private float pipeMax = 2.3f;
}
