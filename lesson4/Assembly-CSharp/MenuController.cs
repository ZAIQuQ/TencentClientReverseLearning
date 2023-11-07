using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000008 RID: 8
public class MenuController : MonoBehaviour
{
	// Token: 0x06000031 RID: 49 RVA: 0x00002C0F File Offset: 0x00000E0F
	private void Awake()
	{
		this.MakeInstance();
	}

	// Token: 0x06000032 RID: 50 RVA: 0x00002C17 File Offset: 0x00000E17
	private void Start()
	{
		this.birds[GameControllers.instance.GetSelectedBird()].SetActive(true);
		this.CheckIfBirdsAreUnlocked();
	}

	// Token: 0x06000033 RID: 51 RVA: 0x00002C36 File Offset: 0x00000E36
	private void MakeInstance()
	{
		if (MenuController.instance == null)
		{
			MenuController.instance = this;
		}
	}

	// Token: 0x06000034 RID: 52 RVA: 0x00002C4E File Offset: 0x00000E4E
	private void CheckIfBirdsAreUnlocked()
	{
		if (GameControllers.instance.IsRedBirdUnlocked() == 1)
		{
			this.isRedBirdUnlocked = true;
		}
		if (GameControllers.instance.IsGreenBirdUnlocked() == 1)
		{
			this.isGreenBirdUnlocked = true;
		}
	}

	// Token: 0x06000035 RID: 53 RVA: 0x00002C7E File Offset: 0x00000E7E
	public void PlayGame()
	{
		SceneManager.LoadScene("GamePLayScene");
	}

	// Token: 0x06000036 RID: 54 RVA: 0x00002C8A File Offset: 0x00000E8A
	public void QuitGame()
	{
		Application.Quit();
	}

	// Token: 0x06000037 RID: 55 RVA: 0x00002C94 File Offset: 0x00000E94
	public void ChangeBird()
	{
		if (GameControllers.instance.GetSelectedBird() == 0)
		{
			if (this.isGreenBirdUnlocked)
			{
				this.birds[0].SetActive(false);
				GameControllers.instance.SetSelectedBird(1);
				this.birds[GameControllers.instance.GetSelectedBird()].SetActive(true);
			}
		}
		else if (GameControllers.instance.GetSelectedBird() == 1)
		{
			if (this.isRedBirdUnlocked)
			{
				this.birds[1].SetActive(false);
				GameControllers.instance.SetSelectedBird(2);
				this.birds[GameControllers.instance.GetSelectedBird()].SetActive(true);
			}
			else
			{
				this.birds[1].SetActive(false);
				GameControllers.instance.SetSelectedBird(0);
				this.birds[GameControllers.instance.GetSelectedBird()].SetActive(true);
			}
		}
		else if (GameControllers.instance.GetSelectedBird() == 2)
		{
			this.birds[2].SetActive(false);
			GameControllers.instance.SetSelectedBird(0);
			this.birds[GameControllers.instance.GetSelectedBird()].SetActive(true);
		}
	}

	// Token: 0x0400002A RID: 42
	public static MenuController instance;

	// Token: 0x0400002B RID: 43
	[SerializeField]
	private GameObject[] birds;

	// Token: 0x0400002C RID: 44
	private bool isGreenBirdUnlocked;

	// Token: 0x0400002D RID: 45
	private bool isRedBirdUnlocked;
}
