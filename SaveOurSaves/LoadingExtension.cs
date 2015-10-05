using System.Reflection;
using ColossalFramework.Math;
using ColossalFramework.Steamworks;
using ICities;

namespace SaveOurSaves
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            LoadingProfilerDetour.Deploy();
            BuildingManagerDetour.Deploy();
            LoadingProfilerDetour.fixesApplied = false;
            LoadingProfilerDetour.counter = 0;

        }

        public override void OnReleased()
        {
            base.OnReleased();
            LoadingProfilerDetour.Revert();
            BuildingManagerDetour.Revert();
            LoadingProfilerDetour.fixesApplied = false;
            LoadingProfilerDetour.counter = 0;
        }
    }
}