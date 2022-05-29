using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using System.Linq;
namespace RORAutochess.UI
{
    public class ShopSlot : MonoBehaviour //sometimes feels like this shouldnt exist
    {
        public GameObject shopEntryInstance;
        public ShopEntry shopEntry;
        public CharacterMaster source;
        public Shop shop;

        public void RefreshEntry() 
        {
            List<SurvivorDef> d = SurvivorCatalog.allSurvivorDefs.ToList();

            List<GameObject> masters = new List<GameObject>();
            foreach(SurvivorDef def in d)
            {
                masters.Add(MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(def.bodyPrefab.GetComponent<CharacterBody>().bodyIndex)));
            }

            int i = UnityEngine.Random.Range(0, masters.Count); // rng goes here
            CharacterMaster master = masters[i].GetComponent<CharacterMaster>();

            if (shopEntryInstance) GameObject.Destroy(shopEntryInstance);

            shopEntryInstance = GameObject.Instantiate<GameObject>(shop.entryPrefab, base.transform);


            shopEntry = shopEntryInstance.GetComponent<ShopEntry>();
            shopEntry.slot = this; // what the fuck is going on whyyyyyyyyyyyyyyyy
            shopEntry.source = this.source;
            shopEntry.unitMaster = master;
        }
    }
}
