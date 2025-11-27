using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerCard : MonoBehaviour
{

    public static GameManagerCard Instance;
    public Sprite cardBack;
    public Card cardPrefab;
    public Sprite[] cardFaces;
    private List<Card> cards;
    private List<int> cardIds;
    public Card firstCard, secondCard;
    public Transform cardHolder;
    public GameObject finalUI;
    public TextMeshProUGUI finalText;
    public TextMeshProUGUI TimerText;
    private int pairsMatched;
    private int totalPairs;
    private float timer;
    private bool isGameOver;
    private bool isLevelFinished;
    public float maxTime = 60f;


    public int score;
public int turnCount;
public int combo;
public TextMeshProUGUI scoreText;
public TextMeshProUGUI turnText;
public TextMeshProUGUI comboText;

[Header("Audio Clips")]

public AudioSource audioSource;
public AudioClip flipSound;
public AudioClip matchSound;
public AudioClip mismatchSound;
public AudioClip winSound;
public AudioClip loseSound;


    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        cards = new List<Card>();
        cardIds = new List<int>();
        pairsMatched = 0;
        totalPairs = cardFaces.Length / 2;

        timer = maxTime;
        isGameOver = false;
        isLevelFinished = false;

        score = 0;
turnCount = 0;
combo = 0;

UpdateUI();


        CreateCards();
        ShuffleCards();

        finalUI.gameObject.SetActive(false);
    }


void UpdateUI()
{
    scoreText.text = "Score: " + score;
    turnText.text = "Turns: " + turnCount;
    comboText.text = "Combo: " + combo;
}
    // Update is called once per frame
    void Update()
    {
       if(!isGameOver && !isLevelFinished)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                UpdateTimerText();
            }
            else
            {
                GameOver();
            }
        }
    }

    void CreateCards()
    {
        for (int i = 0; i <cardFaces.Length / 2; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        foreach (int id in cardIds)
        {
            Card newCard = Instantiate(cardPrefab, cardHolder);
            newCard.cardId = id;
            newCard.gameManager = this;
            cards.Add(newCard);
        }
    }

    void ShuffleCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = Random.Range(i, cardIds.Count);
            int temp = cardIds[i];
            cardIds[i] = cardIds[randomIndex];
            cardIds[randomIndex] = temp;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].cardId = cardIds[i];
        }
    }

    public void CardFlipped(Card flippedCard)
    {
        if (firstCard == null)
        {
            firstCard = flippedCard;
        }
        else if (secondCard == null)
        {
            secondCard = flippedCard;
            CheckMatch();
        }
    }

    void CheckMatch()
    {
         turnCount++; 
        if (firstCard.cardId == secondCard.cardId)
        {
                    audioSource.PlayOneShot(matchSound);

                    combo++;   // increase combo streak

        score += 10 * combo;
            pairsMatched++;

            UpdateUI();

            if (pairsMatched == totalPairs)
            {
                LevelFinished();
            }
            firstCard = null;
            secondCard = null;
        }
        else
        {
                    audioSource.PlayOneShot(mismatchSound);

                    combo = 0; // reset combo on fail

        UpdateUI();
            StartCoroutine(FlipBackCards());
        }
    }

    IEnumerator FlipBackCards()
    {
        yield return new WaitForSeconds(1f);
        firstCard.HideCard();
        secondCard.HideCard();
        firstCard = null;
        secondCard = null;
    }

    void GameOver()
    {
        isGameOver = true;
            audioSource.PlayOneShot(loseSound);

        FinalPanel();
    }

    void LevelFinished()
    {
        isLevelFinished = true;
            audioSource.PlayOneShot(winSound);

        FinalPanel();
    }

    void FinalPanel()
    {
        finalUI.gameObject.SetActive(true);
        if (isLevelFinished)
        {
            finalText.text = "Congratulations! You Win! " + Mathf.Round(timer) + " seconds remaining.";
        }
        else if (!isGameOver)
        {
            finalText.text = "Time's Up! Game Over!";
        }
    }

    public void RestartGame()
    {
        pairsMatched = 0;
        timer = maxTime;
        isGameOver = false;
        isLevelFinished = false;
        finalUI.gameObject.SetActive(false);
        score = 0;
turnCount = 0;
combo = 0;
UpdateUI();

        foreach(var card in cards)
        {
            Destroy(card.gameObject);
        }
        cards.Clear();
        cardIds.Clear();

        CreateCards();
        ShuffleCards();
    }

    void UpdateTimerText()
    {
        TimerText.text = "Time left : " + Mathf.Round(timer).ToString() + "s";
    }
}
