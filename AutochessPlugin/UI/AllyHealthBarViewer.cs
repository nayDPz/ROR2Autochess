using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RoR2.UI;
using RoR2;

namespace RORAutochess.UI
{
	
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(Canvas))]
	public class AllyHealthBarViewer : MonoBehaviour, ILayoutGroup, ILayoutController
	{
		
		public TeamIndex viewerTeamIndex { get; set; }
		public CharacterMaster source;
		

		public static AllyHealthBarViewer FindInstance(CharacterMaster master)
        {
			foreach (AllyHealthBarViewer combatHealthBarViewer in AllyHealthBarViewer.instancesList)
			{
				if (combatHealthBarViewer.source == master)
					return combatHealthBarViewer;
			}
			return null;
		}
		static AllyHealthBarViewer()
        {
            RoR2.CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private static void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
			if(AutochessRun.instance)
            {
				foreach (AllyHealthBarViewer combatHealthBarViewer in AllyHealthBarViewer.instancesList)
				{
					if (body.teamComponent.teamIndex == combatHealthBarViewer.viewerTeamIndex)
					{
						combatHealthBarViewer.AddHealthBarInfo(body.healthComponent);
					}
				}
			}
			
		}

        private void Update()
		{
			this.SetDirty();
		}

		
		private void Awake()
		{
			this.rectTransform = (RectTransform)base.transform;
			this.canvas = base.GetComponent<Canvas>();
		}

		
		private void Start()
		{
			this.FindCamera();

			
		}

		
		private void FindCamera()
		{
			this.uiCamera = this.canvas.rootCanvas.worldCamera.GetComponent<UICamera>();
		}

		
		private void OnEnable()
		{
			AllyHealthBarViewer.instancesList.Add(this);

			if (this.source)
			{
				MinionOwnership.MinionGroup group = MinionOwnership.MinionGroup.FindGroup(this.source.netId);
				if (group != null)
				{
					foreach (MinionOwnership m in group.members)
					{
						this.AddHealthBarInfo(m.gameObject.GetComponent<CharacterMaster>().GetBodyObject().GetComponent<CharacterBody>().healthComponent);
					}
				}
			}
		}

		
		private void OnDisable()
		{
			AllyHealthBarViewer.instancesList.Remove(this);
			for (int i = this.trackedVictims.Count - 1; i >= 0; i--)
			{
				this.Remove(i);
			}
		}

		private void Remove(int trackedVictimIndex)
		{
			this.Remove(trackedVictimIndex, this.victimToHealthBarInfo[this.trackedVictims[trackedVictimIndex]]);
		}

		
		private void Remove(int trackedVictimIndex, AllyHealthBarViewer.HealthBarInfo healthBarInfo)
		{
			this.trackedVictims.RemoveAt(trackedVictimIndex);
			UnityEngine.Object.Destroy(healthBarInfo.healthBarRootObject);
			this.victimToHealthBarInfo.Remove(healthBarInfo.sourceHealthComponent);
		}

		
		private bool VictimIsValid(HealthComponent victim)
		{
			return victim && victim.alive && !victim.body.currentVehicle;
		}

		
		private void LateUpdate()
		{
			this.CleanUp();
		}

		
		private void CleanUp()
		{
			for (int i = this.trackedVictims.Count - 1; i >= 0; i--)
			{
				HealthComponent healthComponent = this.trackedVictims[i];
				if (!this.VictimIsValid(healthComponent))
				{
					this.Remove(i, this.victimToHealthBarInfo[healthComponent]);
				}
			}
		}

		
		private void UpdateAllHealthbarPositions(Camera sceneCam, Camera uiCam)
		{
			if (sceneCam && uiCam)
			{
				foreach (AllyHealthBarViewer.HealthBarInfo healthBarInfo in this.victimToHealthBarInfo.Values)
				{
					if (healthBarInfo.sourceTransform && healthBarInfo.healthBarRootObjectTransform)
					{
						Vector3 position = healthBarInfo.sourceTransform.position;
						position.y += healthBarInfo.verticalOffset;
						Vector3 vector = sceneCam.WorldToScreenPoint(position);
						vector.z = ((vector.z > 0f) ? 1f : -1f);
						Vector3 position2 = uiCam.ScreenToWorldPoint(vector);
						healthBarInfo.healthBarRootObjectTransform.position = position2;
					}
				}
			}
		}

		
		public void AddHealthBarInfo(HealthComponent victimHealthComponent)
		{
			AllyHealthBarViewer.HealthBarInfo healthBarInfo;
			if (!this.victimToHealthBarInfo.TryGetValue(victimHealthComponent, out healthBarInfo))
			{
				healthBarInfo = new AllyHealthBarViewer.HealthBarInfo();
				healthBarInfo.healthBarRootObject = UnityEngine.Object.Instantiate<GameObject>(Stuff.chessHealthbarPrefab, this.container);
				healthBarInfo.healthBarRootObjectTransform = healthBarInfo.healthBarRootObject.transform;
				healthBarInfo.healthBar = healthBarInfo.healthBarRootObject.GetComponentInChildren<HealthBar>();

				healthBarInfo.healthBar.source = victimHealthComponent;
				healthBarInfo.healthBarRootObject.GetComponentInChildren<UnitHealthbar>().targetBodyObject = victimHealthComponent.body.gameObject;
				healthBarInfo.healthBarRootObject.GetComponentInChildren<BuffDisplay>().source = victimHealthComponent.body;
				healthBarInfo.sourceHealthComponent = victimHealthComponent;
				healthBarInfo.verticalOffset = 3.67f;
				healthBarInfo.sourceTransform = (victimHealthComponent.body.coreTransform ?? victimHealthComponent.transform);
				ModelLocator component2 = victimHealthComponent.GetComponent<ModelLocator>();
				if (component2)
				{
					Transform modelTransform = component2.modelTransform;
					if (modelTransform)
					{
						ChildLocator component3 = modelTransform.GetComponent<ChildLocator>();
						if (component3)
						{
							Transform transform = component3.FindChild("HealthBarOrigin");
							if (transform)
							{
								healthBarInfo.sourceTransform = transform;
								//healthBarInfo.verticalOffset = 0f;
							}
						}
					}
				}
				this.victimToHealthBarInfo.Add(victimHealthComponent, healthBarInfo);
				this.trackedVictims.Add(victimHealthComponent);
			}

		}

		
		private void SetDirty()
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			if (!CanvasUpdateRegistry.IsRebuildingLayout())
			{
				LayoutRebuilder.MarkLayoutForRebuild(this.rectTransform);
			}
		}		
		private static void LayoutForCamera(UICamera uiCamera)
		{
			Camera camera = uiCamera.camera;
			Camera sceneCam = uiCamera.cameraRigController.sceneCam;
			for (int i = 0; i < AllyHealthBarViewer.instancesList.Count; i++)
			{
				AllyHealthBarViewer.instancesList[i].UpdateAllHealthbarPositions(sceneCam, camera);
			}
		}		
		public void SetLayoutHorizontal()
		{
			if (this.uiCamera)
			{
				AllyHealthBarViewer.LayoutForCamera(this.uiCamera);
			}
		}		
		public void SetLayoutVertical()
		{
		}

		
		public static readonly List<AllyHealthBarViewer> instancesList = new List<AllyHealthBarViewer>();
	
		public RectTransform container;
		
		private RectTransform rectTransform;
	
		private Canvas canvas;
	
		private UICamera uiCamera;
	
		private List<HealthComponent> trackedVictims = new List<HealthComponent>();
	
		private Dictionary<HealthComponent, AllyHealthBarViewer.HealthBarInfo> victimToHealthBarInfo = new Dictionary<HealthComponent, AllyHealthBarViewer.HealthBarInfo>();
	
		public float zPosition;
	
		private class HealthBarInfo
		{
			
			public HealthComponent sourceHealthComponent;

			
			public Transform sourceTransform;

			
			public GameObject healthBarRootObject;

			
			public Transform healthBarRootObjectTransform;

			
			public HealthBar healthBar;

			
			public float verticalOffset;

		
		}
	}
}
