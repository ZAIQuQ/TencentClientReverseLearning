using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x02000007 RID: 7
public class GamePlayController : MonoBehaviour
{
	// Token: 0x06000023 RID: 35 RVA: 0x000028CB File Offset: 0x00000ACB
	private void Awake()
	{
		this.MakeInstance();
		Time.timeScale = 0f;
	}

	// Token: 0x06000024 RID: 36 RVA: 0x000028DD File Offset: 0x00000ADD
	private void Start()
	{
	}

	// Token: 0x06000025 RID: 37 RVA: 0x000028DF File Offset: 0x00000ADF
	private void MakeInstance()
	{
		if (GamePlayController.instance == null)
		{
			GamePlayController.instance = this;
		}
	}

	// Token: 0x06000026 RID: 38 RVA: 0x000028F8 File Offset: 0x00000AF8
	public void pauseGame()
	{
		if (BirdScripts.instance != null && BirdScripts.instance.isAlive)
		{
			this.pausePanel.SetActive(true);
			this.finishPanel.SetActive(false);
			this.gameOverText.gameObject.SetActive(false);
			this.endScore.text = string.Empty + BirdScripts.instance.score;
			this.bestScore.text = string.Empty + GameControllers.instance.GetHighScore();
			Time.timeScale = 0f;
			this.restartGameButton.onClick.RemoveAllListeners();
			this.restartGameButton.onClick.AddListener(delegate()
			{
				this.resumeGame();
			});
		}
	}

	// Token: 0x06000027 RID: 39 RVA: 0x000029CB File Offset: 0x00000BCB
	public void goToMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}

	// Token: 0x06000028 RID: 40 RVA: 0x000029D7 File Offset: 0x00000BD7
	public void resumeGame()
	{
		this.pausePanel.SetActive(false);
		Time.timeScale = 1f;
	}

	// Token: 0x06000029 RID: 41 RVA: 0x000029EF File Offset: 0x00000BEF
	public void restartGame()
	{
		SceneManager.LoadScene("GamePlayScene");
	}

	// Token: 0x0600002A RID: 42 RVA: 0x000029FC File Offset: 0x00000BFC
	public void playGame()
	{
		this.scoreText.gameObject.SetActive(true);
		this.birds[GameControllers.instance.GetSelectedBird()].SetActive(true);
		this.instructionButton.gameObject.SetActive(false);
		Time.timeScale = 1f;
	}

	// Token: 0x0600002B RID: 43 RVA: 0x00002A4C File Offset: 0x00000C4C
	public void setScore(int score)
	{
		this.scoreText.text = string.Empty + score;
	}

	// Token: 0x0600002C RID: 44 RVA: 0x00002A69 File Offset: 0x00000C69
	public void finishGame()
	{
		this.pausePanel.SetActive(false);
		this.finishPanel.SetActive(true);
	}

	// Token: 0x0600002D RID: 45 RVA: 0x00002A84 File Offset: 0x00000C84
	public void playerDiedShowScore(int score)
	{
		this.pausePanel.SetActive(true);
		this.finishPanel.SetActive(false);
		this.gameOverText.gameObject.SetActive(true);
		this.scoreText.gameObject.SetActive(false);
		this.endScore.text = string.Empty + score;
		if (score > GameControllers.instance.GetHighScore())
		{
			GameControllers.instance.SetHighScore(score);
		}
		this.bestScore.text = string.Empty + GameControllers.instance.GetHighScore();
		if (score <= 20)
		{
			this.medalImage.sprite = this.medals[0];
		}
		else if (score > 20 && score < 40)
		{
			this.medalImage.sprite = this.medals[1];
			if (GameControllers.instance.IsGreenBirdUnlocked() == 0)
			{
				GameControllers.instance.UnlockGreenBird();
			}
		}
		else
		{
			this.medalImage.sprite = this.medals[2];
			if (GameControllers.instance.IsGreenBirdUnlocked() == 0)
			{
				GameControllers.instance.UnlockGreenBird();
			}
			if (GameControllers.instance.IsRedBirdUnlocked() == 0)
			{
				GameControllers.instance.UnlockRedBird();
			}
		}
		this.restartGameButton.onClick.RemoveAllListeners();
		this.restartGameButton.onClick.AddListener(delegate()
		{
			this.restartGame();
		});
	}

	// Token: 0x0400001E RID: 30
	public static GamePlayController instance;

	// Token: 0x0400001F RID: 31
	[SerializeField]
	private Text scoreText;

	// Token: 0x04000020 RID: 32
	[SerializeField]
	private Text endScore;

	// Token: 0x04000021 RID: 33
	[SerializeField]
	private Text bestScore;

	// Token: 0x04000022 RID: 34
	[SerializeField]
	private Text gameOverText;

	// Token: 0x04000023 RID: 35
	[SerializeField]
	private Button restartGameButton;

	// Token: 0x04000024 RID: 36
	[SerializeField]
	private Button instructionButton;

	// Token: 0x04000025 RID: 37
	[SerializeField]
	private GameObject pausePanel;

	// Token: 0x04000026 RID: 38
	[SerializeField]
	private GameObject finishPanel;

	// Token: 0x04000027 RID: 39
	[SerializeField]
	private GameObject[] birds;

	// Token: 0x04000028 RID: 40
	[SerializeField]
	private Sprite[] medals;

	// Token: 0x04000029 RID: 41
	[SerializeField]
	private Image medalImage;
}
