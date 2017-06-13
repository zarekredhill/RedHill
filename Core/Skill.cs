using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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
        public int TypeId { get; }
        public string Name { get; }
        public string Description { get; }
        public ImmutableDictionary<Skill, int> Requirements { get; }

        public Skill(int typeId, string name, string description, ImmutableDictionary<Skill, int> requirements)
        {
            TypeId = typeId;
            Name = name;
            Description = description;
            Requirements = requirements;
        }

        public override string ToString()
        {
            return Name;
        }


    }
}