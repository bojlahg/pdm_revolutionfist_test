using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
{
	private static App _instance;
	public static App instance { get { return _instance; } }

	public StartScreen startScreen;
	public RectTransform canvas;
	//
	private MiniGame game;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
    {
		startScreen.gameObject.SetActive(true);
	}

	public void StartMiniGame()
	{
		// Запускаем мини игру
		startScreen.gameObject.SetActive(false);
		CardMiniGame cmg = new CardMiniGame();
		//
		cmg.SetResources(5, 10);
		cmg.Build();

		game = cmg;
	}

	public void StopMiniGame()
	{
		// Останавилваем мини игру
		game.Destroy();
		game = null;

		startScreen.gameObject.SetActive(true);
	}
}
