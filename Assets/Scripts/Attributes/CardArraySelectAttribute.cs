using UnityEngine;
using CardGame.Data;

namespace CardGame.Attributes
{
    public class CardArraySelectAttribute : PropertyAttribute
    {
        public readonly int selection;
        public readonly string label;

        public CardArraySelectAttribute(int selection, string label) 
        {
            this.selection = selection;
            this.label = label;
        }
    }
}
