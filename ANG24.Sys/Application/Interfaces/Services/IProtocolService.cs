//using Application.Types.ServiceTypes;

//namespace Application.Interfaces.Services
//{
//    public interface IProtocolService : IExecuteableCreator
//    {
//        event Action<string> SelectedProtocol;
//        event Action DeselectedProtocol;
//        event Action UpdateListProtocol;
//        event Action<bool> ActivityProtocol;
//        event Action ResetMaxParam;

//        /// <summary>
//        /// Чтение данных о шаблонах протоколов
//        /// </summary>
//        /// <returns>Список шаблонов</returns>
//        IEnumerable<ProtocolModel> ReadDependencyProtocols();
//        /// <summary>
//        /// Инициализация шаблона протокола.
//        /// Больше подходит для ручного протоколирования
//        /// </summary>
//        /// <returns>Успешность</returns>
//        bool? InitProtocol();
//        /// <summary>
//        /// Местоположение значений испытаний в протколе
//        /// </summary>
//        /// <returns></returns>
//        IEnumerable<DocTable> GetDataProtocol();
//        /// <summary>
//        /// Местоположение оборудования в шаблоне
//        /// </summary>
//        /// <returns></returns>
//        DocDevice GetDocDevice();
//        /// <summary>
//        /// Получает список модулей для протокола, в которых было призведено испытание
//        /// </summary>
//        /// <returns></returns>
//        Dictionary<string, bool> GetModulesProtocol();
//        /// <summary>
//        /// Активность протокола
//        /// </summary>
//        bool Activity { get; }
//        /// <summary>
//        /// Сохранение протокола
//        /// </summary>
//        void SaveProtocol();
//        /// <summary>
//        /// Изменение шаблона протокола
//        /// </summary>
//        /// <param name="Module">Имя модуля</param>
//        void EditDependencyProtocol(string Module);
//        /// <summary>
//        /// Изменение активности протокола
//        /// </summary>
//        /// <param name="model"></param>
//        void ChangeActiveProtocol(ProtocolModel model);
//        /// <summary>
//        /// Промежуточное сохранение оборудования
//        /// </summary>
//        /// <param name="row"></param>
//        /// <param name="column"></param>
//        /// <param name="value"></param>
//        void saveParam(int row, int column, string value);
//        /// <summary>
//        /// Промежуточное сохранение данных с ручного протокола
//        /// </summary>
//        /// <param name="cell"></param>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        bool saveParam(string cell, string value);
//        /// <summary>
//        /// Очистка ячейки в протоколе (используется в ручном протоколировании)
//        /// </summary>
//        /// <param name="cell">Имя ячейки</param>
//        void removeParam(string cell);
//        /// <summary>
//        /// Вызывает событие DeselectedProtocol
//        /// </summary>
//        void CloseProtocol();
//        /// <summary>
//        /// Вызывает событие SelectedProtocol
//        /// </summary>
//        void OpenProtocol();
//        /// <summary>
//        /// Сброс значений при проведении испытания HVMAC, HVMDC
//        /// </summary>
//        void ResetMaxParameters();

//        /// <summary>
//        /// Триггер на промежуточное сохранение. Сробатывает, когда ползователь произвел испытание
//        /// </summary>
//        void IntermediateTrigger();
//        /// <summary>
//        /// Изменяет источник данных для энергетического объекта
//        /// </summary>
//        /// <param name="sourceData">Тип источника данных</param>
//        void ChangeSourceDataEnergyObject(string sourceData);
//        /// <summary>
//        /// Выбирает путь для данных энергетического объекта
//        /// </summary>
//        /// <returns>Имя файла</returns>
//        string SelectSourceDataEnergyObject();
//        /// <summary>
//        /// Сохранения источника данных энергетического объекта
//        /// </summary>
//        /// <param name="name">Имя файла</param>
//        void SaveSourceDataEnergyObject(string name);
//        /// <summary>
//        /// Получить данные об источнике данных энергообъекта
//        /// </summary>
//        /// <returns></returns>
//        (string name, string path, string sourceData) GetEnergyObject();
//        /// <summary>
//        /// Получение списка энергообъектов из шаблона протокола
//        /// </summary>
//        /// <returns></returns>
//        Dictionary<int, List<Property>> GetEnergyObjects();
//        /// <summary>
//        /// Обновление названий таблиц, строк и столбцов
//        /// </summary>
//        void updateDataProtocol();
//        /// <summary>
//        /// Возможность выбора энергообъекта
//        /// </summary>
//        /// <returns></returns>
//        bool GetVisibilityEnergyObject();
//    }
//}
