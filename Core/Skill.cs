using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using RedHill.Core.ESI;

namespace RedHill.Core
{
    public class SkillGroup
    {
        public string Name { get; }
        public ImmutableList<Skill> Skills { get; }

        public SkillGroup(string name, ImmutableList<Skill> skills)
        {
            Name = name;
            Skills = skills;
        }
    }

    public class Skill
    {
        public TypeInfo Type { get; }
        public ImmutableDictionary<Skill, int> Requirements { get; }

        public Skill(TypeInfo type, ImmutableDictionary<Skill, int> requirements)
        {
            Type = type;
            Requirements = requirements;
        }
    }
}