using System.Collections.Generic;
using System.Reflection;
using ICities;
using SaveOurSaves.Detours;
using SaveOurSaves.Redirection;

namespace SaveOurSaves
{
    public class LoadingExtension : LoadingExtensionBase
    {

        private static Dictionary<MethodInfo, RedirectCallsState> _redirects;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            _redirects = RedirectionUtil.RedirectAssembly();
            LoadingProfilerDetour.Initialize();

        }

        public override void OnReleased()
        {
            base.OnReleased();
            LoadingProfilerDetour.Revert();
            RedirectionUtil.RevertRedirects(_redirects);
        }
    }
}