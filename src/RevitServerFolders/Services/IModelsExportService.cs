using System;
using System.Threading;

namespace RevitServerFolders.Services {
    /// <summary>
    /// Сервис для экспорта файлов
    /// </summary>
    internal interface IModelsExportService {
        /// <summary>
        /// Экспортирует заданные файлы в заданную директорию
        /// </summary>
        /// <param name="targetFolder">Абсолютный путь к директории, в которую нужно экспортировать файлы</param>
        /// <param name="modelFiles">Массив абсолютных путей к файлам, которые нужно экспортировать</param>
        /// <param name="progress">Прогресс для уведомления о ходе выполнения операции</param>
        /// <param name="ct">Токен отмены</param>
        void ExportModelObjects(
            string targetFolder,
            string[] modelFiles,
            IProgress<int> progress = null,
            CancellationToken ct = default);
    }
}
