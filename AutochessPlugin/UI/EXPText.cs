using System;
using UnityEngine;
using UnityEngine.TextCore;
using RoR2;
using TMPro;
namespace RORAutochess.UI
{
	// Token: 0x02000CF6 RID: 3318
	[RequireComponent(typeof(RectTransform))]
	public class EXPText : MonoBehaviour
	{

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
		public CharacterMaster source;
	}
}
