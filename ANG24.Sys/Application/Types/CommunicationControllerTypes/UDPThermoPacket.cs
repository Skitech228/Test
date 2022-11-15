namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public class UDPThermoPacket
    {
        public static event Action OnThermoWarning;
        public static event Action OnThermoExt;

        private int _max;
        private static bool notNotifyWarning = true;
        private static bool notNotifyExt = true;
        private int _controlled_max;

        public int[] pixelData { get; set; }
        public int Min { get; set; }
        public int Max { get => _max; set => _max = value; }
        public int Controlled_Min { get; set; }
        public int Controlled_Max
        {
            get => _controlled_max;
            set
            {
                _controlled_max = value;
                if (value / 10 - 273 > 70.0)
                {
                    if (notNotifyWarning)
                    {
                        OnThermoWarning?.Invoke();
                        notNotifyWarning = false;
                        new Timer(state =>
                        {
                            notNotifyWarning = true;
                        }).Change(10000, Timeout.Infinite);
                    }
                }
                if (value / 10 - 273 > 100.0)
                {
                    if (notNotifyExt)
                    {
                        OnThermoExt?.Invoke();
                        notNotifyExt = false;
                        new Timer(state =>
                        {
                            notNotifyExt = true;
                        }).Change(10000, Timeout.Infinite);
                    }
                }
            }
        }
        public int[] OffsetData { get; set; }
        public int VDD { get; set; }
        public int T_amb { get; set; }
        public int[] PTAT_Data { get; set; }

        public UDPThermoPacket(byte[] ReceivedData)
        {
            pixelData = new int[5120];
            OffsetData = new int[1280];
            PTAT_Data = new int[8];

            int counter = 0;
            int PixelCounter = 0;
            int OffsetCounter = 0;
            for (int pack = 0; pack < 7; pack++)
            {
                //1-7 packet
                for (int i = 0; i < 1283; i++)
                {
                    if (i == 0)
                    {
                        counter++;
                        continue;
                    }
                    // если четное
                    if (i % 2 == 0)
                    {
                        pixelData[PixelCounter] |= ReceivedData[counter] << 8;
                        PixelCounter++;
                    }
                    else
                    {
                        pixelData[PixelCounter] |= ReceivedData[counter];
                    }
                    counter++;
                }
            }
            counter++;
            //8 packet
            for (int i = 0; i < 1266; i++)
            {
                if (i % 2 == 0)
                {
                    pixelData[PixelCounter] |= ReceivedData[counter];
                }
                else
                {
                    pixelData[PixelCounter] |= ReceivedData[counter] << 8;
                    PixelCounter++;
                }
                counter++;
            }
            for (int column = 0; column < 5; column++)
            {
                for (int row = 0; row < 5; row++)
                {
                    int dis_counter = 80 * row + column;
                    pixelData[dis_counter] = 20 * 10 + 273;
                }
            }
            for (int column = 75; column < 80; column++)
            {
                for (int row = 0; row < 5; row++)
                {
                    int dis_counter = 80 * row + column;
                    pixelData[dis_counter] = 20 * 10 + 273;
                }
            }
            for (int column = 75; column < 80; column++)
            {
                for (int row = 59; row < 64; row++)
                {
                    int dis_counter = 80 * row + column;
                    pixelData[dis_counter] = 20 * 10 + 273;
                }
            }
            for (int column = 0; column < 5; column++)
            {
                for (int row = 59; row < 64; row++)
                {
                    int dis_counter = 80 * row + column;
                    pixelData[dis_counter] = 20 * 10 + 273;
                }
            }

            var pixel_list = pixelData.ToList();
            Controlled_Min = pixel_list.Min();
            Controlled_Max = pixel_list.Max();
            var TAVG = pixel_list.Average();
            for (int i = 0; i < 16; i++)
            {
                if (i % 2 == 0)
                {
                    OffsetData[OffsetCounter] |= ReceivedData[counter];
                }
                else
                {
                    OffsetData[OffsetCounter] |= ReceivedData[counter] << 8;
                    OffsetCounter++;
                }
                counter++;
            }
            counter++;
            //9 packet
            for (int i = 0; i < 1282; i++)
            {
                if (i % 2 == 0)
                {
                    OffsetData[OffsetCounter] |= ReceivedData[counter];
                }
                else
                {
                    OffsetData[OffsetCounter] |= ReceivedData[counter] << 8;
                    OffsetCounter++;
                }
                counter++;
            }
            counter++;
            // 10 packet
            for (int i = 0; i < 1262; i++)
            {
                if (i % 2 == 0)
                {
                    OffsetData[OffsetCounter] |= ReceivedData[counter];
                }
                else
                {
                    OffsetData[OffsetCounter] |= ReceivedData[counter] << 8;
                    OffsetCounter++;
                }
                counter++;
            }
            int VDD_temp = ReceivedData[counter + 1] << 8;
            VDD_temp |= ReceivedData[counter];
            VDD = VDD_temp;
            counter += 2;
            T_amb = ReceivedData[counter + 1] << 8;
            T_amb |= ReceivedData[counter];
            counter += 2;

            int PTAT_counter = 0;
            for (int i = 0; i < 16; i++)
            {
                if (i % 2 == 0)
                {
                    PTAT_Data[PTAT_counter] |= ReceivedData[counter];
                }
                else
                {
                    PTAT_Data[PTAT_counter] |= ReceivedData[counter] << 8;
                    PTAT_counter++;
                }
                counter++;
            }
            Min = (int)TAVG - 50;
            Max = (int)TAVG + 550;
        }
    }
    public struct ThermoValue
    {
        public int Temp { get; set; }
        public bool NotControlled { get; set; }
    }
}

