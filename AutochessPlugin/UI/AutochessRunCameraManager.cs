using System;
using System.Collections.ObjectModel;
using RoR2.CameraModes;
using UnityEngine;
using RoR2;

namespace RORAutochess.UI
{
	 
	public class AutochessRunCameraManager : MonoBehaviour // this is pretty bad i think
	{
		 
		private static GameObject GetNetworkUserBodyObject(NetworkUser networkUser)
		{
			if (networkUser.masterObject)
			{
				CharacterMaster component = networkUser.masterObject.GetComponent<CharacterMaster>();
				if (component)
				{
					return component.GetBodyObject();
				}
			}
			return null;
		}
		 
		private void Update()
		{
			bool flag = Stage.instance;
			if (flag)
			{
				int i = 0;
				int count = CameraRigController.readOnlyInstancesList.Count;
				while (i < count)
				{
					if (CameraRigController.readOnlyInstancesList[i].suppressPlayerCameras)
					{
						return;
					}
					i++;
				}
			}
			if (flag)
			{
				int num = 0;
				ReadOnlyCollection<NetworkUser> readOnlyLocalPlayersList = NetworkUser.readOnlyLocalPlayersList;
				for (int j = 0; j < readOnlyLocalPlayersList.Count; j++)
				{
					NetworkUser networkUser = readOnlyLocalPlayersList[j];
					CameraRigController cameraRigController = this.cameras[num];
					if (!cameraRigController)
					{
						cameraRigController = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/Main Camera")).GetComponent<CameraRigController>();
						this.cameras[num] = cameraRigController;

						Transform sc = cameraRigController.gameObject.transform.Find("Scene Camera");
						//if(sc) // idk what this is
      //                  {
						//	RoR2.PostProcessing.VisionLimitEffect v = sc.gameObject.GetComponent<RoR2.PostProcessing.VisionLimitEffect>();
						//	if (v) Destroy(v);
      //                  }
					}
					cameraRigController.viewer = networkUser;
					networkUser.cameraRigController = cameraRigController;
					GameObject networkUserBodyObject = AutochessRunCameraManager.GetNetworkUserBodyObject(networkUser);
					ForceSpectate forceSpectate = InstanceTracker.FirstOrNull<ForceSpectate>();
					if (forceSpectate)
					{
						cameraRigController.nextTarget = forceSpectate.target;
						cameraRigController.cameraMode = CameraModePlayerChess.spectator;
					}
					else if (networkUserBodyObject)
					{
						cameraRigController.nextTarget = networkUserBodyObject;
						cameraRigController.cameraMode = CameraModePlayerChess.playerChess;
					}
					else if (!cameraRigController.disableSpectating)
					{
						cameraRigController.cameraMode = CameraModePlayerChess.spectator;
						if (!cameraRigController.target)
						{
							cameraRigController.nextTarget = CameraRigControllerSpectateControls.GetNextSpectateGameObject(networkUser, null);
						}
					}
					else
					{
						cameraRigController.cameraMode = CameraModeNone.instance;
					}
					num++;
				}
				int num2 = num;
				for (int k = num; k < this.cameras.Length; k++)
				{
					ref CameraRigController ptr = ref this.cameras[num];
					if (ptr != null)
					{
						if (ptr)
						{
							UnityEngine.Object.Destroy(this.cameras[num].gameObject);
						}
						ptr = null;
					}
				}
				Rect[] array = AutochessRunCameraManager.screenLayouts[num2];
				for (int l = 0; l < num2; l++)
				{
					this.cameras[l].viewport = array[l];
				}
				return;
			}
			for (int m = 0; m < this.cameras.Length; m++)
			{
				if (this.cameras[m])
				{
					UnityEngine.Object.Destroy(this.cameras[m].gameObject);
				}
			}
		}

		 
		private readonly CameraRigController[] cameras = new CameraRigController[RoR2Application.maxLocalPlayers];

		 
		private static readonly Rect[][] screenLayouts = new Rect[][]
		{
			new Rect[0],
			new Rect[]
			{
				new Rect(0f, 0f, 1f, 1f)
			},
			new Rect[]
			{
				new Rect(0f, 0.5f, 1f, 0.5f),
				new Rect(0f, 0f, 1f, 0.5f)
			},
			new Rect[]
			{
				new Rect(0f, 0.5f, 1f, 0.5f),
				new Rect(0f, 0f, 0.5f, 0.5f),
				new Rect(0.5f, 0f, 0.5f, 0.5f)
			},
			new Rect[]
			{
				new Rect(0f, 0.5f, 0.5f, 0.5f),
				new Rect(0.5f, 0.5f, 0.5f, 0.5f),
				new Rect(0f, 0f, 0.5f, 0.5f),
				new Rect(0.5f, 0f, 0.5f, 0.5f)
			}
		};
	}
}
