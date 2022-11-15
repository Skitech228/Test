using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect;
using ANG24.Sys.Infrastructure.Helpers;

namespace ANG24.Sys.Communication.Helpers
{
    public sealed class ReflectDataPrepare
    {
        private int length;
        public int XOffset { get; set; } = 0;
        public Channel CurrentChannel { get; set; }
        public Channel ComparedChannel1 { get; set; }
        public Channel ComparedChannel2 { get; set; }
        public bool IsCompare { get; set; }
        public IList<Channel> Channels { get; set; }
        public byte Mode2Stage { get; set; }
        public int Length
        {
            get => length;
            set => length = value < 50_000 ? value : 50_000;
            // Почему 50_000? Потому что иди ..., вот почему
            // возможно надо будет убрать
        }
        public int LenMas { get; set; }
        private bool isSmootherEnabled = false;
        // при желании можно изменить число отправляемых точек тут
        // не изменять в ходе работы программы
        private const ushort pointsCount = 1500;
        private readonly ushort[,] channelsData = new ushort[7, pointsCount]; // последние значения с каналов, подготовленные к выводу
        private readonly Filter<int> smoother = Filter<int>.NewFilter(5);

        /// <summary>
        /// Собрать данные всех текущих каналов
        /// </summary>
        /// <param name="array">массив текущего канала</param>
        /// <param name="isDataUpdated">Проверочная переменная, получилось ли заполнить массив</param>
        /// <returns></returns>
        public ChartPoint[] GetChannelsData(ushort[] array, out bool isDataUpdated)
        {
            if (CurrentChannel != Channel.IDMChannel)
                isDataUpdated = true;
            else
                isDataUpdated = false;
            var mode = Mode2Stage;
            // уменьшение числа значений, фильтрация и прочее
            ReduceData(ref array, length, pointsCount);

            // включить сглаживающий фильтр
            if (isSmootherEnabled) Smoother(ref array);
            //заполнение хранилища данных
            if (CurrentChannel != 0 && array[5] != 0)
            {
                Parallel.For(0, pointsCount,
                   (i) =>
                   {
                       switch (CurrentChannel)
                       {
                           case Channel.Channel1:
                           case Channel.Channel2:
                           case Channel.Channel3:
                               channelsData[(byte)CurrentChannel - 1, i] = array[i];
                               break;
                           case Channel.IDMChannel:
                               if (mode == 1)
                                   channelsData[3, i] = array[i];
                               else
                                   channelsData[4, i] = array[i];
                               break;
                           case Channel.DecayChannel:
                           case Channel.ICEChannel:
                               channelsData[(byte)CurrentChannel, i] = array[i];
                               break;
                       }
                   });
                if (Mode2Stage == 2)
                {
                    Mode2Stage = 0;
                    isDataUpdated = true;
                }
            }
            //заполнение структуры для отправки в API
            var newData = new ChartPoint[pointsCount];
            var xStep = (double)LenMas / pointsCount;
            Parallel.For(0, pointsCount, channelsFill);
            if (IsCompare) Parallel.For(0, pointsCount, MatchChannelFill);
            return newData;
            void channelsFill(int i)
            {
                // заполнение списка в случае наличия данного канала в списке каналов
                if (i < XOffset)
                    newData[i].X = (int)-(xStep * (XOffset - i));
                else
                    newData[i].X = (int)(xStep * (i - XOffset));
                foreach (var channel in Channels)
                    switch (channel)
                    {
                        case Channel.Channel1:
                            newData[i].Channel1 = channelsData[(byte)channel - 1, i];
                            break;
                        case Channel.Channel2:
                            newData[i].Channel2 = channelsData[(byte)channel - 1, i];
                            break;
                        case Channel.Channel3:
                            newData[i].Channel3 = channelsData[(byte)channel - 1, i];
                            break;
                        case Channel.IDMChannel:
                            newData[i].Channel41 = channelsData[(byte)channel - 1, i];
                            newData[i].Channel42 = channelsData[(byte)channel, i];
                            break;
                        case Channel.DecayChannel:
                            newData[i].Channel5 = channelsData[(byte)channel, i];
                            break;
                        case Channel.ICEChannel:
                            newData[i].Channel6 = channelsData[(byte)channel, i];
                            break;
                    }
            }
            void MatchChannelFill(int i)
            {
                checked
                {
                    var y = (ushort)(channelsData[(byte)ComparedChannel1 - 1, i] + 128 - channelsData[(byte)ComparedChannel2 - 1, i]);
                    newData[i].MatchedChannel = y;
                }
            }
        }

        private void ReduceData<T>(ref T[] array, int max, int pointsCount)
        {
            // возможно придется обрезать массив до 50_000 точек тут
            var xD = (double)max / pointsCount; // берём переодичность считывания данных
            double j = 0;
            for (int i = 0; i < pointsCount && j < array.Length; i++, j += xD)
                array[i] = array[(int)j];
            Array.Resize(ref array, pointsCount); // уменьшаем до максимума для текущих настроек
        }
        private void Smoother(ref ushort[] array)
        { // фильтр
            for (int i = 0; i < pointsCount; i++)
            {
                smoother.AVG = array[i];
                array[i] = (ushort)smoother.AVG;
            }
        }
        // переключатель сглаживающего фильтра
        public void TurnSmoother() => isSmootherEnabled = !isSmootherEnabled;
    }
}
