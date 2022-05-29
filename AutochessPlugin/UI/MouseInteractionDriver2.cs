using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.UI;
using RORAutochess.Units;

namespace RORAutochess.UI
{
	 
	public class MouseInteractionDriver2 : MonoBehaviour
	{
		 
		 
		 
		public Interactor interactor { get; private set; }
		public CharacterMaster master;
		 
		private void Awake()
		{
			this.networkIdentity = base.GetComponent<NetworkIdentity>();
			this.master = base.GetComponent<CharacterMaster>();
		}

		 
		private void FixedUpdate()
		{
			if (this.networkIdentity.hasAuthority)
			{
				this.interactableCooldown -= Time.fixedDeltaTime;
			}
			if (Input.GetMouseButtonDown(0)) // pickup
			{
				GameObject gameObject = this.FindBestInteractableObject();
				if (gameObject)
				{
					UnitPickupInteraction interaction = gameObject.GetComponent<UnitPickupInteraction>();
					if(interaction && interaction.GetInteractability(this.master) == Interactability.Available)
                    {
						interaction.PickupUnit(this.master);
						this.interactableCooldown = 0.25f;
					}
					
				}
			}
			if (Input.GetMouseButtonDown(1)) // display info
            {

            }
		}

		 
		public GameObject FindBestInteractableObject()
		{
			if (this.interactableOverride)
			{
				return this.interactableOverride;
			}
			float num = 0f;
			Ray originalAimRay = Camera.main.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);

			RaycastHit[] hits = Physics.SphereCastAll(originalAimRay.origin, 5f, originalAimRay.direction, 1000f, LayerIndex.CommonMasks.interactable | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Collide);

			float d = Mathf.Infinity;
			GameObject best = null;
			for (int i = 0; i < hits.Length; i++)
			{
				Collider collider = hits[i].collider;
				GameObject entity2 = EntityLocator.GetEntity(collider.gameObject);
				if (entity2)
				{
					UnitPickupInteraction component2 = entity2.GetComponent<UnitPickupInteraction>();
					if (component2 != null && component2.GetInteractability(this.master) != Interactability.Disabled)
					{
						Physics.Raycast(originalAimRay, out RaycastHit hit, 1000f, LayerIndex.world.intVal);
						float num3 = (collider.transform.position - hit.point).magnitude;
						if (d > num3)
						{
							d = num3;
							best = entity2.gameObject;
						}
					}
				}
			}

			return best;
		}

		 
		static MouseInteractionDriver2()
		{
			OutlineHighlight.onPreRenderOutlineHighlight = (Action<OutlineHighlight>)Delegate.Combine(OutlineHighlight.onPreRenderOutlineHighlight, new Action<OutlineHighlight>(MouseInteractionDriver2.OnPreRenderOutlineHighlight));
		}

		 
		private static void OnPreRenderOutlineHighlight(OutlineHighlight outlineHighlight)
		{
			if (!outlineHighlight.sceneCamera)
			{
				return;
			}
			if (!outlineHighlight.sceneCamera.cameraRigController)
			{
				return;
			}
			CharacterBody target = outlineHighlight.sceneCamera.cameraRigController.targetBody;
			if (!target)
			{
				return;
			}
			MouseInteractionDriver2 component = target.master.GetComponent<MouseInteractionDriver2>();
			if (!component)
			{
				return;
			}
			GameObject gameObject = component.FindBestInteractableObject();
			if (!gameObject)
			{
				return;
			}
			IInteractable component2 = gameObject.GetComponent<IInteractable>();
			Highlight component3 = gameObject.GetComponent<Highlight>();
			if (!component3)
			{
				return;
			}
			Color a = component3.GetColor();
			if (component2 != null && ((MonoBehaviour)component2).isActiveAndEnabled && component2.GetInteractability(component.interactor) == Interactability.ConditionsNotMet)
			{
				a = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
			}
			outlineHighlight.highlightQueue.Enqueue(new OutlineHighlight.HighlightInfo
			{
				renderer = component3.targetRenderer,
				color = a * component3.strength
			});
		}

		 
		public bool highlightInteractor;

		 
		private bool leftClicked;
		private bool rightClicked;
		 
		private NetworkIdentity networkIdentity;		 

		 
		[NonSerialized]
		public GameObject interactableOverride;

		 
		private const float interactableCooldownDuration = 0.25f;

		 
		private float interactableCooldown;
	}
}
