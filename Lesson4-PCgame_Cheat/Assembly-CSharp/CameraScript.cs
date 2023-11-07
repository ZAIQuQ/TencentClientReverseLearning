using System;
using UnityEngine;

// Token: 0x02000003 RID: 3
public class CameraScript : MonoBehaviour
{
	// Token: 0x0600000C RID: 12 RVA: 0x000023BD File Offset: 0x000005BD
	private void Start()
	{
	}

	// Token: 0x0600000D RID: 13 RVA: 0x000023BF File Offset: 0x000005BF
	private void Update()
	{
		if (BirdScripts.instance != null && BirdScripts.instance.isAlive)
		{
			this.MoveTheCamera();
		}
	}

	// Token: 0x0600000E RID: 14 RVA: 0x000023E8 File Offset: 0x000005E8
	private void MoveTheCamera()
	{
		Vector3 position = base.transform.position;
		position.x = BirdScripts.instance.GetPositionX() + CameraScript.offsetX;
		base.transform.position = position;
	}

	// Token: 0x0400000F RID: 15
	public static float offsetX;
}
