namespace CardGame.Data
{
    public enum CardLocation : int
    {
        LOCATION_NONE = -1,
        LOCATION_HAND = 0,
        LOCATION_DRAW = 1,
        LOCATION_DISCARD = 2,
        LOCATION_PLAY = 3,
        LOCATION_PLAY_PREVIEW = 4,
        LOCATION_DISCARD_PREVIEW = 5,
        LOCATION_TRASH = 6,
        LOCATION_MARKETPLACE_0 = 7,
        LOCATION_MARKETPLACE_1 = 8,
        LOCATION_MARKETPLACE_2 = 9,
        LOCATION_MARKETPLACE_3 = 10,
        LOCATION_COUNT = 11
    }

    public enum LinkType : short
    {
        NONE = -1,
        DRAW_PILE_HAND = 0,
        DRAW_PILE_DISCARD_PILE = 1,
        HAND_PLAY = 2,
        HAND_DISCARD_PILE = 3,
        PLAY_DISCARD_PILE = 4,
        HAND_TRASH_PILE = 5,
        HAND_PLAY_PREVIEW = 6,
        HAND_DISCARD_PREVIEW = 7,
        PLAY_PREVIEW_DISCARD_PILE = 8,
        DISCARD_PREVIEW_DISCARD_PILE = 9,
        MARKET_0_DRAW_PILE = 10,
        MARKET_1_DRAW_PILE = 11,
        MARKET_2_DRAW_PILE = 12,
        MARKET_3_DRAW_PILE = 13,
        LINK_COUNT = 14
    }

    public enum PlayerID : short
    {
        NONE = -1,
        P1 = 0,
        P2 = 1,
        P3 = 2,
        P4 = 3,
        PLAYER_COUNT = 4
    }

    public enum OrientationAlignment
    {
        CENTER = 1,
        LEFT = 2,
        RIGHT = 3
    }

    public enum GameState : int
    {
        STATE_START = 0,
        STATE_LOADING = 1,
        STATE_INIT = 2,
        STATE_RUNNING = 3
    }

    public enum GamePhase : int
    {
        NONE = 0,
        CARD_PLAY = 1,
        MAP_PLAY = 2,
        DISCARD = 3,
        DRAW = 4,
        BUY = 5,
        CONFIRM = 6,
        END_TURN = 7
    }

    public enum PlayCardPhase
    {
        NONE = 0,
        STARTUP = 1,
        SELECT_CARD = 2,
        CONFIRM = 3
    }

    public enum DiscardPhase
    {
        NONE = 0,
        STARTUP = 1,
        SELECT_CARD = 2,
        CONFIRM = 3
    }

    public enum MapPlayPhase
    {
        NONE = 0,
        STARTUP = 1
    }

    public enum CommandState : ushort
    {
        COMMAND_READY = 1,
        COMMAND_RUNNING = 2,
        COMMAND_FINISHED = 3
    }

    public enum CommandType : ushort
    {
        COMMAND_MATH = 0,
        COMMAND_DRAW = 1,
        COMMAND_TRANSFER_TO_ALL = 2,
        COMMAND_TRANSFER_TO_SINGLE = 3,
        COMMAND_TOGGLE_PREVIEW = 4,
        COMMAND_PLAY_CARD = 5,
        COMMAND_PLAY_TOKEN = 6,
        COMMAND_DISCARD = 7,
        COMMAND_TRANSFER_FLOAT = 8,
        COMMAND_TRANSFER_TO_SELECTION = 9,
        COMMAND_WAIT_FOR_INPUT = 10,
        COMMAND_COUNT = 11
    }

}
