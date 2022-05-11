using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using System.Linq;
namespace RORAutochess.UI
{
    public class ShopSlot : MonoBehaviour
    {
        public GameObject shopEntryInstance;
        public ShopEntry shopEntry;
        public CharacterMaster source;


        public void RefreshEntry() 
        {
            List<GameObject> masters = MasterCatalog.masterPrefabs.Where(x => x.GetComponent<Units.UnitData>() != null && x.GetComponent<Units.UnitData>().master != null).ToList();
            int i = UnityEngine.Random.Range(0, masters.Count - 1); // rng goes here
            CharacterMaster master = masters[i].GetComponent<CharacterMaster>();

            if (shopEntryInstance) GameObject.Destroy(shopEntryInstance);

            shopEntryInstance = GameObject.Instantiate<GameObject>(Shop.entryPrefab, base.transform);

            shopEntry = shopEntryInstance.GetComponent<ShopEntry>();
            shopEntry.source = this.source;

            Units.UnitData unitData = master.gameObject.GetComponent<Units.UnitData>();

            if (!unitData) // maybe shouldnt do this lol
            {
                unitData = master.gameObject.AddComponent<Units.UnitData>();               
            }

            shopEntry.unitData = unitData;
        }
    }
}
