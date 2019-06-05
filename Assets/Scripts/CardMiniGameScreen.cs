using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardMiniGameScreen : MonoBehaviour
{
	public class Card
	{
		// для облегчения доступа к компонентам
		public GameObject gameObject;
		public Button button;
		public Image image;
		public RectTransform rectTransform;
		public bool updated = false;
	}

	public Text spadesCountTextPortrait, heartsCountTextPortrait, spadesCountTextLandscape, heartsCountTextLandscape;
	public GameObject cardPrefab, mixButton, waitImage, turnOverImage, portraitBar, landscapeBar;
	public Sprite[] spadesCardsSprites, heartsCardsSprites;
	public Sprite cardBackSprite;
	public float cardRadius = 500, scale = 0.60f;
	//
	private Card[] cards;
	private CardMiniGame game;

	private int prevWidth, prevHeight;

    public void Init(CardMiniGame cmg)
    {
		game = cmg;
	}

	public void ExitButtonClick()
	{
		// закрываем мини игру
		App.instance.StopMiniGame();
	}

	public void UpdateCountTexts()
	{
		// обновление текстов счетчиков
		spadesCountTextPortrait.text = game.spadesCount.ToString();
		heartsCountTextPortrait.text = game.heartsCount.ToString();
		spadesCountTextLandscape.text = game.spadesCount.ToString();
		heartsCountTextLandscape.text = game.heartsCount.ToString();
	}

	private void Start()
	{
		mixButton.SetActive(true);
		waitImage.SetActive(false);
		turnOverImage.SetActive(false);

		// создаем карты из заготовки
		cards = new Card[8];
		for(int i = 0; i < cards.Length; ++i)
		{
			Card card = new Card();
			card.gameObject = (GameObject)GameObject.Instantiate(cardPrefab, cardPrefab.transform.parent);
			card.gameObject.name = string.Format("Card_{0}", i); // для отладки дадим имя
			card.gameObject.SetActive(true);

			card.rectTransform = card.gameObject.GetComponent<RectTransform>();
			card.button = card.gameObject.GetComponent<Button>();
			card.image = card.gameObject.GetComponent<Image>();

			float ang = 0.5f * Mathf.PI + i * 2 * Mathf.PI / (float)cards.Length;
			card.rectTransform.anchoredPosition = new Vector2(cardRadius * Mathf.Cos(ang), cardRadius * Mathf.Sin(ang));
			card.gameObject.transform.localScale = scale * Vector3.one;

			card.image.sprite = GetRandomCardSprite();

			int cardIdx = i;
			card.button.onClick.AddListener(() => { TurnOverButtonClick(cardIdx); });

			cards[i] = card;
		}
		// переместим статическую карту в конец иерархии чтоб рисовалась поверх остальных
		cards[0].rectTransform.SetAsLastSibling();
	}


	private Sprite GetRandomCardSprite()
	{
		if(Random.Range(0, 10) % 2 == 0)
		{
			return spadesCardsSprites[Random.Range(0, spadesCardsSprites.Length)];
		}
		else
		{
			return heartsCardsSprites[Random.Range(0, heartsCardsSprites.Length)];
		}
	}

	public void MixButtonClick()
	{
		game.Mix();
	}

	public void TurnOverButtonClick(int idx)
	{
		game.TurnOver(idx);
	}

	public void StartRotateAnim()
	{
		mixButton.SetActive(false);
		waitImage.SetActive(true);

		StartCoroutine(RotateAnim());
	}

	public IEnumerator RotateAnim()
	{
		float timer = 0, timeSpan = 2;
		float rot;

		foreach(Card card in cards)
		{
			card.updated = false;
		}

		while(timer <= timeSpan)
		{

			rot = -2 * Mathf.PI * timer / timeSpan;
			for(int i = 1; i < cards.Length; ++i)
			{
				Card card = cards[i];

				float ang = rot + 0.5f * Mathf.PI + i * 2 * Mathf.PI / (float)cards.Length;
				card.rectTransform.anchoredPosition = new Vector2(cardRadius * Mathf.Cos(ang), cardRadius * Mathf.Sin(ang));

				if(ang <= 0.55f * Mathf.PI && !card.updated)
				{
					card.updated = true;
					//card.image.sprite = cardBackSprite;
					StartCoroutine(FlipCard(card, cardBackSprite));
				}
			}

			timer += Time.deltaTime;
			yield return 0;
		}

		cards[0].updated = false;
		StartCoroutine(FlipCard(cards[0], cardBackSprite));

		waitImage.SetActive(false);
		turnOverImage.SetActive(true);
		game.FinishedRotateAnim();
	}

	public IEnumerator FlipCard(Card card, Sprite spr)
	{
		float timer = 0, timeSpan = 0.25f;

		while(timer <= timeSpan)
		{
			float t = timer / timeSpan;
			card.rectTransform.localScale = Vector3.Lerp(new Vector3(scale, scale, scale), new Vector3(0.0f, scale, scale), t);
			timer += Time.deltaTime;
			yield return 0;
		}
		timer = 0;

		card.image.sprite = spr;

		while(timer <= timeSpan)
		{
			float t = timer / timeSpan;
			card.rectTransform.localScale = Vector3.Lerp(new Vector3(0.0f, scale, scale), new Vector3(scale, scale, scale), t);
			timer += Time.deltaTime;
			yield return 0;
		}
	}

	public void StartFinishAnim(int ci, int tos, int tov)
	{
		StartCoroutine(FinishAnim(ci, tos, tov));
	}

	public IEnumerator FinishAnim(int ci, int tos, int tov)
	{
		Sprite spr = null;

		if(tos == 0)
		{
			spr = spadesCardsSprites[tov];
		}
		else
		{
			spr = heartsCardsSprites[tov];
		}

		yield return StartCoroutine(FlipCard(cards[ci], spr));

		UpdateCountTexts();

		float timer = 0, timeSpan = 2.0f;
		while(timer <= timeSpan)
		{
			float t = timer / timeSpan;
			timer += Time.deltaTime;
			yield return 0;
		}
		timer = 0;

		for(int i = 0; i < cards.Length; ++i)
		{
			if(i != ci)
			{
				StartCoroutine(FlipCard(cards[i], GetRandomCardSprite()));
			}
		}

		
		turnOverImage.SetActive(false);
		mixButton.SetActive(true);
		game.FinishedFinishAnim();
	}

	private void Update()
	{
		if(Screen.width != prevWidth || Screen.height != prevHeight)
		{
			if(Screen.width < Screen.height)
			{
				landscapeBar.SetActive(false);
				portraitBar.SetActive(true);
			}
			else
			{
				landscapeBar.SetActive(true);
				portraitBar.SetActive(false);
			}

			prevWidth = Screen.width;
			prevHeight = Screen.height;
		}
	}
}