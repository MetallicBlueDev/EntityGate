using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MetallicBlueDev.EntityGate.Extensions;
using MetallicBlueDev.EntityGate.GateException;
using MetallicBlueDev.EntityGate.Helpers;
using MetallicBlueDev.EntityGate.InterfacedObject;
using MetallicBlueDev.EntityGate.Properties;

namespace MetallicBlueDev.EntityGate.Core
{
    /// <summary>
    /// Dynamic Proxy Entities (POCO Proxy).
    /// 
    /// Entity framework service provider.
    /// Creation of the context related to the database.
    /// </summary>
    /// <typeparam name="TContext">The type of context.</typeparam>
    [Serializable()]
    internal sealed class EntityGateProvider<TContext> : IDisposable where TContext : DbContext
    {
        private const string SqlProvider = "System.Data.SqlClient";

        private readonly IEntityGateObject gate;
        private readonly EntityGateTracking tracking;

        private bool disposed = false;
        private Type currentEntityType = null;
        private bool lazyLoading = false;

        [NonSerialized()]
        private TContext model = null;

        internal EntityGateProvider(IEntityGateObject gate)
        {
            this.gate = gate;
            tracking = new EntityGateTracking();
            lazyLoading = gate.Configuration.LazyLoading;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                FreeMemory();
                disposed = true;
            }
        }

        /// <summary>
        /// Returns the created model.
        /// </summary>
        /// <returns></returns>
        internal TContext GetModel()
        {
            return model;
        }

        /// <summary>
        /// Additional initialization of the service provider.
        /// </summary>
        internal void Initialize()
        {
            if (disposed)
            {
                throw new TransactionCanceledEntityGateException(Resources.ObjectDisposed);
            }

            if (model == null || gate.Configuration.Updated)
            {
                RemoveHandlers();

                // Creation of a new model.
                model = MakeModel();
                InitializeModel();

                AddHandlers();
            }

            if (!gate.Token.IsTracked)
            {
                CleanTracking();
            }
        }

        /// <summary>
        /// Cleaning the tracking.
        /// </summary>
        internal void CleanTracking()
        {
            tracking.CleanTracking();
        }

        /// <summary>
        /// Stop monitoring.
        /// </summary>
        internal void NoTracking()
        {
            CleanTracking();
            gate.Token.IsTracked = false;
        }

        /// <summary>
        /// Returns the state of the entity.
        /// </summary>
        /// <param name="entity">The instance of the entity.</param>
        /// <returns></returns>
        internal EntityState GetEntityState(IEntityObjectIdentifier entity)
        {
            return GetEntityEntry(entity).State;
        }

        /// <summary>
        /// Change the type of entity currently managed.
        /// </summary>
        /// <param name="entityType">The type of the entity (pure or proxy).</param>
        internal void SetCurrentEntityType(Type entityType)
        {
            if (ReflectionHelper.IsValidEntityType(entityType) && currentEntityType != entityType)
            {
                currentEntityType = ObjectContext.GetObjectType(entityType);

                if (gate.Configuration.CanUseLogging)
                {
                    gate.Configuration.Log(string.Format(CultureInfo.InvariantCulture, Resources.CurrentEntityTypeState, currentEntityType));
                }

                if (!currentEntityType.IsSerializable)
                {
                    throw new EntityGateProviderException(string.Format(CultureInfo.InvariantCulture, Resources.EntityTypeIsNotSerializable, currentEntityType.Name));
                }
            }
        }

        /// <summary>
        /// Returns the currently managed entity type (pure / not proxy).
        /// </summary>
        /// <returns></returns>
        internal Type GetCurrentEntityType()
        {
            if (currentEntityType == null)
            {
                // Security on an incoherent call.
                throw new EntityGateProviderException(Resources.EntityTypeUndefined);
            }

            return currentEntityType;
        }

        /// <summary>
        /// Management of POCO entity monitoring.
        /// </summary>
        internal void ManagePocoEntitiesTracking()
        {
            var isProxyCreationEnabled = model.Configuration.ProxyCreationEnabled;
            model.Configuration.ProxyCreationEnabled = false;

            try
            {
                FireManagePocoEntitiesTracking();
            }
            catch (Exception ex)
            {
                throw new EntityGateProviderException(Resources.FailedToTrackPocoEntities, ex);
            }
            finally
            {
                model.Configuration.ProxyCreationEnabled = isProxyCreationEnabled;
            }
        }

        /// <summary>
        /// Returns the main entity since tracking.
        /// </summary>
        /// <returns></returns>
        internal TEntity GetMainEntity<TEntity>() where TEntity : class, IEntityObjectIdentifier
        {
            var mainEntity = tracking.GetMainEntity();
            return (TEntity)mainEntity;
        }

        /// <summary>
        /// Determines whether a specific entity type is currently being managed.
        /// </summary>
        /// <returns></returns>
        internal bool HasCurrentEntityType()
        {
            return currentEntityType != null;
        }

        /// <summary>
        /// Returns the names of the primary key columns.
        /// </summary>
        /// <param name="entity">The instance of the entity.</param>
        /// <returns></returns>
        internal IEnumerable<string> GetPrimaryKeys(IEntityObjectIdentifier entity)
        {
            return model.GetPrimaryKeys(HasCurrentEntityType() ? GetCurrentEntityType() : entity.GetType());
        }

        /// <summary>
        /// Returns the name of the current model.
        /// </summary>
        /// <returns></returns>
        internal string GetModelName()
        {
            return model.GetObjectContext().DefaultContainerName;
        }

        /// <summary>
        /// Change the deferred loading option.
        /// </summary>
        /// <param name="enabled">State of LazyLoading.</param>
        internal void ChangeLazyLoading(bool enabled)
        {
            lazyLoading = enabled;

            if (gate.Configuration.CanUseLogging)
            {
                gate.Configuration.Log(string.Format(CultureInfo.InvariantCulture, Resources.LazyLoadingState, enabled));
            }

            if (enabled && !model.Configuration.ProxyCreationEnabled)
            {
                throw new EntityGateProviderException(Resources.ProxyCreationMustEnabled);
            }

            model.Configuration.LazyLoadingEnabled = enabled;
        }

        /// <summary>
        /// Creating an EntitySet.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        internal DbSet<TEntity> MakeObjectSet<TEntity>() where TEntity : class, IEntityObjectIdentifier
        {
            DbSet<TEntity> currentDbSet;

            if (ReflectionHelper.IsValidEntityType(typeof(TEntity)))
            {
                currentDbSet = model.Set<TEntity>();
            }
            else
            {
                currentDbSet = model.Set(GetCurrentEntityType()).Cast<TEntity>();
            }

            return currentDbSet;
        }

        /// <summary>
        /// Determines whether the entity is known to the context.
        /// </summary>
        /// <param name="entity">The instance of the entity.</param>
        /// <returns></returns>
        internal bool HasEntity(IEntityObjectIdentifier entity)
        {
            ObjectStateEntry argpEntry = null;
            return TryGetEntityStateEntry(entity, ref argpEntry);
        }

        /// <summary>
        /// Determines whether changes have been made to the context.
        /// </summary>
        /// <returns></returns>
        internal bool HasChanges()
        {
            return model.ChangeTracker.HasChanges();
        }

        /// <summary>
        /// Returns the modified and followed entities.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<EntityStateTracking> GetChangedEntries()
        {
            return tracking.GetEntities()
                .Where(entity => entity.State != EntityState.Unchanged && entity.State != EntityState.Detached);
        }

        /// <summary>
        /// Attempt to obtain the entity via the context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyValue">The key to load.</param>
        /// <param name="entity">The instance of the entity.</param>
        /// <returns></returns>
        internal bool TryGetEntity<TEntity>(object keyValue, ref TEntity entity) where TEntity : class, IEntityObjectIdentifier
        {
            entity = ReflectionHelper.IsValidEntityType(typeof(TEntity))
                ? model.Set<TEntity>().Find(keyValue)
                : (TEntity)model.Set(GetCurrentEntityType()).Find(keyValue);

            return entity != null;
        }

        /// <summary>
        /// Returns an entity instance manageable by the current context.
        /// </summary>
        /// <param name="externalEntity">The entity to check.</param>
        /// <param name="contextEntityType">The type of the entity or null.</param>
        /// <returns></returns>
        internal IEntityObjectIdentifier GetManagedOrPocoEntity(IEntityObjectIdentifier externalEntity, Type contextEntityType)
        {
            if (!HasEntity(externalEntity))
            {
                // Search in instances already followed.
                var entityTracked = GetEntityTracked(externalEntity, true);

                if (entityTracked != null)
                {
                    // This is an optimization, instead of cloning stupidly, we reuse the known instance with a merge managed by EF.
                    externalEntity = entityTracked;
                }
                else
                {
                    // Creation of a pure entity if necessary.
                    externalEntity = PocoHelper.GetPocoEntity(externalEntity, contextEntityType, withDataRelation: true);
                }
            }

            return externalEntity;
        }

        /// <summary>
        /// Applies the changes of the entity to the context.
        /// </summary>
        /// <param name="entity">The entity to manage.</param>
        internal void ManageEntity(IEntityObjectIdentifier entity)
        {
            var currentState = GetEntityState(entity);
            var targetState = GetEntityStateTargeted(entity, currentState);
            ManageEntity(entity, currentState, targetState);
        }

        /// <summary>
        /// Applies the changes of the entity to the context.
        /// </summary>
        /// <param name="entity">The entity to manage.</param>
        /// <param name="currentState">The original state (optional).</param>
        /// <param name="targetState">The chosen state.</param>
        internal void ManageEntity(IEntityObjectIdentifier entity, EntityState? currentState, EntityState targetState)
        {
            if (gate.Configuration.CanUseLogging)
            {
                gate.Configuration.Log(string.Format(CultureInfo.InvariantCulture, Resources.ChangeEntityToState, entity, targetState));
            }

            if (targetState == EntityState.Detached)
            {
                throw new EntityGateProviderException(string.Format(CultureInfo.InvariantCulture, Resources.UnexpectedEntityState, targetState, entity));
            }

            // The current state is not needed for an addition.
            if (!currentState.HasValue && targetState != EntityState.Added)
            {
                currentState = GetEntityState(entity);
            }

            // If the entity is detached, check the consistency.
            if (currentState.HasValue && currentState == EntityState.Detached)
            {
                var entityTracked = GetEntityTracked(entity, targetState != EntityState.Deleted);
                entity = entityTracked ?? entity;
            }

            GetEntityEntry(entity).State = targetState;
        }

        /// <summary>
        /// Saves the changes made to the context.
        /// </summary>
        /// <returns></returns>
        internal int SaveChanges()
        {
            DetectChanges();
            DetectLocalMode();

            if (!tracking.HasEntities())
            {
                throw new EntityGateProviderException(Resources.NoTrackedEntity);
            }

            if (gate.Configuration.CanUseLogging)
            {
                gate.Configuration.Log(Resources.SavingChanges);
            }

            return model.SaveChanges();
        }

        /// <summary>
        /// Refreshes the entity via the context.
        /// </summary>
        /// <param name="entity"></param>
        internal void RefreshChanges(IEntityObjectIdentifier entity)
        {
            if (entity == null)
            {
                throw new EntityGateProviderException(Resources.InvalidEntityRefreshChanges);
            }

            model.GetObjectContext().Refresh(RefreshMode.ClientWins, entity);
        }

        /// <summary>
        /// Returns the record of the original values of the entity.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <param name="allProperties">Return all properties or just the modified ones.</param>
        /// <returns></returns>
        internal KeyValuePair<string, object>[] GetOriginalValues(IEntityObjectIdentifier entity, bool allProperties)
        {
            var values = new List<KeyValuePair<string, object>>();
            var stateEntry = GetEntityStateEntry(entity);
            var modifiedProperties = !allProperties ? stateEntry.GetModifiedProperties().ToArray() : null;
            var originalValues = stateEntry.OriginalValues;

            for (int i = 0, loopTo = originalValues.FieldCount - 1; i <= loopTo; i++)
            {
                var fieldName = originalValues.GetName(i);

                if (modifiedProperties != null && !modifiedProperties.Any(pName => (pName ?? "") == (fieldName ?? "")))
                {
                    continue;
                }

                values.Add(new KeyValuePair<string, object>(originalValues.GetName(i), originalValues.GetValue(i)));
            }

            return values.ToArray();
        }

        /// <summary>
        /// Procedure for cleaning up used resources.
        /// </summary>
        private void FreeMemory()
        {
            RemoveHandlers();
            CleanTracking();

            if (model != null)
            {
                model.Dispose();
                model = null;
            }

            currentEntityType = null;

            gate.Configuration.ConfigurationUpdated();
        }

        /// <summary>
        /// Check if the connection configuration needs to be synchronized.
        /// </summary>
        private void CheckConnectionConfiguration()
        {
            if (gate.Configuration.Updated)
            {
                gate.Configuration.Update(model.Database.Connection);
            }
        }

        /// <summary>
        /// Returns the state to apply to the entity.
        /// </summary>
        /// <param name="entity">The entity to manage.</param>
        /// <param name="currentState">The original state.</param>
        /// <returns></returns>
        private EntityState GetEntityStateTargeted(IEntityObjectIdentifier entity, EntityState currentState)
        {
            var targetState = currentState;
            var currentKey = GetEntityKey(entity);

            // Request the addition if necessary.
            if (targetState != EntityState.Added || !HasEntityKey(currentKey))
            {
                // Protection against inconsistent states.
                if (!entity.HasValidEntityKey())
                {
                    targetState = EntityState.Added;
                }
            }

            // Ask for attachment if necessary.
            if (targetState == EntityState.Detached)
            {
                targetState = EntityState.Modified;
            }

            return targetState;
        }

        /// <summary>
        /// POCO entity tracking audit routine.
        /// </summary>
        private void FireManagePocoEntitiesTracking()
        {
            if (gate.Token.IsTracked)
            {
                // Follow-up on all entities.
                TrackPocoEntities();
            }
            else
            {
                // Follow-up only of the main entity.
                TrackMainPocoEntity();
            }
        }

        /// <summary>
        /// Request tracking of all (modified) entities.
        /// </summary>
        private void TrackPocoEntities()
        {
            var mainEntity = gate.CurrentEntityObject;

            foreach (var entry in GetEntriesTracked().Where(entity => entity.State != EntityState.Unchanged || entity.Entity == mainEntity))
            {
                TrackPocoEntity(entry, entry.Entity == mainEntity);
            }
        }

        /// <summary>
        /// Request tracking of the main entity.
        /// </summary>
        private void TrackMainPocoEntity()
        {
            var entry = GetEntityEntry(gate.CurrentEntityObject);
            TrackPocoEntity(entry, true);
        }

        /// <summary>
        /// Request tracking of an entity.
        /// </summary>
        /// <param name="entry">Tracking information about the entity.</param>
        /// <param name="isMainEntry">Determine if this is the main entity.</param>
        private void TrackPocoEntity(DbEntityEntry entry, bool isMainEntry)
        {
            var pocoEntity = entry.State == EntityState.Deleted ? entry.OriginalValues.ToObject() : entry.CurrentValues.ToObject();
            tracking.MarkEntity(pocoEntity, entry.State, isMainEntry);
        }

        /// <summary>
        /// Returns the entities followed in the context.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<DbEntityEntry> GetEntriesTracked()
        {
            return model.ChangeTracker.Entries();
        }

        /// <summary>
        /// Force detection of changes (only if necessary, default is not the case).
        /// </summary>
        private void DetectChanges()
        {
            if (!model.Configuration.AutoDetectChangesEnabled)
            {
                if (gate.Configuration.CanUseLogging)
                {
                    gate.Configuration.Log(Resources.DetectionOfChanges);
                }

                model.ChangeTracker.DetectChanges();
            }
        }

        /// <summary>
        /// Detects local mode (use without serialization).
        /// </summary>
        private void DetectLocalMode()
        {
            if (!tracking.HasEntities())
            {
                // Starts tracking management.
                ManagePocoEntitiesTracking();
            }
        }

        /// <summary>
        /// Returns the information about the state of the entity.
        /// </summary>
        /// <param name="entity">The entity to manage.</param>
        /// <returns></returns>
        private ObjectStateEntry GetEntityStateEntry(IEntityObjectIdentifier entity)
        {
            ObjectStateEntry result = null;

            if (!TryGetEntityStateEntry(entity, ref result) || result == null)
            {
                if (gate.Configuration.CanUseLogging)
                {
                    // It's not necessarily a problem.
                    // This means that the object is currently not followed in the context.
                    gate.Configuration.Log(string.Format(CultureInfo.InvariantCulture, Resources.ObjectStateEntryNotFound, entity));
                }
            }

            return result;
        }

        /// <summary>
        /// Attempt to find information about the state of the entity.
        /// </summary>
        /// <param name="entity">The entity to manage.</param>
        /// <param name="entry">Information about the state of the entity</param>
        /// <returns></returns>
        private bool TryGetEntityStateEntry(IEntityObjectIdentifier entity, ref ObjectStateEntry entry)
        {
            return entity != null
                && model.GetObjectContext()
                .ObjectStateManager
                .TryGetObjectStateEntry(entity, out entry);
        }

        /// <summary>
        /// Returns the instance of the entity since the trace, with update if requested.
        /// </summary>
        /// <param name="entity">The entity to manage.</param>
        /// <param name="updateValues">Update the tracked entity with the values of the entity to be managed.</param>
        /// <returns></returns>
        private IEntityObjectIdentifier GetEntityTracked(IEntityObjectIdentifier entity, bool updateValues)
        {
            IEntityObjectIdentifier trackedEntity = null;
            var currentEntry = GetEntityEntryTracked(entity);

            if (currentEntry != null)
            {
                if (updateValues)
                {
                    currentEntry.CurrentValues.SetValues(entity);
                }

                trackedEntity = (IEntityObjectIdentifier)currentEntry.Entity;
            }

            return trackedEntity;
        }

        /// <summary>
        /// Returns the entity information from the trace.
        /// </summary>
        /// <param name="entity">The entity to manage.</param>
        /// <returns></returns>
        private DbEntityEntry GetEntityEntryTracked(IEntityObjectIdentifier entity)
        {
            var sourceType = entity.GetType();
            return GetEntriesTracked()
                .FirstOrDefault(entry => entry.Entity.GetType() == sourceType
                && ((IEntityObjectIdentifier)entry.Entity).Identifier.Equals(entity.Identifier));
        }

        /// <summary>
        /// Returns the tracking information about the entity.
        /// </summary>
        /// <param name="entity">The entity to manage.</param>
        /// <returns></returns>
        private DbEntityEntry GetEntityEntry(object entity)
        {
            if (entity == null)
            {
                throw new EntityGateProviderException(Resources.InvalidEntity);
            }

            var entityEntry = model.Entry(entity);

            if (entityEntry == null)
            {
                throw new EntityGateProviderException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidEntityEntryFor, entity));
            }

            return entityEntry;
        }

        /// <summary>
        /// Returns the key of the entity.
        /// </summary>
        /// <param name="entity">The entity to manage.</param>
        /// <returns></returns>
        private EntityKey GetEntityKey(IEntityObjectIdentifier entity)
        {
            EntityKey result = null;
            var ose = GetEntityStateEntry(entity);

            if (ose != null)
            {
                result = ose.EntityKey;
            }

            return result;
        }

        /// <summary>
        /// Determines whether the key is known to the context.
        /// </summary>
        /// <param name="entityKey">An identifier for an entity.</param>
        /// <returns></returns>
        private bool HasEntityKey(EntityKey entityKey)
        {
            return entityKey != null
                && model.GetObjectContext()
                .ObjectStateManager
                .TryGetObjectStateEntry(entityKey, out var argentry);
        }

        /// <summary>
        /// Initialization of the model.
        /// </summary>
        private void InitializeModel()
        {
            if (model == null)
            {
                throw new EntityGateProviderException(Resources.InvalidContext);
            }

            if (gate.Configuration.CanUseLogging)
            {
                gate.Configuration.Log(Resources.InitializingModel);
            }

            InitializeModelLog();
            InitializeModelConfiguration();
            InitializeModelWorkspace();

            ApplyTrackingIfNeeded();
        }

        /// <summary>
        /// Applies entity tracking on the model as needed.
        /// </summary>
        private void ApplyTrackingIfNeeded()
        {
            if (tracking.HasEntities() && gate.Token.IsTracked)
            {
                if (gate.Configuration.CanUseLogging)
                {
                    gate.Configuration.Log(Resources.ApplyEntityTracking);
                }

                try
                {
                    gate.Token.IsTracked = false;
                    tracking.UnloadEmptyEntityCollection();
                    ApplyTracking();
                }
                finally
                {
                    gate.Token.IsTracked = true;
                }
            }
        }

        /// <summary>
        /// Applies entity tracking on the model.
        /// </summary>
        private void ApplyTracking()
        {
            foreach (var stateTracking in tracking.GetEntities())
            {
                ManageEntity(stateTracking.EntityObject, null, stateTracking.State);
            }
        }

        /// <summary>
        /// Initialization of the log command.
        /// </summary>
        private void InitializeModelLog()
        {
            model.Database.Log = TraceEntityLog;
        }

        /// <summary>
        /// Trace commands Entity Framework.
        /// </summary>
        /// <param name="message">SQL message.</param>
        private void TraceEntityLog(string message)
        {
            if (gate.Configuration.CanUseLogging)
            {
                gate.Configuration.Log(message);
            }

            if (message.IsNotNullOrEmpty(4))
            {
                if (message.IsMatch("SELECT|UPDATE|DELETE|INSERT"))
                {
                    gate.Token.SqlStatement = message;
                }
            }
        }

        /// <summary>
        /// Initialization of the model configuration.
        /// </summary>
        private void InitializeModelConfiguration()
        {
            model.Configuration.ProxyCreationEnabled = true;

            ChangeLazyLoading(lazyLoading);
            CheckConnectionConfiguration();
        }

        /// <summary>
        /// Initialization of the Meta-data of the context.
        /// </summary>
        private void InitializeModelWorkspace()
        {
            try
            {
                model.GetObjectContext().MetadataWorkspace.LoadFromAssembly(model.GetType().Assembly);
            }
            catch (Exception ex)
            {
                throw new EntityGateProviderException(string.Format(CultureInfo.InvariantCulture, Resources.UnableToLoadMetadata, GetModelName()), ex);
            }
        }

        /// <summary>
        /// Listening to the events of the model.
        /// </summary>
        private void AddHandlers()
        {
            if (model != null)
            {
                var context = model.GetObjectContext();
                context.SavingChanges += Model_SavingChanges;
            }
        }

        /// <summary>
        /// Stop listening to model events
        /// </summary>
        private void RemoveHandlers()
        {
            if (model != null)
            {
                var context = model.GetObjectContext();
                context.SavingChanges -= Model_SavingChanges;
            }
        }

        /// <summary>
        /// Returns a new model.
        /// </summary>
        /// <returns></returns>
        private TContext MakeModel()
        {
            TContext rslt;

            if (gate.Configuration.CanUseLogging)
            {
                gate.Configuration.Log(Resources.MakingNewModel);
            }

            rslt = HasCurrentEntityType() ? NewContextByEntityType() : NewContextByInstance(typeof(TContext));

            return rslt;
        }

        /// <summary>
        /// Returns the configured instance of the context according to the entity type.
        /// </summary>
        /// <returns></returns>
        private TContext NewContextByEntityType()
        {
            TContext context = null;
            var currentEntityType = GetCurrentEntityType();

            // Search for a context that seems usable with the entity.
            foreach (var newContextType in currentEntityType.Assembly.GetTypes().Where(contextType => ContextHelper.IsValidContext<TContext>(currentEntityType, contextType)))
            {
                // Attempt to create the context.
                context = NewContextByInstance(newContextType);

                if (context.IsRelativeToEntityType(currentEntityType))
                {
                    break;
                }
            }

            if (context == null)
            {
                throw new EntityGateProviderException(string.Format(CultureInfo.InvariantCulture, Resources.FailedToCreateContextWithEntityType, currentEntityType.Name));
            }

            return context;
        }

        /// <summary>
        /// Returns the configured instance of the context according to the entity type and the connection string.
        /// </summary>
        /// <param name="contextType">The type of the context.</param>
        /// <returns></returns>
        private TContext NewContextByInstance(Type contextType)
        {
            TContext context = null;

            foreach (var currentConstructorInfo in contextType.GetConstructors())
            {
                context = NewContextByInstance(contextType, currentConstructorInfo);

                if (context != null)
                {
                    break;
                }
            }

            if (context == null)
            {
                throw new EntityGateProviderException(string.Format(CultureInfo.InvariantCulture, Resources.FailedToCreateContextWithType, contextType.Name));
            }

            return context;
        }

        /// <summary>
        /// Returns the configured instance of the context according to the entity type and the connection string.
        /// </summary>
        /// <param name="contextType">The type of the context.</param>
        /// <param name="constructorInfo">The constructor of the type.</param>
        /// <returns></returns>
        private TContext NewContextByInstance(Type contextType, ConstructorInfo constructorInfo)
        {
            TContext context = null;
            var parameterInfos = constructorInfo.GetParameters();

            if (parameterInfos.Length == 1)
            {
                var currentParameterInfo = (ParameterInfo)parameterInfos.GetValue(0);

                if (currentParameterInfo.ParameterType == typeof(string))
                {
                    context = (TContext)constructorInfo.Invoke(new object[] { CreateEntityConnectionString(contextType) });
                }
            }

            return context;
        }

        /// <summary>
        /// Creation of the ConnectionString for EF.
        /// </summary>
        /// <param name="contextType">The type of the context.</param>
        /// <returns></returns>
        private string CreateEntityConnectionString(Type contextType)
        {
            var entityBuilder = new EntityConnectionStringBuilder
            {
                ProviderConnectionString = gate.Configuration.CreateBuilder().ToString(),
                Provider = SqlProvider,
                Metadata = ContextHelper.GetMetadata(contextType.Name, contextType.Assembly.GetManifestResourceNames())
            };

            return entityBuilder.ToString();
        }

        private void Model_SavingChanges(object sender, EventArgs e)
        {
            if (!gate.Token.IsTracked)
            {
                // Model not managed by EntityGate on which a developer tries to write data.
                throw new TransactionCanceledEntityGateException(Resources.SaveWithEntityGate);
            }
        }
    }
}

