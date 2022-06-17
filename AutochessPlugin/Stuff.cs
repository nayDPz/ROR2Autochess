using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2;
using RORAutochess.UI;
using UnityEngine.UI;

namespace RORAutochess
{
    public static class Stuff
    {
		public static GameObject podShopPrefab;
        public static GameObject chessHealthbarPrefab;
		public static GameObject podSpawnPointPrefab;
		public static GameObject playerBodyPrefab;

		// Vanilla stuff
		public static InteractableSpawnCard teleporterSpawnCard;
		public static GameObject chatBoxPrefab;
        public static GameObject buffIcon;
        public static Material uiCooldownMat;
        public static Material uiFlashMat;
        public static Material transparentBlueMat;
		public static TimerStringFormatter timerTextThing;
        public static void LoadStuff()
        {

			// SHOULD JUST DO SHADER SWAP EVENTUALLY
			
			buffIcon = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/BuffIcon.prefab").WaitForCompletion();
            uiFlashMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/UI/matUIPaintStreakFlash.mat").WaitForCompletion();
            uiCooldownMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/UI/matUISkillCD.mat").WaitForCompletion();
            transparentBlueMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/matEditorTemporary.mat").WaitForCompletion();


			teleporterSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Teleporters/iscTeleporter.asset").WaitForCompletion();
			chatBoxPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/ChatBox, In Run.prefab").WaitForCompletion();
			timerTextThing = Addressables.LoadAssetAsync<TimerStringFormatter>("RoR2/Base/Common/tsfRunStopwatch.asset").WaitForCompletion();


			podShopPrefab = AutochessPlugin.assetbundle.LoadAsset<GameObject>("PodShop");
			podShopPrefab.transform.Find("PodShopSlot").Find("FlashPanel").GetComponent<Image>().material = Stuff.uiFlashMat;
			//podShopPrefab.transform.Find("FlashPanel").Find("FlashPanel, Expanding").GetComponent<Image>().material = Stuff.uiFlashMat;

			podSpawnPointPrefab = AutochessPlugin.assetbundle.LoadAsset<GameObject>("PodSpawnPoint");
			podSpawnPointPrefab.transform.Find("EscapePodMesh").GetComponent<MeshRenderer>().material = transparentBlueMat;

			chessHealthbarPrefab = AutochessPlugin.assetbundle.LoadAsset<GameObject>("ChessHealthbar");
            UnitHealthbar h = chessHealthbarPrefab.GetComponent<UnitHealthbar>();
			h.skill1Icon.gameObject.transform.Find("FlashPanel").GetComponent<Image>().material = Stuff.uiFlashMat;
			h.skill1Icon.gameObject.transform.Find("FlashPanel").Find("FlashPanel, Expanding").GetComponent<Image>().material = Stuff.uiFlashMat;
			h.skill1Icon.gameObject.transform.Find("CooldownPanel").GetComponent<RawImage>().material = Stuff.uiCooldownMat;
			
			h.skill2Icon.gameObject.transform.Find("FlashPanel").GetComponent<Image>().material = Stuff.uiFlashMat;
			h.skill2Icon.gameObject.transform.Find("FlashPanel").Find("FlashPanel, Expanding").GetComponent<Image>().material = Stuff.uiFlashMat;
			h.skill2Icon.gameObject.transform.Find("CooldownPanel").GetComponent<RawImage>().material = Stuff.uiCooldownMat;
			
			h.skill3Icon.gameObject.transform.Find("FlashPanel").GetComponent<Image>().material = Stuff.uiFlashMat;
			h.skill3Icon.gameObject.transform.Find("FlashPanel").Find("FlashPanel, Expanding").GetComponent<Image>().material = Stuff.uiFlashMat;
			h.skill3Icon.gameObject.transform.Find("CooldownPanel").GetComponent<RawImage>().material = Stuff.uiCooldownMat;
			
			h.skill4Icon.gameObject.transform.Find("FlashPanel").GetComponent<Image>().material = Stuff.uiFlashMat;
			h.skill4Icon.gameObject.transform.Find("FlashPanel").Find("FlashPanel, Expanding").GetComponent<Image>().material = Stuff.uiFlashMat;
			h.skill4Icon.gameObject.transform.Find("CooldownPanel").GetComponent<RawImage>().material = Stuff.uiCooldownMat;

			playerBodyPrefab = AutochessPlugin.assetbundle.LoadAsset<GameObject>("AutochessPlayerBody");
			ContentPacks.bodyPrefabs.Add(playerBodyPrefab);

			
		}
    }
}
