using System;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedHill.Core.ESI
{
    public class SkillsProvider
    {
        private RequestHandler RequestHandler { get; }
        private CategoriesProvider CategoriesProvider { get; }

        public SkillsProvider(CategoriesProvider categoriesProvider, RequestHandler requestHandler)
        {
            CategoriesProvider = categoriesProvider;
            RequestHandler = requestHandler;
        }

        public async Task<ImmutableList<Skill>> GetSkills()
        {
            var categories = await CategoriesProvider.Get();
            var skillsCategory = categories.Single(a => string.Equals("Skill", a.Name, StringComparison.OrdinalIgnoreCase));

            return skillsCategory.GroupIds.Select(a => new Skill(a.ToString(), a.ToString())).ToImmutableList();

        }

    }

}