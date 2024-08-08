using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using dosymep.Bim4Everyone;

namespace RevitServerFolders.Services {
    /// <summary>
    /// Экспортирует файлы с Revit-server в заданную директорию
    /// </summary>
    internal class RvtExportService : IModelsExportService {
        private const string _revitServerToolPath
            = @"C:\Program Files\Autodesk\Revit {0}\RevitServerToolCommand\RevitServerTool.exe";
        private const string _revitServerToolArgs = @"createLocalRvt ""{0}"" -s ""{1}"" -d ""{2}/"" -o";
        private const string _rvtSearchPattern = "*.rvt";


        public RvtExportService() {
        }


        public void ExportModelObjects(
            string targetFolder,
            string[] modelFiles,
            IProgress<int> progress = null,
            CancellationToken ct = default) {
            if(string.IsNullOrWhiteSpace(targetFolder)) {
                throw new ArgumentException(nameof(targetFolder));
            }
            if(modelFiles is null) {
                throw new ArgumentNullException(nameof(modelFiles));
            }

            Directory.CreateDirectory(targetFolder);

            var revitFiles = Directory.GetFiles(targetFolder, _rvtSearchPattern);
            foreach(var revitFile in revitFiles) {
                File.SetAttributes(revitFile, FileAttributes.Normal);
                File.Delete(revitFile);
            }

            for(int i = 0; i < modelFiles.Length; i++) {
                progress?.Report(i);
                ct.ThrowIfCancellationRequested();

                try {
                    ExportDocument(modelFiles[i], targetFolder);
                    dosymep.Revit.DocumentExtensions.UnloadAllLinks(Directory.GetFiles(targetFolder, _rvtSearchPattern));
                } catch(Exception) {
                    // pass
                }
            }
        }


        private void ExportDocument(string modelName, string targetFolder) {
            // https://help.autodesk.com/view/RVT/2022/ENU/?guid=GUID-77F62A96-8A61-4539-9664-0DD0AB08B6ED
            Uri uri = new Uri(modelName);

            string arguments = string.Format(
                _revitServerToolArgs, uri.LocalPath.Trim('\\').Trim('/'), uri.Host, targetFolder);

            string fileName = string.Format(_revitServerToolPath, ModuleEnvironment.RevitVersion);

            using(Process process = Process.Start(new ProcessStartInfo() {
                Arguments = arguments,
                FileName = fileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            })) {
                process?.WaitForExit();
            }
        }
    }
}
