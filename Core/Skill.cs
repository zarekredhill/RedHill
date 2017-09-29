using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using RedHill.Core.ESI;

namespace RedHill.Core
{
    [DebuggerDisplay("{Name}")]
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

    [DebuggerDisplay("{Name}")]
    public class Skill : IType
    {
        public int Id { get; }
        public string Name { get; }
        public ImmutableList<Requirement> Requirements { get; }

        public Skill(int id, string name, ImmutableList<Requirement> requirements)
        {
            Id = id;
            Name = name;
            Requirements = requirements;
        }

        public class Requirement
        {
            public Skill Skill { get; }
            public int Level { get; }

            public Requirement(Skill skill, int level)
            {
                Skill = skill;
                Level = level;
            }
        }
    }
}