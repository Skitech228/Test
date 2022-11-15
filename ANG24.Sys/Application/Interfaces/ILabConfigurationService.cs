using ANG24.Sys.Application.Types;

namespace ANG24.Sys.Application.Interfaces
{
    public interface ILabConfigurationService
    {
        /// <summary>
        /// Конфигурация лабаратории в виде объекта
        /// </summary>
        LabConfiguration Config { get; }
        /// <summary>
        /// обращение к словарю конфигурации по имени параметра
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        object this[string parameterName] { get; set; }
        /// <summary>
        /// Конфигурация лабаратории в виде словаря
        /// </summary>
        IDictionary<string, object> Configuration { get; }
        /// <summary>
        /// Выполнение сериализации настроек лаборатории
        /// </summary>
        void Save();
        /// <summary>
        /// Получить 
        /// </summary>
        /// <returns>файл конфигурации лабаратории</returns>
        LabConfiguration GetConfig();
        /// <summary>
        /// Сохранить 
        /// </summary>
        /// <param name="config">файл конфигурации лабаратории</param>
        /// <returns>Получилось ли сохранить</returns>
        bool SaveConfig(LabConfiguration config);
    }
}
