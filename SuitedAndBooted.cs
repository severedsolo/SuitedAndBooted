using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Localization;
using KSP.UI.Screens;
using UnityEngine;

namespace SuitedAndBooted
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class SuitedAndBootedSc : SuitedAndBooted
    {
        
    }
    
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SuitedAndBootedFlight : SuitedAndBooted
    {
        
    }
    
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class SuitedAndBootedEditor : SuitedAndBooted
    {
        
    }
    public class SuitedAndBooted : MonoBehaviour
    {
        public List<SuitCombo> SuitVariants;
        public readonly Dictionary<int, SuitRule> Rules = new Dictionary<int, SuitRule>();
        public static SuitedAndBooted Instance;

        private void Awake()
        {
            Instance = this;
            GameEvents.OnCrewmemberHired.Add(NewCrew);
            GameEvents.onGUIAstronautComplexSpawn.Add(AstronautComplexSpawn);
            GameEvents.onGUILaunchScreenSpawn.Add(LaunchScreenSpawn);
            GameEvents.onAttemptEva.Add(OnEVA);
        }

        private void OnEVA(ProtoCrewMember data0, Part data1, Transform data2)
        {
            RunRules();
        }

        private void LaunchScreenSpawn(GameEvents.VesselSpawnInfo data)
        {
            RunRules();
        }

        private void AstronautComplexSpawn()
        {
            RunRules();
        }

        private void NewCrew(ProtoCrewMember newCrewMember, int numOfCrew)
        {
            RunRules();
        }

        private void Start()
        {
            SuitCombos sc = GameDatabase.Instance.GetComponent<SuitCombos>();
            SuitVariants = sc.StockCombos;
            Debug.Log("[SuitedAndBooted]: Loaded "+sc.StockCombos.Count+" stock suit variants");
            for (int i = 0; i < sc.ExtraCombos.Count; i++)
            {
                SuitVariants.Add(sc.ExtraCombos.ElementAt(i));
            }
            Debug.Log("[SuitedAndBooted]: Loaded "+sc.ExtraCombos.Count+" modded suit variants");
            ConfigNode baseNode = GameDatabase.Instance.GetConfigNode("SuitedAndBooted/SUITEDANDBOOTED");
            ConfigNode[] ruleNodes = baseNode.GetNodes("RULE");
            for (int i = 0; i < ruleNodes.Length; i++)
            {
                try
                {
                    SuitRule sr = new SuitRule(ruleNodes.ElementAt(i));
                    if (Rules.ContainsKey(sr.Priority))
                    {
                        Debug.Log("[SuitManager]: WARNING: Multiple rules defined with priority "+sr.Priority+" only one will be applied");
                        Debug.Log("[SuitManager]: Duplicate Values: " + sr.Name + " + " + Rules[sr.Priority].Name);
                    }
                    Rules.Add(sr.Priority, sr);
                }
                catch (Exception ex)
                {
                    Debug.Log("[SuitManager]: " + ex);
                }
            }
            RunRules();
        }

        public void RunRules()
        {
            List<ProtoCrewMember> crewList = HighLogic.CurrentGame.CrewRoster.Kerbals().ToList();
            //Run rules in reverse order - highest priority gets applied last
            for (int i = Rules.Count-1; i >= 0; i--)
            {
                SuitRule r = Rules[i];
                foreach (ProtoCrewMember p in crewList)
                {
                    if (!r.RuleMatch(p)) continue;
                    p.suit = GetCorrectSuitForVariant(r.suitType);
                    p.ComboId = MatchSuitToId(p.gender, r.Suit);
                }
            }
        }

        private ProtoCrewMember.KerbalSuit GetCorrectSuitForVariant(string suitType)
        {
            switch (suitType)
            {
                case "Default":
                    return ProtoCrewMember.KerbalSuit.Default;
                case "Vintage":
                    return ProtoCrewMember.KerbalSuit.Vintage;
                case "Future":
                    return ProtoCrewMember.KerbalSuit.Future;
                default:
                    return ProtoCrewMember.KerbalSuit.Default;
            }
        }

        private string MatchSuitToId(ProtoCrewMember.Gender gender, string suit)
        {
            string genderString = gender == ProtoCrewMember.Gender.Female ? "Female" : "Male";
            for (int i = 0; i < SuitVariants.Count; i++)
            {
                SuitCombo sc = SuitVariants.ElementAt(i);
                if (sc.gender != genderString) continue;
                if (sc.name == suit) return sc.name;
            }
            return null;
        }

        private void OnDisable()
        {
            GameEvents.OnCrewmemberHired.Remove(NewCrew);
            GameEvents.onGUIAstronautComplexSpawn.Remove(AstronautComplexSpawn);
            GameEvents.onGUILaunchScreenSpawn.Remove(LaunchScreenSpawn);
            GameEvents.onAttemptEva.Remove(OnEVA);
        }
    }
}