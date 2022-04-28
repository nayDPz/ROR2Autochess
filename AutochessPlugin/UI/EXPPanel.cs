using System;
using UnityEngine;
using UnityEngine.TextCore;
using RoR2;
using TMPro;
namespace RORAutochess.UI
{
	// Token: 0x02000CF6 RID: 3318
	[RequireComponent(typeof(RectTransform))]
	public class EXPPanel : MonoBehaviour
	{
		// Token: 0x06004B77 RID: 19319 RVA: 0x00135751 File Offset: 0x00133951
		private void Awake()
		{
			this.rectTransform = base.GetComponent<RectTransform>();
		}

		// Token: 0x06004B78 RID: 19320 RVA: 0x00135760 File Offset: 0x00133960
		public void Update()
		{
			TeamIndex teamIndex = this.source ? this.source.teamIndex : TeamIndex.Neutral;
			float x = 0f;
			float max = 50f;
			if (this.source && TeamManager.instance)
			{
				x = TeamManager.instance.GetTeamExperience(teamIndex) - TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex);
				max = TeamManager.instance.GetTeamNextLevelExperience(teamIndex) - TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex);
				//x = Mathf.InverseLerp(TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex), TeamManager.instance.GetTeamNextLevelExperience(teamIndex), TeamManager.instance.GetTeamExperience(teamIndex));

			}
			if (this.currentXP)
			{
				this.currentXP.text = x.ToString();
			}
			if (this.maxXP)
			{
				this.maxXP.text = max.ToString();
			}
		}


		public TextMeshProUGUI currentXP;
		public TextMeshProUGUI maxXP;

		// Token: 0x04004842 RID: 18498
		public CharacterMaster source;

		// Token: 0x04004843 RID: 18499
		public RectTransform fillRectTransform;

		// Token: 0x04004844 RID: 18500
		private RectTransform rectTransform;
	}
}
