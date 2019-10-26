using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using MetallicBlueDev.EntityGate.Extensions;

namespace MetallicBlueDev.EntityGate.Helpers
{
    /// <summary>
    /// Help with context manipulation.
    /// </summary>
    internal class ContextHelper
    {
        /// <summary>
        /// Determines whether the entity / type combination appears valid.
        /// </summary>
        /// <typeparam name="TContext">The type of context.</typeparam>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="contextType">The type of the context.</param>
        /// <returns></returns>
        internal static bool IsValidContext<TContext>(Type entityType, Type contextType) where TContext : DbContext
        {
            return typeof(TContext).IsAssignableFrom(contextType)
                && entityType.Namespace.Equals(contextType.Namespace, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Returns the meta-data needed by the model.
        /// </summary>
        /// <param name="contextTypeName"></param>
        /// <param name="resourceNames"></param>
        /// <returns></returns>
        internal static string GetMetadata(string contextTypeName, IEnumerable<string> resourceNames)
        {
            var metadata = new StringBuilder();

            foreach (var resourceName in resourceNames.Where(cName => cName.Length > 3 && cName.Contains(contextTypeName)))
            {
                switch (resourceName.Substring(resourceName.Length - 4))
                {
                    case "csdl":
                    case "ssdl":
                    case ".msl":
                    {
                        if (metadata.Length > 0)
                        {
                            metadata.Append("|");
                        }

                        metadata.Append("res://*/");
                        metadata.Append(resourceName);
                        break;
                    }
                }
            }

            return metadata.ToString();
        }

        /// <summary>
        /// Determines whether the entity type matches.
        /// </summary>
        /// <param name="entitySetBase">The entity set.</param>
        /// <param name="entityType">The type of entity requested.</param>
        /// <returns></returns>
        internal static bool MatchEntityType(EntitySetBase entitySetBase, Type entityType)
        {
            return entitySetBase.ElementType != null
                && entitySetBase.ElementType.Name.EqualsIgnoreCase(entityType.Name);
        }
    }
}

