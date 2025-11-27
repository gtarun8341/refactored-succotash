using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int score;
    public int turnCount;
    public int combo;
    public float timer;

    public List<int> cardIds;     
    public List<bool> matched;   
    public List<bool> flipped;    
}
