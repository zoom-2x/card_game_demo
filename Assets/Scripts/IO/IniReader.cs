using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

namespace CardGame.IO
{
    public enum ReaderType
    {
        FLOAT = 1,
        INT = 2,
        VECTOR3 = 3,
        COLOR = 4,
        STRING = 5
    }

    public class IniReader
    {
        Dictionary<string, Dictionary<string, string>> content = new Dictionary<string, Dictionary<string, string>>();
        
        public IniReader(string filepath)
        {
            if (!String.IsNullOrEmpty(filepath))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(filepath)) {
                        parse(sr);
                    }
                }
                catch {
                    Debug.LogWarning($"IniReader: File not found \"{filepath}\" !");
                }
            }
            else
                Debug.LogWarning("IniReader: Invalid filepath !");
        }

        void parse(StreamReader sr)
        {
            try
            {
                string current_category = "default";
                Dictionary<string, string> current_dictionary = null;

                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine().Trim();

                    // Comment.
                    if (String.IsNullOrEmpty(line) || line[0] == ';')
                        continue;
                    
                    // Category.
                    if (line.Length > 2 && line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        current_category = line.Substring(1, line.Length - 2).ToLower();
                        current_dictionary = new Dictionary<string, string>();
                        content.Add(current_category, current_dictionary);
                    }

                    // key = value.
                    else if (line.Length >= 3 && line.Contains('='))
                    {
                        string[] parts = line.Split('=');
                        current_dictionary.Add(parts[0].Trim(), parts[1].Trim());
                    }
                }
            }
            catch (Exception e) {
                Debug.Log($"IniReader Exception: {e.ToString()}");
            }
        }

        bool key_exists(string category, string key) {
            return (content.ContainsKey(category.ToLower()) && content[category].ContainsKey(key.ToLower()));
        }

        public string get_string(string category, string key, string defval = "")
        {
            string res = defval;

            if (key_exists(category, key))
                res = content[category][key];

            return res;
        }

        public int get_int(string category, string key, int defval = 0)
        {
            int res = defval;

            if (key_exists(category, key))
            {
                string v = content[category][key];
                
                if (!int.TryParse(v, out res))
                    res = defval;
            }

            return res;
        }

        public bool get_bool(string category, string key, bool defval = false)
        {
            bool res = defval;

            if (key_exists(category, key))
            {
                string v = content[category][key];

                if (!bool.TryParse(v, out res))
                {
                    int res2 = 0;

                    if (!int.TryParse(v, out res2))
                        res = false;

                    if (res2 == 1)
                        res = true;
                    else
                        res = false;
                }
            }

            return res;
        }

        public float get_float(string category, string key, float defval = 0)
        {
            float res = defval;

            if (key_exists(category, key))
            {
                string v = content[category][key];
                
                if (!float.TryParse(v, out res))
                    res = defval;
            }

            return res;
        }

        public Vector3 get_vector3(string category, string key, float x = 0, float y = 0, float z = 0) 
        {
            Vector3 res = new Vector3(x, y, z);

            if (content.ContainsKey(category.ToLower()) && content[category].ContainsKey(key.ToLower()))
            {
                string v = content[category][key];
                string[] parts = v.Split(',');

                if (parts.Length == 3)
                {
                    float.TryParse(parts[0], out res.x);
                    float.TryParse(parts[1], out res.y);
                    float.TryParse(parts[2], out res.z);
                }
            }

            return res;
        }

        public Color get_color(string category, string key, float r = 1, float g = 1, float b = 1, float a = 1) 
        {
            Color res = new Color(r, g, b, a);

            if (content.ContainsKey(category.ToLower()) && content[category].ContainsKey(key.ToLower()))
            {
                string v = content[category][key];
                string[] parts = v.Split(',');

                if (parts.Length == 3)
                {
                    float.TryParse(parts[0], out res.r);
                    float.TryParse(parts[1], out res.g);
                    float.TryParse(parts[2], out res.b);
                }
                else if (parts.Length == 4)
                {
                    float.TryParse(parts[0], out res.r);
                    float.TryParse(parts[1], out res.g);
                    float.TryParse(parts[2], out res.b);
                    float.TryParse(parts[3], out res.a);
                }
            }

            return res;
        }
    }
}
