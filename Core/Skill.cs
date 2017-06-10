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

        public Skill(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}