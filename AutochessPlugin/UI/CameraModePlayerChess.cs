using System;
using System.Runtime.CompilerServices;
using Rewired;
using UnityEngine;
using RoR2;
using RoR2.CameraModes;

namespace RORAutochess.UI
{

	public class CameraModePlayerChess : CameraModeBase
	{


		private Transform cameraTransform;

		public override object CreateInstanceData(CameraRigController cameraRigController)
		{
			return new CameraModePlayerChess.InstanceData();
		}


		public override void OnInstallInternal(object rawInstancedata, CameraRigController cameraRigController)
		{
			base.OnInstallInternal(rawInstancedata, cameraRigController);
			((CameraModePlayerChess.InstanceData)rawInstancedata).neutralFov = cameraRigController.baseFov;
		}


		public override bool IsSpectating(CameraRigController cameraRigController)
		{
			return this.isSpectatorMode;
		}


		public override void UpdateInternal(object rawInstanceData, in CameraModeBase.CameraModeContext context, out CameraModeBase.UpdateResult result)
		{
			Quaternion rotation = context.cameraInfo.previousCameraState.rotation;
			Vector3 position = context.cameraInfo.previousCameraState.position;
			float fov = context.cameraInfo.baseFov;
			if (!this.cameraTransform)
			{
				this.cameraTransform = ChessBoard.cameraTransform;
			}
			if(!this.cameraTransform)
            {
				Log.LogError("fuck");
            }

			result.cameraState.position = this.cameraTransform.position;
			result.cameraState.rotation = this.cameraTransform.rotation;
			result.cameraState.fov = 50f; // ?
			result.showSprintParticles = false;
			result.firstPersonTarget = null;

			this.UpdateCursor(rawInstanceData, context, result.cameraState, out result.crosshairWorldPosition);
		}

		

		protected void UpdateCursor(object rawInstanceData, in CameraModeBase.CameraModeContext context, in CameraState cameraState, out Vector3 cursorWorldPosition)
		{
			CameraModePlayerChess.InstanceData instanceData = (CameraModePlayerChess.InstanceData)rawInstanceData;
			instanceData.lastAimAssist = instanceData.aimAssist;
			Ray cursorRaycastRay = this.GetCursorRaycastRay(context);
			instanceData.lastCrosshairHurtBox = null;
			RaycastHit[] array = Physics.RaycastAll(cursorRaycastRay, context.cameraInfo.cameraRigController.maxAimRaycastDistance, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore);
			float num = float.PositiveInfinity;
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit2 = array[i];
				HurtBox hurtBox = raycastHit2.collider.GetComponent<HurtBox>();
				EntityLocator component = raycastHit2.collider.GetComponent<EntityLocator>();
				float distance = raycastHit2.distance;
				if (distance > 3f && num > distance)
				{
					if (hurtBox)
					{
						if (hurtBox.teamIndex == context.targetInfo.teamIndex)
						{
							goto IL_166; // ?
						}
						if (hurtBox.healthComponent && hurtBox.healthComponent.dontShowHealthbar)
						{
							hurtBox = null;
						}
					}

					num = distance;
					instanceData.lastCrosshairHurtBox = hurtBox;
				}
			IL_166:;
			}
			
			cursorWorldPosition = cursorRaycastRay.GetPoint(context.cameraInfo.cameraRigController.maxAimRaycastDistance);
			
		}


		private Ray GetCursorRaycastRay(in CameraModeBase.CameraModeContext context)
		{
			if (!context.cameraInfo.sceneCam)
			{
				return default(Ray);
			}
			
			return Camera.main.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
		}



		public override void OnTargetChangedInternal(object rawInstanceData, CameraRigController cameraRigController, in CameraModeBase.OnTargetChangedArgs args)
		{
			CameraModePlayerChess.InstanceData instanceData = (CameraModePlayerChess.InstanceData)rawInstanceData;
			if (!instanceData.hasEverHadTarget && args.newTarget)
			{
				CharacterBody component = args.newTarget.GetComponent<CharacterBody>();
				if (component)
				{
					CharacterDirection characterDirection = component.characterDirection;
					if (characterDirection)
					{
						instanceData.pitchYaw = new PitchYawPair(0f, characterDirection.yaw);
					}
				}
				instanceData.hasEverHadTarget = true;
			}
		}


		public override void MatchStateInternal(object rawInstanceData, in CameraModeBase.CameraModeContext context, in CameraState cameraStateToMatch)
		{
			((CameraModePlayerChess.InstanceData)rawInstanceData).SetPitchYawFromLookVector(cameraStateToMatch.rotation * Vector3.forward);
		}

        public override void CollectLookInputInternal(object rawInstanceData, in CameraModeContext context, out CollectLookInputResult result)
        {
			result.lookInput = Vector3.zero;
        }

        public override void ApplyLookInputInternal(object rawInstanceData, in CameraModeContext context, in ApplyLookInputArgs input)
        {
            
        }

        public bool isSpectatorMode;


		public static CameraModePlayerChess playerChess = new CameraModePlayerChess
		{
			isSpectatorMode = false
		};


		public static CameraModePlayerChess spectator = new CameraModePlayerChess
		{
			isSpectatorMode = true
		};


		protected class InstanceData
		{

			public void SetPitchYawFromLookVector(Vector3 lookVector)
			{
				float x = Mathf.Sqrt(lookVector.x * lookVector.x + lookVector.z * lookVector.z);
				this.pitchYaw.pitch = Mathf.Atan2(-lookVector.y, x) * 57.29578f;
				this.pitchYaw.yaw = Mathf.Repeat(Mathf.Atan2(lookVector.x, lookVector.z) * 57.29578f, 360f);
			}


			public float currentCameraDistance;


			public float cameraDistanceVelocity;


			public float stickAimPreviousAcceleratedMagnitude;


			public float minPitch;


			public float maxPitch;


			public PitchYawPair pitchYaw;


			public CameraRigController.AimAssistInfo lastAimAssist;


			public CameraRigController.AimAssistInfo aimAssist;


			public HurtBox lastCrosshairHurtBox;


			public bool hasEverHadTarget;


			public float neutralFov;


			public float neutralFovVelocity;
		}

	}

	
	
}



