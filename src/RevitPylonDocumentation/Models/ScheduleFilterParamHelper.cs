namespace RevitPylonDocumentation.Models {
    internal class ScheduleFilterParamHelper {
        public ScheduleFilterParamHelper(string paramNameInSchedule, string paramNameInHost) {
            ParamNameInSchedule = paramNameInSchedule;
            ParamNameInHost = paramNameInHost;
        }

        public bool IsCheck { get; set; } = false;
        public string ParamNameInSchedule { get; set; }
        public string ParamNameInHost { get; set; }
    }
}
