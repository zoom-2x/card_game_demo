using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Data;
using CardGame.Attributes;
using CardGame.Animation;
using CardGame.Mono.Orientation;
using CardGame.Mono.State;
using CardGame.Hexgrid;

namespace CardGame.Mono
{
    public struct CardSelection
    {
        public CardMono card;
        public Vector3 local_position;
    }

    public class Player : MonoBehaviour
    {
        static byte _index = 1;
        bool initialized = false;

        public string player_name = "Default Player";
        public PlayerID id = PlayerID.NONE;

        [EnumNamedArray(typeof(CardLocation))]
        public CardContainerMono[] containers = new CardContainerMono[(int) CardLocation.LOCATION_COUNT];
        [EnumNamedArray(typeof(LinkType))]
        public Link[] links = new Link[(int) LinkType.LINK_COUNT];

        [System.NonSerialized] public HexMap map;
        [System.NonSerialized] public PlayerResources res = new PlayerResources();
        [System.NonSerialized] public PlayerResources res_modifiers = new PlayerResources();

        List<HexTile> owned_tiles = new List<HexTile>();
        List<HexRegion> owned_regions = new List<HexRegion>();
        List<HexTile> target_tiles = new List<HexTile>();

        [System.NonSerialized] public int discard_count = 0;
        [System.NonSerialized] public HexTileInfoState default_tile_info_state = HexTileInfoState.STATE_HIDDEN;
        [System.NonSerialized] public CardSelection card_selection = new CardSelection() {card = null, local_position = Vector3.zero};

        void Start()
        {}

        // ----------------------------------------------------------------------------------

        public void initialize()
        {
            if (!initialized)
            {
                initialized = true;
                reset();

                // Container owner and location initialization.
                for (int i = 0; i < (int) CardLocation.LOCATION_COUNT; ++i)
                {
                    CardContainerMono container = containers[i];

                    if (container != null)
                    {
                        container.owner = id;
                        container.location = (CardLocation) i;

                        GameSystems.input.register_location_event(container.location, (uint) container.event_mask);
                    }
                }

                // generate_starting_cards();
                reset();
            }
        }

        protected void generate_starting_cards()
        {
            // CardMono card_influence = CardLibrary.create_card(CardTemplateEnum.TEMPLATE_INFLUENCE, id);
            // CardMono card_credits = CardLibrary.create_card(CardTemplateEnum.TEMPLATE_CREDITS, id);
            // CardMono card_propaganda = CardLibrary.create_card(CardTemplateEnum.TEMPLATE_PROPAGANDA, id);
        }

        public void reset()
        {
            if (player_name.Length == 0)
                player_name = "Player " + _index++;

            for (int i = 0; i < PlayerResources.COUNT; ++i)
            {
                res.values[i] = 0;
                res_modifiers.values[i] = 0;
            }

            res.vp = GameSystems.game.max_vp;
        }

        public Link get_link(LinkType type) {
            return links[(int) type];
        }

        public CardContainerMono get_container(CardLocation location) {
            return containers[(int) location];
        }

        public int get_container_count(CardLocation location)
        {
            CardContainerMono container = get_container(location);
            return container.cards.Count;
        }

        public IDynamicBehaviour get_behaviour(CardLocation location)
        {
            CardContainerMono container = get_container(location);
            IDynamicBehaviour behaviour = container.gameObject.GetComponent<IDynamicBehaviour>();

            return behaviour;
        }

        public int get_resource_base(Resource type) {
            return res.values[(int) type];
        }

        public int get_resource_modif(Resource type) {
            return res_modifiers.values[(int) type];
        }

        public int get_resource_total(Resource type) {
            return (int) (res.values[(int) type] + res_modifiers.values[(int) type]);
        }

        public void set_resource_base(Resource type, short val) {
            res.values[(int) type] = val;
        }

        public void set_resource_modif(Resource type, short val) {
            res_modifiers.values[(int) type] = val;
        }

        public void incr_resource_base(Resource type, short val) {
            res.values[(int) type] += val;
        }

        public void incr_resource_modif(Resource type, short val) {
            res_modifiers.values[(int) type] += val;
        }

        public void add_tile(HexTile tile)
        {
            if (tile == null || tile.owner == id)
                return;

            tile.set_owner(id);
            owned_tiles.Add(tile);
        }

        public void add_region(HexRegion region)
        {
            if (region == null || region.owner == id)
                return;

            region.set_owner(id);
            owned_regions.Add(region);
        }

        // NOTE(gabic): Pe viitor, probabil ar fi o idee mai buna
        // sa incerc o cautare binara, cu sortare (dupa tile id) la 
        // fiecare adaugare in lista.
        public void remove_tile(HexTile tile)
        {
            if (tile == null || tile.owner != id)
                return;

            tile.set_owner(PlayerID.NONE);
            owned_tiles.Remove(tile);
        }

        bool is_owned(HexTile t) 
        {
            if (t == null)
                return false;

            return t.owner == id;
        }

        bool is_on_border(HexTile t)
        {
            for (int j = 0; j < 6; ++j)
            {
                HexTile n = t.get_neighbour(j);

                if (!is_owned(n)) 
                    return true;
            }

            return false;
        }

        bool is_owned_border(HexTile t, int edge) {
            return is_owned(t.get_neighbour(edge));
        }

        // Generates a list of border tiles which can be the 
        // target of certain player actions.
        public void generate_border_target_tiles()
        {
            target_tiles.Clear();
            map.reset_tile_state();
            HexTile start_tile = null;

            // Find an edge tile.
            for (int i = 0; i < owned_tiles.Count; ++i)
            {
                HexTile t = owned_tiles[i];

                if (is_on_border(t))
                {
                    start_tile = t;
                    break;
                }
            }

            // Determine the rest of the border tiles.
            if (start_tile != null)
            {
                int start_edge = HexTile.EDGE_NE;
                int transition_edge = -1;
                int check_count = 0;

                Vector2Int start = new Vector2Int(start_tile.id, -1);

                while (true)
                {
                    bool added = false;
                    int added_count = 0;
                    transition_edge = -1;

                    // Cycle through the tile's edges and determine a border.
                    // while (true)
                    for (int i = 0; i < 6; ++i)
                    {
                        int edge = (start_edge++) % 6;

                        if (start_tile.id == start.x && edge == start.y)
                        {
                            transition_edge = -1;
                            break;
                        }

                        HexTile neighbour = start_tile.get_neighbour(edge);
                        bool is_edge = !is_owned(neighbour);

                        if (is_edge)
                        {
                            if (neighbour != null && 
                                neighbour.has_flag(HexTile.FLAG_TILE) && 
                                neighbour.tile_state != HexTileState.STATE_DASHED)
                            {
                                added = true;
                                added_count++;

                                neighbour.set_tile_state(HexTileState.STATE_DASHED);
                                neighbour.locked_state = true;
                                target_tiles.Add(neighbour);
                            }

                            if (start.y == -1)
                                start.y = edge;

                            added = true;
                        }

                        else if (added)
                        {
                            transition_edge = edge;
                            break;
                        }

                        // The region contains a single hex.
                        if (added_count == 6)
                            break;
                    }

                    if (transition_edge != -1)
                    {
                        start_tile = start_tile.get_neighbour(transition_edge);
                        start_edge = (transition_edge + 4) % 6;
                    }
                    else
                        break;

                    check_count++;

                    if (check_count > 1000)
                    {
                        Debug.Log("infinite");
                        break;
                    }
                }
            }

            Debug.Log($"tiles: {target_tiles.Count}");
        }

        public void mark_own_tiles()
        {
            for (int i = 0; i < owned_tiles.Count; ++i)
            {
                HexTile tile = owned_tiles[i];
                tile.set_tile_state(HexTileState.STATE_DASHED);
            }
        }

        public void set_default_tile_info_state() {
            set_tile_info_state(default_tile_info_state);
        }

        public void set_tile_info_state(HexTileInfoState state)
        {
            for (int i = 0; i < owned_tiles.Count; ++i)
            {
                HexTile tile = owned_tiles[i];
                tile.set_tile_info_state(state);
            }
        }
    }
}
