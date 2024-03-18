using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

namespace CardGame.IO
{
    public class DebugFile
    {
        public static bool enabled = false;
        bool append = false;

        string filepath = "game/debug.log";
        StreamWriter writer;

        static DebugFile instance;
        
        DebugFile()
        {
            if (enabled)
            {
                try {
                    writer = new StreamWriter(Path.Combine(Utils.basepath, filepath), append);
                }
                catch (Exception e) {
                    Debug.LogWarning($"DebugFile: {e.ToString()}");
                }
            }
        }

        public static DebugFile get_instance()
        {
            if (instance == null)
                instance = new DebugFile();

            return instance;
        }

        public void separator()
        {
            if (!enabled)
                return;

            writer?.WriteLine("--------------------------------------------");
        }

        public void write(string s) 
        {
            if (!enabled)
                return;

            writer?.WriteLine(s);
        }

        public void write(bool b, string prefix) 
        {
            if (!enabled)
                return;

            writer?.WriteLine($"{prefix}: {b}");
        }

        public void write(string s, string prefix) 
        {
            if (!enabled)
                return;

            writer?.WriteLine($"{prefix}: {s}");
        }

        public void write(int v)
        {
            if (!enabled)
                return;

            writer?.WriteLine($"{v}");
        }

        public void write(int v, string prefix) 
        {
            if (!enabled)
                return;

            writer?.WriteLine($"{prefix}: {v}");
        }

        public void write(Vector3 v)
        {
            if (!enabled)
                return;

            writer?.WriteLine($"{v.x}, {v.y}, {v.z}");
        }

        public void write(Vector3 v, string prefix)
        {
            if (!enabled)
                return;

            writer?.WriteLine($"{prefix}: {v.x}, {v.y}, {v.z}");
        }

        public void write(Vector3[] arr)
        {
            if (!enabled)
                return;

            for (int i = 0; i < arr.Length; ++i)
            {
                Vector3 v = arr[i];
                write(v);
            }
        }

        public void flush()
        {
            if (!enabled)
                return;

            writer?.Flush();
        }

        public void close()
        { 
            if (!enabled)
                return;

            writer?.Close();
        }
    }
}
