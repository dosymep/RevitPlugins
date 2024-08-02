using dosymep.Bim4Everyone.SimpleServices;

using Serilog.Events;

namespace RevitPlatformSettings.ViewModels.Settings {
    internal sealed class TelemetrySettingsViewModel : SettingsViewModel {
        private readonly IPlatformSettingsService _platformSettingsService;

        public TelemetrySettingsViewModel(
            int id, int parentId, string settingsName,
            IPlatformSettingsService platformSettingsService)
            : base(id, parentId, settingsName) {
            _platformSettingsService = platformSettingsService;

            LogTraceIsActive = _platformSettingsService.LogTrace.IsActive;
            LogTraceLogLevel = _platformSettingsService.LogTrace.LogLevel;
            LogTraceServerName = _platformSettingsService.LogTrace.ServerName;

            LogTraceJournalIsActive = _platformSettingsService.LogTraceJournal.IsActive;
            LogTraceJournalUseUtc = _platformSettingsService.LogTraceJournal.UseUtc;
            LogTraceJournalLogLevel = _platformSettingsService.LogTraceJournal.LogLevel;
            LogTraceJournalOutputTemplate = _platformSettingsService.LogTraceJournal.OutputTemplate;
        }
        
        public bool? LogTraceIsActive { get; }
        public LogEventLevel? LogTraceLogLevel { get; }
        public string LogTraceServerName { get; }

        public bool? LogTraceJournalIsActive { get; }
        public bool? LogTraceJournalUseUtc { get; }
        public LogEventLevel? LogTraceJournalLogLevel { get; }
        public string LogTraceJournalOutputTemplate { get; }
    }
}
