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

    private float autosaveTimer = 0f;

    private bool initializationFailed = false;
    public bool InitializationFailed => initializationFailed;



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

            int neededPairs = (rows * columns) / 2;

    if (cardFaces.Length < neededPairs)
    {
        initializationFailed = true; 
        Debug.LogError($"Not enough card faces! Need {neededPairs}, but only {cardFaces.Length} provided.");
        cardHolder.gameObject.SetActive(false);
        finalUI.SetActive(true);
        finalText.text = "Error: Not enough card images for selected grid!";
        return;
    }

        CreateCards();
        // ShuffleCards();

        if (PlayerPrefs.HasKey("CardGameSave"))
            LoadGame();
        else
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
        SaveGame();
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
        if (initializationFailed) return;
        
        if (!isGameOver && !isLevelFinished)
        {
            timer -= Time.deltaTime;
            TimerText.text = "Time: " + Mathf.Round(timer);

            autosaveTimer += Time.deltaTime;
            if (autosaveTimer >= 5f)
            {
                SaveGame();
                autosaveTimer = 0f;
            }

            if (timer <= 0)
                GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        audioSource.PlayOneShot(loseSound);

        PlayerPrefs.DeleteKey("CardGameSave");
        finalUI.SetActive(true);
        finalText.text = "Time's Up! Game Over!";
    }

    void LevelFinished()
    {
        isLevelFinished = true;
        audioSource.PlayOneShot(winSound);

        PlayerPrefs.DeleteKey("CardGameSave");

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

        PlayerPrefs.DeleteKey("CardGameSave");


        finalUI.SetActive(false);
        UpdateUI();

        CreateCards();
        ShuffleCards();
    }


    public void SaveGame()
{
    if (initializationFailed) return;
    if (isGameOver || isLevelFinished) return; 
    SaveData data = new SaveData();

    data.score = score;
    data.turnCount = turnCount;
    data.combo = combo;
    data.timer = timer;

    data.cardIds = new List<int>(cardIds);
    data.matched = new List<bool>();
    data.flipped = new List<bool>();

foreach (Card c in cards)
{
    data.matched.Add(c.isMatched);

    if (c.isMatched)
        data.flipped.Add(true);
    else
        data.flipped.Add(false); 
}
    string json = JsonUtility.ToJson(data);
    PlayerPrefs.SetString("CardGameSave", json);
    PlayerPrefs.Save();

    Debug.Log("Game Saved");
}


public void LoadGame()
{
    
    if (initializationFailed) return;

    if (!PlayerPrefs.HasKey("CardGameSave"))
    {
        Debug.Log("No save file found");
        return;
    }

    string json = PlayerPrefs.GetString("CardGameSave");
    SaveData data = JsonUtility.FromJson<SaveData>(json);

    if (data.timer <= 0)
    {
        Debug.Log("Save ignored: timer expired");
        PlayerPrefs.DeleteKey("CardGameSave");
        return;
    }

    if (data.matched.TrueForAll(x => x))
    {
        Debug.Log("Save ignored: game already completed");
        PlayerPrefs.DeleteKey("CardGameSave");
        return;
    }

    // Restore basic values
    score = data.score;
    turnCount = data.turnCount;
    combo = data.combo;
    timer = data.timer;

    cardIds = new List<int>(data.cardIds);

    int matchedCount = 0;

    // Restore card states
    for (int i = 0; i < cards.Count; i++)
    {
        cards[i].cardId = cardIds[i];
        cards[i].hasLoadedState = true;


        if (data.matched[i])
        {
            cards[i].isMatched = true;
            cards[i].isFlipped = true;

            matchedCount++;
            cards[i].cardImage.color = Color.white;
            cards[i].cardImage.sprite = cardFaces[cards[i].cardId];
        }
        else if (data.flipped[i])
        {
            cards[i].isFlipped = true;
            cards[i].cardImage.color = Color.white;
            cards[i].cardImage.sprite = cardFaces[cards[i].cardId];
        }
        else
        {
                cards[i].isFlipped = false;
                cards[i].isMatched = false;
                cards[i].cardImage.color = Color.white;
                cards[i].cardImage.sprite = cardBack;
        }
    }

    pairsMatched = matchedCount / 2;

    UpdateUI();

    Debug.Log("Game Loaded");
}


}
