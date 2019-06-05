using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMiniGame: MiniGame
{
	public enum State
	{
		RotateAnim,
		WaitTurnOver,
		FinishAnim,
		Finish,
	}
	
	public struct PlayResult
	{
		public PlayResult(int s, int v)
		{
			suit = s;
			val = v;
		}

		public int suit, val;
	}

	private int _spadesCount = 0, _heartsCount = 0;

	private CardMiniGameScreen gameScreen;
	private State _state = State.Finish;

	public State state { get { return _state; } }
	public int spadesCount { get { return _spadesCount; } }
	public int heartsCount { get { return _heartsCount; } }

	private List<PlayResult> playedGames = new List<PlayResult>();

	public void SetResources(int spdcnt, int hrtcnt)
	{
		_spadesCount = spdcnt;
		_heartsCount = hrtcnt;
	}

	public override void Build()
	{
		// загружаем из ресурсов префаб экрана игры
		GameObject prefab = Resources.Load<GameObject>("CardMiniGame");

		// создаем экземпляр экрана игры
		GameObject go = (GameObject)GameObject.Instantiate(prefab);

		go.transform.SetParent(App.instance.canvas.transform);
		go.transform.localScale = Vector3.one;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;

		RectTransform rt = go.GetComponent<RectTransform>();
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;

		gameScreen = go.GetComponent<CardMiniGameScreen>();
		gameScreen.Init(this);
		gameScreen.UpdateCountTexts();
	}

	public override void Destroy()
	{
		// удаляем ресурсы
		GameObject.Destroy(gameScreen.gameObject);
		Resources.UnloadUnusedAssets();
	}

	public void Mix()
	{
		// Перемешиваем
		if(_state == State.Finish)
		{
			_state = State.RotateAnim;
			gameScreen.StartRotateAnim();
		}
	}

	public void FinishedRotateAnim()
	{
		_state = State.WaitTurnOver;
	}

	public void TurnOver(int idx)
	{
		// Переворачиваем
		if(_state == State.WaitTurnOver)
		{
			_state = State.FinishAnim;

			int suit;
			int val;
			if(playedGames.Count == 2)
			{
				// проверяем масть последних 2х игр
				if(playedGames[0].suit == playedGames[1].suit)
				{
					suit = (playedGames[0].suit + 1) % 2;
				}
				else
				{
					suit = Random.Range(0, 2);
				}
				// проверяем сумму выигрышей
				int total = (4 + playedGames[0].val * 2) + (4 + playedGames[1].val * 2);

				if(total == 8)
				{
					val = 2;
				}
				else if(total == 10)
				{
					val = 3;
				}
				else
				{
					val = Random.Range(0, 4);
				}
			}
			else
			{
				// меньше двух игр сыграно
				suit = Random.Range(0, 2);
				val = Random.Range(0, 4);
			}

			playedGames.Add(new PlayResult(suit, val));
			// держим в уме только последние 2 игры
			if(playedGames.Count == 3)
			{
				playedGames.RemoveAt(0);
			}

			gameScreen.StartFinishAnim(idx, suit, val);

			if(suit == 0)
			{
				_spadesCount += 4 + val * 2;
			}
			else
			{
				_heartsCount += 4 + val * 2;
			}
		}
	}

	public void FinishedFinishAnim()
	{
		_state = State.Finish;
	}
}
