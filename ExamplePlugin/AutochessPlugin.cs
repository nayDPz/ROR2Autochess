using System.Reflection;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.IO;
using RoR2.Audio;
using System.Collections.Generic;
using RoR2.UI;
using RoR2.Projectile;
using System.Linq;
using UnityEngine.AddressableAssets;
using RoR2.Navigation;
using BepInEx;
using R2API.Utils;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace RORAutochess
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(PrefabAPI))]
	
    public class AutochessPlugin : BaseUnityPlugin
	{
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "AuthorName";
        public const string PluginName = "ExamplePlugin";
        public const string PluginVersion = "1.0.0";

        
        public static string AssetBundlePath
        {
            get
            {
                //This returns the path to your assetbundle assuming said bundle is on the same folder as your DLL. If you have your bundle in a folder, you can uncomment the statement below this one.
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(AutochessPlugin.PInfo.Location), "genericboardscene");
                //return Path.Combine(Path.GetDirectoryName(MainClass.PInfo.Location), assetBundleFolder, myBundle);
            }
        }



        public static PluginInfo PInfo { get; private set; }
        internal static AssetBundle scene;
        

        public void Awake()
        {
            Log.Init(Logger);
            PInfo = Info;

            if (scene == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AutochessPlugin.genericboardscene"))
                {
                    scene = AssetBundle.LoadFromStream(assetStream);
                }           
            }

            

            GenericBoard.Setup(15, 12, 0.1f, 4f);


            new ContentPacks().Initialize();

            Log.LogInfo(nameof(Awake) + " done.");
        }


        private void Update()
        {


        }
    }
}
