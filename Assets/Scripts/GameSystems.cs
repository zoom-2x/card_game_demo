using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Mono;
using CardGame.Managers;
using CardGame.Hexgrid;

namespace CardGame
{
    public static class GameSystems
    {
        // public static GameMono GAME;
        public static GameObject cache;

        public static BundleManager bundle_manager = new BundleManager();
        public static AssetManager asset_manager = new AssetManager(bundle_manager);
        public static MemoryManager memory_manager = new MemoryManager();
        // public static CommandManager command_manager = new CommandManager();

        public static List<CardContainerMono> reposition_list = new List<CardContainerMono>();
        public static List<CardContainerMono> elastic_list = new List<CardContainerMono>();

        public static List<IDynamicBehaviour>[] persistent = new List<IDynamicBehaviour>[4];
        public static GameInput input = new GameInput();

        public static Game game;
    }
}
