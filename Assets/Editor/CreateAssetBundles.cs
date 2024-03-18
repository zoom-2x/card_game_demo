using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using System;
using System.IO;

public class CreateAssetBundles : IPostprocessBuildWithReport
{
    // -----------------------------------------------------------
    // -- Asset bundle generation option.
    // -----------------------------------------------------------

    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/assetbundles";

        if (!Directory.Exists(assetBundleDirectory))
            Directory.CreateDirectory(assetBundleDirectory);

        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
    }

    static void BuildLevelAssetBundles()
    {}

    // -----------------------------------------------------------
    // -- Post build assetbundles directory copy.
    // -----------------------------------------------------------

    public int callbackOrder { get { return 0; } }

    struct SourceData
    {
        public string src_path;
        public string name;
    };

    public void OnPostprocessBuild(BuildReport report) 
    {
        string build_output_path = report.summary.outputPath;
        int index = build_output_path.LastIndexOf(".exe");

        if (index > -1)
        {
            build_output_path = build_output_path.Remove(index);
            string output_data_path = $"{build_output_path}_Data";

            SourceData[] sources = new SourceData[]
            {
                new SourceData() {src_path = "Assets/assetbundles", name = "assetbundles"},
                new SourceData() {src_path = "Assets/game", name = "game"},
            };

            foreach (SourceData source in sources)
            {
                string final_path = Path.Combine(output_data_path, source.name);
                DirectoryInfo dest_path = new DirectoryInfo(final_path);

                if (dest_path.Exists)
                    dest_path.Delete(true);

                copy_directory(source.src_path, final_path, true);
            }
        }
    }

    void copy_directory(string src, string dest, bool recursive = false)
    {
        DirectoryInfo di = new DirectoryInfo(src);

        if (!di.Exists)
        {
            Debug.LogWarning($"Directory does not exists: {src}");
            return;
        }

        Debug.Log($"Copying directory: {src}");
        
        DirectoryInfo[] in_dirs = di.GetDirectories();
        Directory.CreateDirectory(dest);

        foreach (FileInfo f in di.GetFiles())
        {
            string fpath = Path.Combine(dest, f.Name);
            f.CopyTo(fpath, true);
        }

        if (recursive)
        {
            foreach (DirectoryInfo d in in_dirs)
            {
                string dpath = Path.Combine(dest, d.Name);
                copy_directory(d.FullName, dpath);
                Debug.Log(d.FullName);
            }
        }
    }
}
