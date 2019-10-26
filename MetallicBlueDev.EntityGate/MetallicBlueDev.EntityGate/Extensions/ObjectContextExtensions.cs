using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using MetallicBlueDev.EntityGate.GateException;
using MetallicBlueDev.EntityGate.Helpers;
using MetallicBlueDev.EntityGate.Properties;

namespace MetallicBlueDev.EntityGate.Extensions
{
    /// <summary>
    /// Help with the manipulation of <see cref="IObjectContextAdapter"/>.
    /// </summary>
    public static class ObjectContextExtensions
    {
        /// <summary>
        /// Returns the context information.
        /// </summary>
        /// <param name="context">The instance of the context.</param>
        /// <returns></returns>
        public static ObjectContext GetObjectContext(this IObjectContextAdapter context)
        {
            if (context == null || context.ObjectContext == null)
            {
                throw new EntityGateProviderException(Resources.ContextNotAvailable);
            }

            return context.ObjectContext;
        }

        /// <summary>
        /// Returns the entity container.
        /// </summary>
        /// <param name="model">The instance of the context.</param>
        /// <returns></returns>
        internal static EntityContainer GetEntityContainer(this IObjectContextAdapter model)
        {
            var sourceContext = model.GetObjectContext();
            var entityContainer = sourceContext.MetadataWorkspace.GetEntityContainer(sourceContext.DefaultContainerName, DataSpace.CSpace);

            if (entityContainer == null)
            {
                throw new EntityGateProviderException(Resources.EntityContainerNotAvailable);
            }

            return entityContainer;
        }

        /// <summary>
        /// Returns the names of the primary key columns.
        /// </summary>
        /// <param name="context">The instance of the context.</param>
        /// <param name="entityType">The type of the entity.</param>
        /// <returns></returns>
        internal static IEnumerable<string> GetPrimaryKeys(this IObjectContextAdapter context, Type entityType)
        {
            var entityContainer = context.GetEntityContainer();
            var entitySet = entityContainer.GetEntitySetByName(entityType.Name, false);

            if (entitySet == null)
            {
                throw new EntityGateProviderException(string.Format(CultureInfo.InvariantCulture, Resources.EntitySetNotAvailable, entityType.Name));
            }

            return entitySet.ElementType.KeyMembers.Select(k => k.Name);
        }

        /// <summary>
        /// Determines whether the context is the one that references the requested entity type.
        /// </summary>
        /// <typeparam name="TContext">The type of context.</typeparam>
        /// <param name="context">The context to check.</param>
        /// <param name="entityType">The type of entity requested.</param>
        /// <returns></returns>
        internal static bool IsRelativeToEntityType(this IObjectContextAdapter context, Type entityType)
        {
            var isValidContext = false;

            if (context != null)
            {
                // We will check the compatibility of the context with the entity.
                var entityContainer = context.GetEntityContainer();

                if (entityContainer.BaseEntitySets != null)
                {
                    // The entity is present in the context.
                    if (entityContainer.BaseEntitySets.Any(entitySetBase => ContextHelper.MatchEntityType(entitySetBase, entityType)))
                    {
                        isValidContext = true;
                    }
                }
            }

            return isValidContext;
        }
    }
}

