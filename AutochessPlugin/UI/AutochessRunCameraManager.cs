using System;
using System.Collections.ObjectModel;
using RoR2.CameraModes;
using UnityEngine;
using RoR2;
using RoR2.UI;

namespace RORAutochess.UI
{
	 
	public class AutochessRunCameraManager : MonoBehaviour // this is pretty bad i think
	{

		private void Update()
		{
			if (Stage.instance)
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
						cameraRigController.createHud = false;


						GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(AutochessRun.ui); // idk
						gameObject.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster").Find("ChatBoxRoot").GetComponent<InstantiatePrefabBehavior>().prefab = Stuff.chatBoxPrefab; // this sucks
						gameObject.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("AutochessRunInfoHudPanel").Find("TimerPanel").Find("TimerText").GetComponent<TimerText>().format = Stuff.timerTextThing;
						cameraRigController.hud = gameObject.GetComponent<HUD>();
						cameraRigController.hud.cameraRigController = cameraRigController;
						cameraRigController.hud.GetComponent<Canvas>().worldCamera = cameraRigController.uiCam;
						cameraRigController.hud.GetComponent<CrosshairManager>().cameraRigController = cameraRigController;
						cameraRigController.hud.localUserViewer = cameraRigController.localUserViewer;

					}
					cameraRigController.viewer = networkUser;
					networkUser.cameraRigController = cameraRigController;

					cameraRigController.nextTarget = RunCameraManager.GetNetworkUserBodyObject(networkUser);
					cameraRigController.cameraMode = CameraModePlayerChess.playerChess;

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
