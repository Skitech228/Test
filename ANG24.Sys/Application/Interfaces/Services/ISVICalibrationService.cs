using ANG24.Sys.Application.Types.ServiceTypes;

namespace ANG24.Sys.Application.Interfaces.Services;
public interface ISVICalibrationService : IExecuteableCreator
{
    /// <summary>
    /// Срабатывает при изменении коллекции точек в текущем модуле
    /// </summary>
    event Action<IEnumerable<Point>> OnPointsChanged;
    /// <summary>
    /// Получить список точек
    /// </summary>
    /// <returns>список точек текущего модуля</returns>
    IEnumerable<Point> GetPoints();
    /// <summary>
    /// Получить дату текущей рабочей калибровки
    /// </summary>
    /// <returns></returns>
    string GetCalibDate();
    /// <summary>
    /// Получить серийный номер SVI
    /// </summary>
    /// <returns></returns>
    int GetSerial();
    /// <summary>
    /// Получить текущую нулевую точку
    /// </summary>
    /// <returns></returns>
    int GetZeroPoint();
    /// <summary>
    /// Получить число точек для обоих модулей
    /// </summary>
    /// <returns></returns>
    (int DC, int AC) GetPointsCount();

    /// <summary>
    /// Запросить выполнения алгоритма получения точек с СВИ
    /// Работает только для версии 3+
    /// </summary>
    void GetRealPoints();
    /// <summary>
    /// Добавить точку
    /// </summary>
    /// <param name="x">её теоретическое положение в списке точек на контроллере</param>
    /// <param name="voltage">Нарпяжение в этом месте</param>
    void AddPoint(int x, int voltage);
    /// <summary>
    /// Изменение точки
    /// </summary>
    /// <param name="index">её теоретическое положение в списке точек на контроллере</param>
    /// <param name="voltage">Нарпяжение в этом месте</param>
    void EditPoint(int index, int voltage);
    /// <summary>
    /// Удалить точку в текущем модуле
    /// </summary>
    /// <param name="index">индекс точки</param>
    void DeletePoint(int index);
    /// <summary>
    /// Очистить список точек в обоих модулях
    /// </summary>
    void DeleteAllPoints();
    /// <summary>
    /// Задать серийный номер SVI
    /// </summary>
    /// <param name="serial">серийный номер</param>
    void SetSerial(int serial);
    /// <summary>
    /// Задать нулевую точку
    /// </summary>
    /// <param name="pos">нулевая точка</param>
    void SetZeroPoint(int pos);
}