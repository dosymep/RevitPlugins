using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitPlatformSettings.ViewModels;
using RevitPlatformSettings.ViewModels.Settings;

namespace RevitPlatformSettings.Factories {
    internal class SettingsViewModelFactory : ISettingsViewModelFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public SettingsViewModelFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public T Create<T>(int id, int parentId, string settingsName) where T : SettingsViewModel {
            return _resolutionRoot.Get<T>(
                new ConstructorArgument(nameof(id), id),
                new ConstructorArgument(nameof(parentId), parentId),
                new ConstructorArgument(nameof(settingsName), settingsName)
            );
        }
    }
}