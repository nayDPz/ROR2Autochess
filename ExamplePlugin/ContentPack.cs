using RoR2.ContentManagement;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
namespace RORAutochess
{
    internal class ContentPacks : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => AutochessPlugin.PluginGUID;

        internal static List<GameObject> bodyPrefabs = new List<GameObject>();
        internal static List<SceneDef> sceneDefs = new List<SceneDef>();
        internal static List<GameObject> gameModePrefabs = new List<GameObject>();
        public void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;
            contentPack.sceneDefs.Add(sceneDefs.ToArray());
            contentPack.gameModePrefabs.Add(gameModePrefabs.ToArray());
            contentPack.bodyPrefabs.Add(bodyPrefabs.ToArray());
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}