using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.UI;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RORAutochess.UI
{
    public class InventoryClickDrag : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static void Init()
        {
            
            //GameObject icon = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/ItemIcon.prefab").WaitForCompletion();
            //icon.AddComponent<HGButton>();
            //icon.AddComponent<InventoryClickDrag>();
            
        }

        private HGButton button;
        private Transform baseParent;
        private CharacterMaster master;
        private int stack;
        private ItemIndex index;

        private bool selected;
        private bool dragging;
        private void Awake()
        {
            this.baseParent = base.transform.parent;
            this.master = base.transform.parent.gameObject.GetComponent<ItemInventoryDisplay>().inventory.gameObject.GetComponent<CharacterMaster>();
            this.button = base.GetComponent<HGButton>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new UnityAction(OnButtonClicked));

            
        }

        private void FixedUpdate()
        {
              
            if(this.selected && !this.dragging)
            {
                Vector3 v = Input.mousePosition;
                v.z = 12.35f;
                v.x -= Screen.width / 2;
                v.y -= Screen.height / 2;
                base.transform.localPosition = v;
            }
        }

        private void ExchangeItem(CharacterMaster from, CharacterMaster to)
        {
            ItemIcon icon = base.GetComponent<ItemIcon>();
            stack = icon.itemCount;
            index = icon.itemIndex;
            from.inventory.RemoveItem(index, stack);
            to.inventory.GiveItem(index, stack);
        }

        public void OnButtonClicked()
        {
            //if (this.selected)
            //{
            //    Vector3 v = Input.mousePosition;
            //    v.z = 12.35f;
            //    v.x -= Screen.width / 2;
            //    v.y -= Screen.height / 2;
            //    base.transform.localPosition = v;
            //}
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!this.selected)
            {
                this.selected = true;


                base.transform.SetParent(GameObject.Find("HUDAutochess(Clone)").transform.Find("MainContainer")); // ???



                Chat.AddMessage(ItemCatalog.GetItemDef(index).name);
            }
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 v = Input.mousePosition;
            v.z = 12.35f;
            v.x -= Screen.width / 2;
            v.y -= Screen.height / 2;
            base.transform.localPosition = v;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!this.selected)
            {
                this.dragging = true;
                selected = true;
                base.transform.SetParent(GameObject.Find("HUDAutochess(Clone)").transform.Find("MainContainer"));
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            this.dragging = false;
            if (this.selected)
            {
                this.selected = false;
                base.transform.SetParent(this.baseParent);
                base.transform.parent.gameObject.GetComponent<ItemInventoryDisplay>().OnIconScaleChanged(); // resets back to position, nothing else seemed to work

                MouseInteractionDriver2 d = this.master.GetComponent<MouseInteractionDriver2>();
                if (d)
                {
                    GameObject target = d.FindBestInteractableObject();
                    if (target)
                    {
                        CharacterBody targetBody = target.GetComponent<CharacterBody>();
                        if (targetBody)
                        {
                            GameObject master = targetBody.masterObject;
                            MinionOwnership mo = master.GetComponent<MinionOwnership>();
                            if (mo)
                            {
                                if (mo.group == MinionOwnership.MinionGroup.FindGroup(this.master.netId))
                                {
                                    this.ExchangeItem(this.master, master.GetComponent<CharacterMaster>());
                                }
                            }
                        }
                    }

                }
                

            }
        }
    }
}
