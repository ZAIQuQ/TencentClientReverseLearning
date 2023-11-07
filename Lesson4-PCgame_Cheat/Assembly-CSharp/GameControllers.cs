using System;
using UnityEngine;

// Token: 0x02000006 RID: 6
public class GameControllers : MonoBehaviour
{
	// Token: 0x06000016 RID: 22 RVA: 0x000027C7 File Offset: 0x000009C7
	private void Awake()
	{
		this.MakeSingleton();
		this.isGameStartedFirstTime();
	}

	// Token: 0x06000017 RID: 23 RVA: 0x000027D5 File Offset: 0x000009D5
	private void Start()
	{
	}

	// Token: 0x06000018 RID: 24 RVA: 0x000027D7 File Offset: 0x000009D7
	private void MakeSingleton()
	{
		if (GameControllers.instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			GameControllers.instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	// Token: 0x06000019 RID: 25 RVA: 0x0000280C File Offset: 0x00000A0C
	private void isGameStartedFirstTime()
	{
		if (!PlayerPrefs.HasKey("isGameStartedFirstTime"))
		{
			PlayerPrefs.SetInt("High Score", 0);
			PlayerPrefs.SetInt("Selected Bird", 0);
			PlayerPrefs.SetInt("Green Bird", 1);
			PlayerPrefs.SetInt("Red Bird", 1);
			PlayerPrefs.SetInt("isGameStartedFirstTime", 0);
		}
	}

	// Token: 0x0600001A RID: 26 RVA: 0x0000285F File Offset: 0x00000A5F
	public void SetHighScore(int score)
	{
		PlayerPrefs.SetInt("High Score", score);
	}

	// Token: 0x0600001B RID: 27 RVA: 0x0000286C File Offset: 0x00000A6C
	public int GetHighScore()
	{
		return PlayerPrefs.GetInt("High Score");
	}

	// Token: 0x0600001C RID: 28 RVA: 0x00002878 File Offset: 0x00000A78
	public void SetSelectedBird(int selectedBird)
	{
		PlayerPrefs.SetInt("Selected Bird", selectedBird);
	}

	// Token: 0x0600001D RID: 29 RVA: 0x00002885 File Offset: 0x00000A85
	public int GetSelectedBird()
	{
		return PlayerPrefs.GetInt("Selected Bird");
	}

	// Token: 0x0600001E RID: 30 RVA: 0x00002891 File Offset: 0x00000A91
	public void UnlockGreenBird()
	{
		PlayerPrefs.SetInt("Green Bird", 1);
	}

	// Token: 0x0600001F RID: 31 RVA: 0x0000289E File Offset: 0x00000A9E
	public int IsGreenBirdUnlocked()
	{
		return PlayerPrefs.GetInt("Green Bird");
	}

	// Token: 0x06000020 RID: 32 RVA: 0x000028AA File Offset: 0x00000AAA
	public void UnlockRedBird()
	{
		PlayerPrefs.SetInt("Red Bird", 1);
	}

	// Token: 0x06000021 RID: 33 RVA: 0x000028B7 File Offset: 0x00000AB7
	public int IsRedBirdUnlocked()
	{
		return PlayerPrefs.GetInt("Red Bird");
	}

	// Token: 0x04000019 RID: 25
	public static GameControllers instance;

	// Token: 0x0400001A RID: 26
	private const string HIGH_SCORE = "High Score";

	// Token: 0x0400001B RID: 27
	private const string SELECTED_BIRD = "Selected Bird";

	// Token: 0x0400001C RID: 28
	private const string GREEN_BIRD = "Green Bird";

	// Token: 0x0400001D RID: 29
	private const string RED_BIRD = "Red Bird";
}
