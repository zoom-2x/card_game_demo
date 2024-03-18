using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;

namespace CardGame.Mono
{
    public enum KeyboardEnum : ushort
    {
        KEY_FRONT = 0,
        KEY_BACK = 1,
        KEY_LEFT = 2,
        KEY_RIGHT = 3,
        KEY_UP = 4,
        KEY_DOWN = 5,
        KEY_ENTER = 6,
        KEY_BACKSPACE = 7,
        KEY_COUNT = 8
    }

    public enum MouseEventEnum
    {
        OVER = 1,
        SELECTED = 2,
        DRAG = 4,
        DRAG_RELEASED = 8
    }

    public enum MouseEventMask
    {
        NONE = 0,
        OVER = 0b00000001,
        CLICK = 0b00000111,
        DRAG_AND_DROP = 0b00011111
    }

    public struct ObjectEvent
    {
        public int id;
        public uint flags;
        public Transform t;
    }

    public struct ObjectDelay
    {
        public int id;
        public float delay;
    }

    public struct CardPlayEvent
    {
        public CardMono card;
        public uint events;
        public int id;
        public Vector3 drag_local_position;
    }

    public struct MapPlayEvent
    {
        public uint events;
    }

    public struct BuyEvent
    {
        public CardContainerMono container;
        public uint events;
    }

    public class GameInput
    {
        public bool enable = true;

        public const uint OVER = 0b00000001;
        // public const uint LEFT_CLICK = 0b00000010;
        // public const uint RIGHT_CLICK = 0b00000100;
        // public const uint DRAG = 0b00001000;
        // public const uint DROP = 0b00010000;

        public const uint LEFT_BUTTON_DOWN = 0b00000010;
        public const uint LEFT_BUTTON_UP = 0b00000100;
        public const uint RIGHT_BUTTON_DOWN = 0b00001000;
        public const uint RIGHT_BUTTON_UP = 0b00010000;

        public uint[] card_play_event_filter = new uint[(int) CardLocation.LOCATION_COUNT];

        int collision_mask = 0;
        public PlayerID filter_player = PlayerID.NONE;

        // public ObjectEvent obj_event = new ObjectEvent() {id = 0, flags = 0, t = null};

        // Game phase events.
        public CardPlayEvent card_play_event = new CardPlayEvent() {card = null, events = 0};
        public MapPlayEvent map_play_event = new MapPlayEvent() {events = 0};
        public BuyEvent buy_event = new BuyEvent() {container = null, events = 0};

        List<ObjectDelay> delays = new List<ObjectDelay>();

        RaycastHit hit = new RaycastHit();

        public bool mouse_left_button_pressed = false;

        public Vector3[] mouse_move_points = new Vector3[3];
        public int mouse_move_start = 0;
        int mouse_move_index = 0;
        int frame_count = 0;
        int mouse_move_target_fps = 10;

        // Filters for restricting input detection.
        public CardLocation filter_location = CardLocation.LOCATION_NONE;

        protected string[] key_bindings = new string[(ushort) KeyboardEnum.KEY_COUNT];

        bool[] _key_down = new bool[(ushort) KeyboardEnum.KEY_COUNT];
        bool[] _key_pressed = new bool[(ushort) KeyboardEnum.KEY_COUNT];

        public GameInput()
        {
            key_bindings[(ushort) KeyboardEnum.KEY_FRONT] = "w";
            key_bindings[(ushort) KeyboardEnum.KEY_BACK] = "s";
            key_bindings[(ushort) KeyboardEnum.KEY_LEFT] = "a";
            key_bindings[(ushort) KeyboardEnum.KEY_RIGHT] = "d";
            key_bindings[(ushort) KeyboardEnum.KEY_UP] = "q";
            key_bindings[(ushort) KeyboardEnum.KEY_DOWN] = "e";
            key_bindings[(ushort) KeyboardEnum.KEY_ENTER] = "return";
            key_bindings[(ushort) KeyboardEnum.KEY_BACKSPACE] = "backspace";

            // Reset the location event mask for each card location.
            for (int i = 0; i < (int) CardLocation.LOCATION_COUNT; ++i) {
                card_play_event_filter[i] = 0;
            }

            reset_collision_mask();
        }

        public void reset_collision_mask() {
            collision_mask = Constants.LAYER_MASK_PLAYER[0] |
                             Constants.LAYER_MASK_PLAYER[1] |
                             Constants.LAYER_MASK_PLAYER[2] |
                             Constants.LAYER_MASK_PLAYER[3];
        }

        public void set_collision_mask(int mask) {
            collision_mask = mask;
        }

        public void register_location_event(CardLocation location, uint events)
        {
            if (location >= CardLocation.LOCATION_HAND && location < CardLocation.LOCATION_COUNT)
                card_play_event_filter[(int) location] = events;
        }

        // ----------------------------------------------------------------------------------
        // -- Card play events.
        // ----------------------------------------------------------------------------------
        
        void card_play_update()
        {
            bool mouse_left_clicked = Input.GetMouseButtonDown(0);
            bool mouse_right_clicked = Input.GetMouseButtonDown(1);
            bool mouse_left_released = Input.GetMouseButtonUp(0);
            bool mouse_right_released = Input.GetMouseButtonUp(1);
            bool mouse_pressed = Input.GetMouseButton(0);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool hit_check = Physics.Raycast(ray, out hit, Mathf.Infinity, collision_mask);

            card_play_event.card = null;
            card_play_event.id = -1; 
            card_play_event.events = 0;

            if (hit_check)
            {
                CardMono card = hit.transform.GetComponent<CardMono>();

                if (card != null && card.location != CardLocation.LOCATION_NONE)
                {
                    card_play_event.card = card;
                    card_play_event.id = card.GetInstanceID();
                    card_play_event.events = OVER;
                }
            }

            if (mouse_left_clicked)
                card_play_event.events |= LEFT_BUTTON_DOWN; 

            if (mouse_left_released)
                card_play_event.events |= LEFT_BUTTON_UP; 

            if (mouse_right_clicked)
                card_play_event.events |= RIGHT_BUTTON_DOWN; 

            if (mouse_right_released)
                card_play_event.events |= RIGHT_BUTTON_UP; 
        }

        void reset_card_play_event()
        {
            card_play_event.card = null;
            card_play_event.events = 0;
            card_play_event.id = 0;
            card_play_event.drag_local_position = Vector3.zero;
        }

        public void update()
        {
            frame_count++;

            // Nothing registers if disabled.
            if (!enable)
            {
                reset_card_play_event(); 
                return;
            }

            // ----------------------------------------------------------------------------------
            // -- Keyboard.
            // ----------------------------------------------------------------------------------

            for (int i = 0; i < (ushort) KeyboardEnum.KEY_COUNT; ++i)
            {
                _key_down[i] = Input.GetKeyDown(key_bindings[i]);
                _key_pressed[i] = Input.GetKey(key_bindings[i]);
            }

            // ----------------------------------------------------------------------------------
            // -- Mouse.
            // ----------------------------------------------------------------------------------

            // bool mouse_left_clicked = Input.GetMouseButtonDown(0);
            // bool mouse_right_clicked = Input.GetMouseButtonDown(1);
            // bool mouse_pressed = Input.GetMouseButton(0);
            // bool mouse_released = Input.GetMouseButtonUp(0);

            if ((frame_count % mouse_move_target_fps) == 0)
            {
                mouse_move_points[mouse_move_index] = Input.mousePosition;
                mouse_move_start = mouse_move_index == 0 ? 2 : mouse_move_index - 1;
                mouse_move_index = (mouse_move_index + 1) % 3;
            }

            card_play_update();
        }

        // Returns a vector that represents the direction of the last mouse movement.
        public Vector3 mouse_slide_vector(float z)
        {
            int start = mouse_move_start;
            int end = (start + 1) % 3;

            Vector3 screen_p0 = mouse_move_points[start];
            Vector3 screen_p1 = mouse_move_points[end];

            screen_p0.z = z;
            screen_p1.z = z;

            Vector3 world_p0 = GameSystems.game.scene_camera.ScreenToWorldPoint(screen_p0);
            Vector3 world_p1 = GameSystems.game.scene_camera.ScreenToWorldPoint(screen_p1);

            return (world_p1 - world_p0) * 0.2f;
        }

        public bool key_down(KeyboardEnum key) {
            return _key_down[(ushort) key];
        }

        public bool key_pressed(KeyboardEnum key) {
            return _key_pressed[(ushort) key];
        }
    }
}
