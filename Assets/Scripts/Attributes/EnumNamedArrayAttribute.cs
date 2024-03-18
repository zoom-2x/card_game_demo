using UnityEngine;

namespace CardGame.Attributes
{
    public class EnumNamedArrayAttribute : PropertyAttribute
    {
        public string[] names;

        public EnumNamedArrayAttribute(System.Type type) {
            names = System.Enum.GetNames(type);
        }
    }
}
