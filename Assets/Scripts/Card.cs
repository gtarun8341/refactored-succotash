using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int cardId;
    public GameManagerCard gameManager;
    public Image cardImage;

    public bool isFlipped = false;
    public bool isMatched = false;

    private bool isAnimating = false;

    [HideInInspector] public bool hasLoadedState = false;


    void Start()
    {
    if (!hasLoadedState)
    {
        cardImage.color = Color.white;
        cardImage.sprite = gameManager.cardBack;
    }
    }

    public void FlipCard()
    {
         if (gameManager.InitializationFailed) return; 

        if (isMatched || isAnimating || isFlipped)
            return;

        // if (!isFlipped)
        // {
            StartCoroutine(FlipAnimation(gameManager.cardFaces[cardId], true));
            gameManager.audioSource.PlayOneShot(gameManager.flipSound);
            gameManager.EnqueueFlippedCard(this);
        // }
    }

    public IEnumerator FlipAnimation(Sprite newSprite, bool flipUp)
    {
        isAnimating = true;

        for (float t = 1f; t >= 0f; t -= Time.deltaTime * 6)
        {
            transform.localScale = new Vector3(1, t, 1);
            yield return null;
        }

        cardImage.sprite = newSprite;

        for (float t = 0f; t <= 1f; t += Time.deltaTime * 6)
        {
            transform.localScale = new Vector3(1, t, 1);
            yield return null;
        }

        isFlipped = flipUp;
        isAnimating = false;
    }

    public IEnumerator MatchEffect()
    {
        isMatched = true;
        isFlipped = true;  
        isAnimating = true;

        cardImage.color = new Color(1f, 1f, 0.5f);

        for (float t = 1f; t <= 1.2f; t += Time.deltaTime * 8)
        {
            transform.localScale = new Vector3(t, t, 1);
            yield return null;
        }
        for (float t = 1.2f; t >= 1f; t -= Time.deltaTime * 8)
        {
            transform.localScale = new Vector3(t, t, 1);
            yield return null;
        }

        cardImage.color = Color.white;
    transform.localScale = Vector3.one;
        isAnimating = false;
    }

    public void HideCard()
    {
        if (isAnimating) return;

        StartCoroutine(FlipAnimation(gameManager.cardBack, false));
    }
}
