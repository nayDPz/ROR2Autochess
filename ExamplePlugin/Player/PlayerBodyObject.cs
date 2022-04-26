using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using R2API;
using UnityEngine;
using RoR2;

namespace RORAutochess.Player
{
    public class PlayerBodyObject
    {
        internal static GameObject bodyPrefab;
        internal static void CreatePrefab() // probably cant do this until after UI
        {
            bodyPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion(), "AutochessPlayerBody");
            GameObject.Destroy(bodyPrefab.GetComponent<CharacterMotor>());
            GameObject.Destroy(bodyPrefab.GetComponent<CharacterDirection>());
            GameObject.Destroy(bodyPrefab.GetComponent<CharacterDeathBehavior>());
            GameObject.Destroy(bodyPrefab.GetComponent<GenericSkill>());
            GameObject.Destroy(bodyPrefab.GetComponent<GenericSkill>());
            GameObject.Destroy(bodyPrefab.GetComponent<GenericSkill>());
            GameObject.Destroy(bodyPrefab.GetComponent<GenericSkill>());
            
            GameObject.Destroy(bodyPrefab.GetComponent<SfxLocator>());
            //GameObject.Destroy(bodyPrefab.GetComponent<SfxLocator>());
            GameObject.Destroy(bodyPrefab.GetComponent<HealthComponent>());
            GameObject.Destroy(bodyPrefab.GetComponent<KinematicCharacterController.KinematicCharacterMotor>());
            GameObject.Destroy(bodyPrefab.GetComponent<CapsuleCollider>());
            GameObject.Destroy(bodyPrefab.GetComponent<Rigidbody>());
            GameObject.Destroy(bodyPrefab.GetComponent<EntityStateMachine>());


            GameObject.Destroy(bodyPrefab.GetComponent<InputBankTest>());
            GameObject.Destroy(bodyPrefab.GetComponent<TeamComponent>());

            GameObject.Destroy(bodyPrefab.GetComponent<SkillLocator>());
            bodyPrefab.AddComponent<UI.MouseInteractionDriver>();
            ContentPacks.bodyPrefabs.Add(bodyPrefab);

        }
    }
}
