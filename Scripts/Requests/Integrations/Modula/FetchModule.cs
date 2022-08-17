#if MODULA
using System;
using Modula;
using Modula.Common;
using Modula.Optimization;

namespace RequestForMirror.Integrations.Modula
{
    public abstract class FetchModule<TResponse> : Fetch<TResponse>, IModule
    {
        private ModuleDefaultImplementation _defaultImplementation;
        // ReSharper disable once MemberCanBePrivate.Global
        protected ModuleDefaultImplementation DefaultImplementation
        {
            get { return _defaultImplementation ??= new ModuleDefaultImplementation(this); }
        }

        public virtual void Update()
        {
            DefaultImplementation.Update();
        }

        public TimingConstraints UpdateInvocationConstraints => DefaultImplementation.UpdateConstraints;
        public virtual TypedList<IModule> RequiredOtherModules { get; } = new TypedList<IModule>();

        public ModularBehaviour Main => DefaultImplementation.Main;

        public void OnAdd()
        {
            DefaultImplementation.OnAdd();
        }

        public void AddModule(Type moduleType)
        {
            DefaultImplementation.AddModule(moduleType);
        }

        public string GetName()
        {
            return DefaultImplementation.GetName();
        }

        public DataLayer GetData()
        {
            return DefaultImplementation.GetData();
        }

        public void ManagedUpdate()
        {
            ModuleUpdate();
        }

        public virtual bool ShouldSerialize()
        {
            return true;
        }

        protected virtual void ModuleUpdate()
        {
        }
    }
}
#endif