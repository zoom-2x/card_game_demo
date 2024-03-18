using UnityEngine;
using CardGame.Data;
using CardGame.Attributes;

[CreateAssetMenu(fileName = "card_data", menuName = "Game/CardTemplate", order = 1)]
public class CardTemplate : ScriptableObject 
{
    [CardArraySelect(1, "Card effect")]
    public int effect_index;
    [CardArraySelect(2, "Card art")]
    public int art_index;

    new public string name;
    public string description;
    public int cost;
    // 5 x 1 effect (4 bytes) = 20 bytes
    public byte[] effects;
}
