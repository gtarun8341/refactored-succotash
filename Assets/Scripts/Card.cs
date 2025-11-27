using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int cardId;
    public GameManagerCard gameManager;
    private bool isFlipped;
    public Image cardImage;

    void Start()
    {
        isFlipped = false;
        cardImage.sprite = GameManagerCard.Instance.cardBack;
    }

    public void FlipCard()
    {
        
            if(!isFlipped && (gameManager.firstCard == null || gameManager.secondCard == null))
            {
                isFlipped = true;
                cardImage.sprite = gameManager.cardFaces[cardId];
                gameManager.CardFlipped(this);
            }
        
    }

    public void HideCard()
    {
        isFlipped = false;
        cardImage.sprite = gameManager.cardBack;
    }
}
