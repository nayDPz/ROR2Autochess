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

		private Plane boardPlane = new Plane(Vector3.up * 3f, ChessBoard.boardPosition);

		private Transform cameraTransform;


		private float doubleSpaceTimer;

		public float minFocusLength;
		public float scrollSpeed = 6f;
		public float rotationSpeed = 0.2f;

		private float lerpDuration = 0.25f;
		private float lerpTimer;
		private Vector3? lerpVector;
		private float lerpDistance = 45f;

		private Vector3 desiredCameraPosition;
		private Vector3 desiredCameraAngles;
		private Quaternion desiredCameraRotation;

		private Vector3 rotationAnchor;
		private Vector3 initialOffset;
		private Vector2 mouseAnchor;
		private Vector3 mouseWorldAnchor;
		private float focusLength;
		private float baseFocusLength;

		private bool cameraControl;
		private bool dragging;
		private float dragSens = 1f;

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

			Ray ray = new Ray(this.cameraTransform.position, this.cameraTransform.rotation * Vector3.forward);
			this.boardPlane.Raycast(ray, out this.baseFocusLength);
		}

		public override void UpdateInternal(object rawInstanceData, in CameraModeBase.CameraModeContext context, out CameraModeBase.UpdateResult result)
		{
			this.doubleSpaceTimer -= Time.fixedDeltaTime;
			if (Input.GetKeyDown(KeyCode.Space))
			{
				if (this.doubleSpaceTimer > 0f)
					this.ResetCamera();
				this.doubleSpaceTimer = 0.25f;
			}

			
			

			if (!this.cameraTransform) // ?
			{
				this.cameraTransform = ChessBoard.cameraTransform;
				this.ResetCamera();
			}

			
			

			this.cameraControl = Input.GetKey(KeyCode.Space);



			Ray ray = new Ray(this.desiredCameraPosition, this.desiredCameraRotation * Vector3.forward);

			



			if (this.cameraControl)
            {
				
				if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.mouseScrollDelta.y != 0)
					this.boardPlane.Raycast(ray, out focusLength);

				Vector3 cursorPos = context.cameraInfo.sceneCam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, focusLength));

				Vector3 currentCameraPos = context.cameraInfo.sceneCam.transform.position;
				Quaternion currentCameraRot = context.cameraInfo.sceneCam.transform.rotation;

				if (Input.GetMouseButton(0))
				{
					if (Input.GetMouseButtonDown(0))
					{
						this.dragging = false;
						this.mouseWorldAnchor = cursorPos;
					}

					Vector3 d = cursorPos - currentCameraPos;

					if (!this.dragging && (this.mouseWorldAnchor - cursorPos).magnitude > this.dragSens) this.dragging = true;

					if (this.dragging)
						this.desiredCameraPosition = this.mouseWorldAnchor - d;


				}


				if (Input.GetMouseButtonUp(0) && !this.dragging)
				{
					this.lerpTimer = this.lerpDuration;
					Vector3 between = currentCameraPos - ray.GetPoint(focusLength);
					between = between.normalized * Mathf.Min(this.lerpDistance, focusLength);
					Ray ray2 = context.cameraInfo.sceneCam.ScreenPointToRay(Input.mousePosition);
					this.boardPlane.Raycast(ray2, out float d);
					this.lerpVector = ray2.GetPoint(d) + between;

				}

				if (Input.GetMouseButton(1))
				{

					if (Input.GetMouseButtonDown(1))
					{
						this.mouseAnchor = Input.mousePosition;
						this.rotationAnchor = ray.GetPoint(focusLength); // pointless
						this.initialOffset = currentCameraPos - this.rotationAnchor;
					}

					if(this.lerpVector == null)
                    {
						Vector2 d = this.mouseAnchor - new Vector2(Input.mousePosition.x, Input.mousePosition.y);
						this.desiredCameraAngles.y = -d.x * this.rotationSpeed;
						this.desiredCameraAngles.x = -d.y * this.rotationSpeed;


						Vector3 cross = currentCameraRot * Vector3.left;
						Quaternion pitch = Quaternion.AngleAxis(this.desiredCameraAngles.x, cross);
						Quaternion yaw = Quaternion.Euler(0f, this.desiredCameraAngles.y, 0f);

						Vector3 newOffset = yaw * this.initialOffset; // FUCK PITCH FUCK PITCH FUCK PITCH

						this.desiredCameraPosition = newOffset + this.rotationAnchor;
						this.desiredCameraRotation = Quaternion.LookRotation(-newOffset);
					}
					
					


					
				}

				float s = Input.mouseScrollDelta.y;
				if (s != 0)
				{
					Quaternion r = s > 0 && !Input.GetMouseButton(1) ? Quaternion.LookRotation(cursorPos - context.cameraInfo.previousCameraState.position) : this.desiredCameraRotation;
					this.desiredCameraPosition += (r * Vector3.forward) * s * this.scrollSpeed;
				}
				
				
			}

			this.lerpTimer -= Time.fixedDeltaTime;
			if (this.lerpTimer < 0f)
				this.lerpVector = null;		
			if(this.lerpVector != null)
            {
				this.desiredCameraPosition = Vector3.Lerp((Vector3)this.lerpVector, this.desiredCameraPosition, this.lerpTimer / this.lerpDuration);
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



