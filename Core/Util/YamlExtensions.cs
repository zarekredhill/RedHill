using System;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace RedHill.Core.Util
{
    public static class YamlExtensions
    {
        public static T GetScalar<T>(this YamlNode node, params string[] path)
        {
            T result;
            if (TryGetScalar(node, out result, path)) return result;
            throw new InvalidOperationException(string.Format("Unable to find path {0} in node {1}", string.Join(",", path), node));
        }
        
        public static bool TryGetScalar<T>(this YamlNode node, out T result, params string[] path)
        {
            var currentNode = node;
            result = default(T);
            for(var i = 0; i < path.Length; i++)
            {
                currentNode = ((YamlMappingNode) currentNode).Children.SingleOrDefault(a => string.Equals(path[i], ((YamlScalarNode)a.Key).Value, StringComparison.OrdinalIgnoreCase)).Value;
                if (null == currentNode) return false;
            }
             var s = ((YamlScalarNode)currentNode).Value;
             result = (T) Convert.ChangeType(s, typeof(T));
             return true;
        }

        public static T Get<T>(this YamlNode node, params string[] path)
            where T : YamlNode
        {
            T result;
            if (TryGet(node, out result, path)) return result;
            throw new InvalidOperationException(string.Format("Unable to find path {0} in node {1}", string.Join(",", path), node));
        }

        public static bool TryGet<T>(this YamlNode node, out T result, params string[] path)
            where T : YamlNode
        {
            var currentNode = node;
            result = default(T);
            for(var i = 0; i < path.Length; i++)
            {
                currentNode = ((YamlMappingNode) currentNode).Children.SingleOrDefault(a => string.Equals(path[i], ((YamlScalarNode)a.Key).Value, StringComparison.OrdinalIgnoreCase)).Value;
                if (null == currentNode) return false;
            }
             result = (T) Convert.ChangeType(currentNode, typeof(T));
             return true;
        }
    }
}