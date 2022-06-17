using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RORAutochess.Board;
using RoR2;
using TMPro;
using RoR2.UI;
namespace RORAutochess.UI
{
    class RoundTimerBar : MonoBehaviour
    {
		public void Awake()
		{
			this.timerText = base.transform.Find("TimerText").GetComponent<TimerText>();
			this.timerText.format = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<TimerStringFormatter>("RoR2/Base/Common/tsfRunStopwatch.asset").WaitForCompletion(); // O_O
		}

		public void Update()
		{
			float x = 0f;
			float max = 50f;
			if (RoundController.instance)
			{
				
				//max = RoundController.instance.currentPhaseDuration;
				//x = max - RoundController.instance.currentPhaseTime;

			}
			if (this.fillRectTransform)
			{
				float fill = Mathf.Lerp(0, rectBase.rect.width, x / max);
				this.fillRectTransform.sizeDelta = new Vector2(fill, 16);
			}

			if (this.timerText)
				this.timerText.seconds = x;

		}

		public TimerText timerText;
		public RectTransform fillRectTransform;
		public RectTransform rectBase;
	}
}
