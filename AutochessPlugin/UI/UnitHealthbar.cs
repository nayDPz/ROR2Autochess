using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine.UI;
namespace RORAutochess.UI
{
    public class UnitHealthbar : MonoBehaviour
    {
        public HealthBar healthBar;
        public EquipmentIcon equipmentIcon;
        public SkillIcon skill1Icon;
        public SkillIcon skill2Icon;
        public SkillIcon skill3Icon;
        public SkillIcon skill4Icon;
        public TextMeshProUGUI itemCountText;
        public BuffDisplay buffDisplay;
		public CanvasGroup canvasGroup;
		private float fadeOutStopwatch = 0.5f;
		private float fadeOutTime = 0.5f;
		private float sphereCd = 0.1f;
		private bool fade;
		private bool faded;

		private int inventoryCount;
		private Inventory targetInventory;
		public GameObject targetBodyObject;
		private CharacterMaster targetMaster;
		




		private bool setMat;
		private void SetMaterials()
        {
			if(!setMat)
            {
				setMat = true;
				
			}
        }
        private void Awake()
        {
			// lazy
			
			this.buffDisplay.buffIconPrefab = Stuff.buffIcon;

			this.canvasGroup = base.GetComponent<CanvasGroup>();  //////////////

		}

		private void UpdateAlpha()
        {
			this.sphereCd -= Time.fixedDeltaTime;
			if (this.sphereCd <= 0)
			{
				bool before = this.fade;
				if(this.targetBodyObject)
                {
					this.sphereCd = 0.1f;
					HurtBox[] h = new SphereSearch
					{
						radius = 11f,
						mask = LayerIndex.entityPrecise.mask,
						origin = this.targetBodyObject.transform.position + Vector3.back * 6.5f,
						queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
					}.RefreshCandidates().OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();
					if (h.Length > 1) this.fade = true;
					else this.fade = false;

					if (before != this.fade) this.fadeOutStopwatch = this.fadeOutTime;
				}			
			}

			this.fadeOutStopwatch -= Time.fixedDeltaTime;
			float start = this.canvasGroup.alpha;
			float end = this.fade ? 0.12f : 1f;
			this.canvasGroup.alpha = Mathf.Lerp(start, end, (this.fadeOutTime - this.fadeOutStopwatch) / this.fadeOutTime);
		}

        private void Update()
        {
			this.UpdateAlpha();

			if (!this.targetMaster && this.targetBodyObject) this.targetMaster = this.targetBodyObject.GetComponent<CharacterBody>().master;

			Inventory inventory = this.targetMaster ? this.targetMaster.inventory : null;
			if (this.itemCountText)
			{
				if(inventory)
                {
					int i = 0;
					foreach(ItemTierDef tier in ItemTierCatalog.allItemTierDefs) // :(
                    {
						i += inventory.GetTotalItemCountOfTier(tier.tier);
					}
					itemCountText.text = i.ToString();
				}
					
			}			
			if (this.targetBodyObject)
			{
				SkillLocator component2 = this.targetBodyObject.GetComponent<SkillLocator>();
				if (component2)
				{
					this.SetMaterials();
					if (this.skill1Icon)
					{
						this.skill1Icon.targetSkillSlot = SkillSlot.Primary;
						this.skill1Icon.targetSkill = component2.primary;
					}
					if (this.skill2Icon)
					{
						this.skill2Icon.targetSkillSlot = SkillSlot.Secondary;
						this.skill2Icon.targetSkill = component2.secondary;
					}
					if (this.skill3Icon)
					{
						this.skill3Icon.targetSkillSlot = SkillSlot.Utility;
						this.skill3Icon.targetSkill = component2.utility;
					}
					if (this.skill4Icon)
					{
						this.skill4Icon.targetSkillSlot = SkillSlot.Special;
						this.skill4Icon.targetSkill = component2.special;
					}
				}
				if(this.equipmentIcon)
				{
					equipmentIcon.targetInventory = inventory;
					equipmentIcon.targetEquipmentSlot = (this.targetBodyObject ? this.targetBodyObject.GetComponent<EquipmentSlot>() : null);
					equipmentIcon.playerCharacterMasterController = (this.targetMaster ? this.targetMaster.GetComponent<PlayerCharacterMasterController>() : null);
				}
				if (this.buffDisplay)
				{
					this.buffDisplay.source = this.targetBodyObject.GetComponent<CharacterBody>();
				}
			}
		}
    }
}
