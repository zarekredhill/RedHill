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
        public string Name { get; }
        public string Description { get; }
        public ImmutableDictionary<string, decimal> Attributes { get; }

        public Skill(string name, string description, ImmutableDictionary<string, decimal> attributes)
        {
            Name = name;
            Description = description;
            Attributes = attributes;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}