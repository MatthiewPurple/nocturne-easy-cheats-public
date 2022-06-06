using MelonLoader;
using HarmonyLib;
using UnityEngine;
using newdata_H;
using System;
using UniverseLib.UI;
using UnityEngine.UI;
using UniverseLib.UI.Models;
using Stolen;
using System.Collections.Generic;
using System.Collections;

[assembly: MelonInfo(typeof(EasyCheats.EasyCheats), "Easy Cheats", "2.0.0", "Matthiew Purple & Kraby")]
[assembly: MelonGame("", "smt3hd")]

namespace EasyCheats
{
    public class EasyCheats : MelonMod
    {
        public const float GuiCreationDelay = 5f;   // in seconds

        static bool displayingDemon = false;
        public static bool EncountersEnabled = true;
        
        public static DemonListWindow DemonList = null;
        public static ItemListWindow ItemList = null;
        public static SkillListWindow SkillList = null;



        [HarmonyPatch(typeof(cmpDrawStatus), nameof(cmpDrawStatus.cmpDrawAisyoHelp))]
        private class Patch
        {
            public static void Postfix(datUnitWork_t pStock)
            {
                displayingDemon = true;
                
                // Disable UI incompatible with this menu
                DemonList.SetActive(false);
                ItemList.SetActive(false);


                if (!SkillList.Active)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
                    {
                        //+1 level
                        if (pStock.level < 99)
                        {
                            pStock.level++;
                        }
                        else
                        {
                            pStock.level = 1;
                            pStock.exp = 0;
                        }

                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        //+1 strength
                        if (pStock.param[0] < 40)
                        {
                            pStock.param[0]++;
                        }
                        else
                        {
                            pStock.param[0] = 1;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        //+1 magic
                        if (pStock.param[2] < 40)
                        {
                            pStock.param[2]++;
                        }
                        else
                        {
                            pStock.param[2] = 1;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                    {
                        //+1 vitality
                        if (pStock.param[3] < 40)
                        {
                            pStock.param[3]++;
                        }
                        else
                        {
                            pStock.param[3] = 1;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                    {
                        //+1 agility
                        if (pStock.param[4] < 40)
                        {
                            pStock.param[4]++;
                        }
                        else
                        {
                            pStock.param[4] = 1;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
                    {
                        //+1 luck
                        if (pStock.param[5] < 40)
                        {
                            pStock.param[5]++;
                        }
                        else
                        {
                            pStock.param[5] = 1;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
                    {
                        //+10 max HP
                        if (pStock.maxhp < 999)
                        {
                            pStock.maxhp += (ushort)Math.Min(10, 999 - pStock.maxhp);
                            //pStock.hp = pStock.maxhp;
                        }
                        else
                        {
                            pStock.maxhp = 1;
                            pStock.hp = 1;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
                    {
                        //+10 max MP
                        if (pStock.maxmp < 999)
                        {
                            pStock.maxmp += (ushort)Math.Min(10, 999 - pStock.maxmp);
                            //pStock.mp = pStock.maxmp;
                        }
                        else
                        {
                            pStock.maxmp = 1;
                            pStock.mp = 1;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
                    {
                        //1 exp away from next level
                        pStock.exp += rstCalcCore.GetNextExpDisp(pStock, pStock.exp) - 1;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
                    {
                        for (int i = 0; i < pStock.skill.Length; ++i)
                            Debug.Log($"skill {i} = {pStock.skill[i]}");

                        Debug.Log($"skillcnt={pStock.skillcnt}");
                        Debug.Log($"skill.length={pStock.skill.Length}");

                        if (pStock.skillcnt == 8)
                        {
                            pStock.skillcnt = 0;
                            for (int i = 0; i < pStock.skill.Length; ++i)
                                pStock.skill[i] = 0;
                        }

                        // display skill list menu
                        SkillList.SetActive(true);
                    }
                }
                else
                {
                    SkillList.WorkingUnit = pStock;
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        SkillList.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(fclRecoverCalc), nameof(fclRecoverCalc.rcvAppRecover))]
        private class Patch2
        {
            public static void Postfix(datUnitWork_t pStock)
            {
                pStock.hp = pStock.maxhp;
                pStock.mp = pStock.maxmp;
            }
        }

        [HarmonyPatch(typeof(nbEncount), nameof(nbEncount.nbEncountCalc))]
        private class PatchEncounters
        {
            public static void Postfix(ref int __result)
            {
                if (!EncountersEnabled)
                    __result = 0;
            }
        }



        public override void OnUpdate()
        {
            DragPanel.UpdateInstances();
        }

        public override void OnLateUpdate()
        {
            if (!displayingDemon)
            {
                if (SkillList != null)
                    SkillList.SetActive(false);

                if ((ItemList != null && ItemList.Active) || (DemonList != null && DemonList.Active))
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        DemonList.SetActive(false);
                        ItemList.SetActive(false);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
                    {
                        //+10000 macca
                        if (dds3GlobalWork.DDS3_GBWK.maka < 99999999)
                        {
                            dds3GlobalWork.DDS3_GBWK.maka += 10000;
                        }
                        else
                        {
                            dds3GlobalWork.DDS3_GBWK.maka = 0;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        //+1 demon slot
                        if (dds3GlobalWork.DDS3_GBWK.maxstock < 12) dds3GlobalWork.DDS3_GBWK.maxstock++;
                        else dds3GlobalWork.DDS3_GBWK.maxstock = dds3GlobalWork.DDS3_GBWK.stockcnt - 1;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        //+1 kagutsuchi phase
                        if (dds3GlobalWork.DDS3_GBWK.Moon.Age < 15) dds3GlobalWork.DDS3_GBWK.Moon.Age++;
                        else dds3GlobalWork.DDS3_GBWK.Moon.Age = 0;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                    {
                        //freeze kagutsuchi
                        evtMoon.evtDisableMoonCalc();
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                    {
                        //unfreeze kagutsuchi
                        evtMoon.evtEnableMoonCalc();
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
                    {
                        EncountersEnabled = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
                    {
                        EncountersEnabled = false;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
                    {
                        foreach(var unit in dds3GlobalWork.DDS3_GBWK.unitwork)
                        {
                            fclRecoverCalc.rcvAppAllRecover(unit);
                        }
                    }                   
                    else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
                    {
                        // display demon list window
                        DemonList.SetActive(true);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
                    {
                        // display item list window
                        ItemList.SetActive(true);
                    }
                }

            }

            displayingDemon = false;
        }
    
        public override void OnApplicationLateStart()
        {
            MelonLoader.MelonCoroutines.Start(createGui());
        }



        private IEnumerator createGui()
        {
            yield return new WaitForSeconds(GuiCreationDelay);

            DemonList = new DemonListWindow();
            DemonList.SetActive(false);
            ItemList = new ItemListWindow();
            ItemList.SetActive(false);
            SkillList = new SkillListWindow();
            SkillList.SetActive(false);
        }
    }
}


