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

		private Plane boardPlane = new Plane(Vector3.up, ChessBoard.boardPosition);

		private Transform cameraTransform;

		private float doubleClickTimer;


		public float scrollSpeed = 2f;
		public float rotationSpeed = 1f;
		

		private Vector3 desiredCameraPosition;
		private Vector3 desiredCameraAngles;
		private Quaternion desiredCameraRotation;

		private Vector3 mouseAnchor;
		private float focusLength;

		private bool cameraControl;
		private bool moveCamera;
		private bool rotateCamera;

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


		private void ResetCamera()
        {
			this.desiredCameraPosition = this.cameraTransform.position;
			this.desiredCameraRotation = this.cameraTransform.rotation;
			this.desiredCameraAngles = this.desiredCameraRotation.eulerAngles;
		}

		public override void UpdateInternal(object rawInstanceData, in CameraModeBase.CameraModeContext context, out CameraModeBase.UpdateResult result)
		{
			this.doubleClickTimer -= Time.fixedDeltaTime;
			if (Input.GetKeyDown(KeyCode.Space))
			{
				if (this.doubleClickTimer > 0f)
					this.ResetCamera();
				this.doubleClickTimer = 0.25f;
			}

			
			

			if (!this.cameraTransform) // ?
			{
				this.cameraTransform = ChessBoard.cameraTransform;
				this.ResetCamera();
			}

			
			

			this.cameraControl = Input.GetKey(KeyCode.Space);



			Ray ray = new Ray(context.cameraInfo.previousCameraState.position, context.cameraInfo.previousCameraState.rotation * Vector3.forward);

			if(!Input.GetMouseButton(0))
				this.boardPlane.Raycast(ray, out focusLength);



			if (this.cameraControl)
            {
				if (Input.GetMouseButtonDown(0))
				{
					
					this.mouseAnchor = context.cameraInfo.sceneCam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, focusLength));
				}

				
				if (Input.GetMouseButton(0))
                {
					Vector3 d = context.cameraInfo.sceneCam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, focusLength)) - context.cameraInfo.previousCameraState.position;
					this.desiredCameraPosition = this.mouseAnchor - d;
				}

				
				if(Input.GetMouseButton(1))
                {
					if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
					{
						this.desiredCameraAngles.y += -Input.GetAxis("Mouse X") * this.rotationSpeed;
						this.desiredCameraAngles.x += Input.GetAxis("Mouse Y") * this.rotationSpeed;


						this.desiredCameraRotation = Quaternion.Euler(this.desiredCameraRotation.x + this.desiredCameraAngles.x, this.desiredCameraRotation.y + this.desiredCameraAngles.y, this.desiredCameraRotation.z);
					}
				}


			}

			float s = Input.mouseScrollDelta.y;
			if (s != 0)
			{
				this.desiredCameraPosition += (this.desiredCameraRotation * Vector3.forward) * s * this.scrollSpeed;
			}





			result.cameraState.position = this.desiredCameraPosition;
			result.cameraState.rotation = this.desiredCameraRotation;
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



