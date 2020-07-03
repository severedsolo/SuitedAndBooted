using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Localization;
using ProceduralFairings;
using UnityEngine;

namespace SuitedAndBooted
{
    public class SuitRule
    {
        public int Priority;
        public bool Lv1 = true;
        public bool Lv2 = true;
        public bool Lv3 = true;
        public bool Lv4 = true;
        public bool Lv5 = true;
        public bool Males = true;
        public bool Females = true;
        public bool Veterans = true;
        public bool NonVeterans = true;
        public List<string> AllowedClasses = new List<string>();
        public string Suit;
        public string suitType;
        public string Name;
        
        public SuitRule(ConfigNode ruleNode)
        {
            Name = ruleNode.GetValue("Name");
            string[] loadedValues;
            if (ruleNode.HasValue("Level"))
            {
                loadedValues = ruleNode.GetValues("Level");
                AssignLevels(loadedValues);
            }
            if(!ruleNode.HasValue("Gender")) throw new InvalidOperationException("Rule "+Name+" is missing Value \"Gender\"");
            loadedValues = ruleNode.GetValues("Gender");
            AssignGenders(loadedValues);
            if(ruleNode.HasValue("Class")) AllowedClasses = ruleNode.GetValues("Class").ToList();
            else AssignAllClasses();
            if (ruleNode.HasValue("AllowVeterans")) bool.TryParse(ruleNode.GetValue("AllowVeterans"), out Veterans);
            if (ruleNode.HasValue("AllowNonVeterans")) bool.TryParse(ruleNode.GetValue("AllowNonVeterans"), out NonVeterans);
            if (ruleNode.HasValue("Priority")) int.TryParse(ruleNode.GetValue("Priority"), out Priority);
            else Priority = SuitedAndBooted.Instance.Rules.Count;
            Suit = ruleNode.GetValue("Suit");
            VerifySuitExists(Suit);
            Debug.Log("[SuitManager]: Created Rule "+Name);
        }

        private void AssignAllClasses()
        {
            ConfigNode[] traitNodes = GameDatabase.Instance.GetConfigNodes("EXPERIENCE_TRAIT");
            for (int i = 0; i < traitNodes.Length; i++)
            {
                ConfigNode cn = traitNodes.ElementAt(i);
                AllowedClasses.Add(cn.GetValue("name"));
            }
        }

        private void VerifySuitExists(string suit)
        {
            for (int i = 0; i < SuitedAndBooted.Instance.SuitVariants.Count; i++)
            {
                SuitCombo s = SuitedAndBooted.Instance.SuitVariants.ElementAt(i);
                suitType = s.suitType;
                if (s.name == suit) return;
            }
            throw new InvalidOperationException("[SuitManager]: Suit "+suit+" doesn't exist");
        }

        private void AssignGenders(string[] loadedValues)
        {
            if (!loadedValues.Contains("Male")) Males = false;
            if (!loadedValues.Contains("Female")) Females = false;
        }

        private void AssignLevels(string[] loadedValues)
        {
            if (!loadedValues.Contains("1")) Lv1 = false;
            if (!loadedValues.Contains("2")) Lv2 = false;
            if (!loadedValues.Contains("3")) Lv3 = false;
            if (!loadedValues.Contains("4")) Lv4 = false;
            if (!loadedValues.Contains("5")) Lv5 = false;
        }

        public bool RuleMatch(ProtoCrewMember p)
        {
            if (!LevelAllowed(p.experienceLevel)) return false;
            if (!GenderAllowed(p.gender)) return false;
            if(!AllowedClasses.Contains(p.trait)) return false;
            if (!VeteranAllowed(p.veteran)) return false;
            return true;
        }

        private bool VeteranAllowed(bool veteran)
        {
            return veteran ? Veterans : NonVeterans;
        }

        private bool GenderAllowed(ProtoCrewMember.Gender gender)
        {
            return gender == ProtoCrewMember.Gender.Female ? Females : Males;
        }

        private bool LevelAllowed(int level)
        {
            switch (level)
            {
                case 1:
                    return Lv1;
                case 2:
                    return Lv2;
                case 3:
                    return Lv3;
                case 4:
                    return Lv4;
                case 5: 
                    return Lv5;
                default:
                    return true;
            }
        }
    }
}