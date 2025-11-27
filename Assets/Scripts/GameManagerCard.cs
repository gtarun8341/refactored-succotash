using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerCard : MonoBehaviour
{
    public static GameManagerCard Instance;

    [Header("Cards")]
    public Sprite cardBack;
    public Card cardPrefab;
    public Sprite[] cardFaces;

    private List<Card> cards = new List<Card>();
    private List<int> cardIds = new List<int>();

    [Header("UI")]
    public Transform cardHolder;
    public GameObject finalUI;
    public TextMeshProUGUI finalText;
    public TextMeshProUGUI TimerText;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI comboText;

    [Header("Game State")]
    private int pairsMatched;
    private int totalPairs;

    private float timer;
    public float maxTime = 60f;
    private bool isGameOver;
    private bool isLevelFinished;

    public int score;
    public int turnCount;
    public int combo;

    [Header("Grid Settings")]
    public int rows = 4;
    public int columns = 4;
    public float spacing = 10f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    private Queue<Card> flipQueue = new Queue<Card>();
    private bool isProcessing = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        finalUI.SetActive(false);

        score = combo = turnCount = 0;
        timer = maxTime;
        isGameOver = false;
        isLevelFinished = false;

        SetupGrid();
        CreateCards();
        ShuffleCards();

        UpdateUI();
    }


    public void EnqueueFlippedCard(Card card)
    {
        flipQueue.Enqueue(card);

        if (!isProcessing)
            StartCoroutine(ProcessFlipQueue());
    }

    IEnumerator ProcessFlipQueue()
    {
        isProcessing = true;

        while (flipQueue.Count >= 2)
        {
            Card c1 = flipQueue.Dequeue();
            Card c2 = flipQueue.Dequeue();

            turnCount++;

            if (c1.cardId == c2.cardId)
            {
                audioSource.PlayOneShot(matchSound);

                combo++;
                score += 10 * combo;

                c1.isMatched = true;
                c2.isMatched = true;

                StartCoroutine(c1.MatchEffect());
                StartCoroutine(c2.MatchEffect());

                pairsMatched++;

                UpdateUI();

                if (pairsMatched == totalPairs)
                    LevelFinished();
            }
            else
            {
                combo = 0;
                UpdateUI();

                audioSource.PlayOneShot(mismatchSound);

                yield return new WaitForSeconds(0.6f);

                c1.HideCard();
                c2.HideCard();
            }

            yield return null;
        }

        isProcessing = false;
    }


    void SetupGrid()
    {
        GridLayoutGroup grid = cardHolder.GetComponent<GridLayoutGroup>();
        RectTransform rt = cardHolder.GetComponent<RectTransform>();

        float width = rt.rect.width;
        float height = rt.rect.height;

        float padding = 100f;

        grid.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);

        float cellWidth = (width - padding * 2 - spacing * (columns - 1)) / columns;
        float cellHeight = (height - padding * 2 - spacing * (rows - 1)) / rows;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        grid.spacing = new Vector2(spacing, spacing);
        grid.cellSize = new Vector2(cellWidth, cellHeight);
    }


    void CreateCards()
    {
        int totalCards = rows * columns;

        cardIds.Clear();
        cards.Clear();

        for (int i = 0; i < totalCards / 2; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        for (int i = 0; i < totalCards; i++)
        {
            Card c = Instantiate(cardPrefab, cardHolder);
            c.gameManager = this;
            cards.Add(c);
        }

        totalPairs = totalCards / 2;
    }

    void ShuffleCards()
    {
        for (int i = 0; i < cardIds.Count; i++)
        {
            int r = Random.Range(0, cardIds.Count);
            int temp = cardIds[i];
            cardIds[i] = cardIds[r];
            cardIds[r] = temp;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].cardId = cardIds[i];
        }
    }


    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        turnText.text = "Turns: " + turnCount;
        comboText.text = "Combo: " + combo;
    }

    void Update()
    {
        if (!isGameOver && !isLevelFinished)
        {
            timer -= Time.deltaTime;
            TimerText.text = "Time: " + Mathf.Round(timer);

            if (timer <= 0)
                GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        audioSource.PlayOneShot(loseSound);

        finalUI.SetActive(true);
        finalText.text = "Time's Up! Game Over!";
    }

    void LevelFinished()
    {
        isLevelFinished = true;
        audioSource.PlayOneShot(winSound);

        finalUI.SetActive(true);
        finalText.text = "Congratulations! You Win!";
    }


    public void RestartGame()
    {
        foreach (Card c in cards)
            Destroy(c.gameObject);

        cards.Clear();
        flipQueue.Clear();

        score = combo = turnCount = 0;
        pairsMatched = 0;

        isGameOver = false;
        isLevelFinished = false;

        timer = maxTime;

        finalUI.SetActive(false);
        UpdateUI();

        CreateCards();
        ShuffleCards();
    }
}
