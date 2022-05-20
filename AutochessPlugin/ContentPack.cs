using RoR2.ContentManagement;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using System;
using RORAutochess.AI;
using RORAutochess.Board;

namespace RORAutochess
{
    internal class ContentPacks : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => AutochessPlugin.PluginGUID;

        internal static List<GameObject> bodyPrefabs = new List<GameObject>();
        internal static List<SceneDef> sceneDefs = new List<SceneDef>();
        internal static List<GameObject> gameModePrefabs = new List<GameObject>();
        internal static List<Type> entityStates = new List<Type>();

        public void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;

            entityStates.Add(typeof(CombatPhase));
            entityStates.Add(typeof(PostCombatPhase));
            entityStates.Add(typeof(PreCombatPhase));
            entityStates.Add(typeof(PrepPhase));


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
            contentPack.entityStateTypes.Add(entityStates.ToArray());
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