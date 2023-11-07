using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000009 RID: 9
public class MyCoroutine
{
	// Token: 0x06000039 RID: 57 RVA: 0x00002DC0 File Offset: 0x00000FC0
	public static IEnumerator WaitforRealSeconds(float time)
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
		yield break;
	}
}
