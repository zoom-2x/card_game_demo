namespace CardGame.Data
{
    public class Constants
    {
        public const bool DEBUG_MODE = true;

        public const float DECK_WIDTH = 0.768f;
        public const float DECK_HEIGHT = 1.15f;

        public const float CARD_WIDTH = 0.64f;
        public const float CARD_HEIGHT = 1.0f;
        public const float CARD_DEPTH = 0.004f;

        public const float MARKET_WIDTH = 4.46f;
        public const float MARKET_HEIGHT = 2.0f;

        public const int MAX_CARDS = 10;

        public const string P1_DOMAIN = "p1_domain";
        public const string P2_DOMAIN = "p2_domain";
        public const string P3_DOMAIN = "p3_domain";
        public const string P4_DOMAIN = "p4_domain";

        public static readonly int[] LAYER_PLAYER = new int[] {6, 7, 8, 9};
        public const int LAYER_MARKETPLACE = 10;

        public static readonly int[] LAYER_MASK_PLAYER = new int[] { 1 << 6, 1 << 7, 1 << 8, 1 << 9 };
        public const int LAYER_MASK_MARKETPLACE = 1 << 10;
    }
}
