using System.IO.Ports;

namespace ANG24.Sys.Communication.Types
{
    public class ReturnRequestKP500Controller
    {
        public bool state { get; set; }
        public string messaga { get; set; }
        public byte[] value { get; set; }
    }
    public class KP500ControllerData
    {
        private static KP500ControllerData _kp500ControllerData;
        private SerialPort _currentPort;
        private readonly object _locker = new object();
        private bool resetAll = false;

        public static KP500ControllerData Instance()
        {
            if (_kp500ControllerData == null)
            {
                _kp500ControllerData = new KP500ControllerData();
            }
            return _kp500ControllerData;
        }

        public bool Init()
        {
            bool result = true;
            //return true;
            if (_currentPort == null)
            {
                lock (_locker)
                {
                    try
                    {
                        _currentPort = new SerialPort("COM15", 19200, Parity.None, 8, StopBits.One)
                        {
                            ReadTimeout = 100
                        };
                        if (!_currentPort.IsOpen) _currentPort.Open();
                        if (!_currentPort.IsOpen)
                        {
                            result = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        var t1 = ex;
                        result = false;
                    }
                }
            }
            else
            {
                try
                {
                    if (!_currentPort.IsOpen) _currentPort.Open();
                    if (!_currentPort.IsOpen)
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    var t = ex;
                    result = false;
                }
            }
            if (!result) _currentPort = null;
            return result;
        }

        public bool InitAuto()
        {
            bool result = false;
            if (_currentPort == null)
            {
                lock (_locker)
                {
                    string[] ports = SerialPort.GetPortNames();
                    foreach (var port in ports)
                    {
                        var selectPort = new SerialPort(port, 19200, Parity.None, 8, StopBits.One)
                        {
                            ReadTimeout = 75,
                            WriteTimeout = 2
                        };
                        try
                        {
                            if (!selectPort.IsOpen)
                            {
                                selectPort.Open();
                            }
                            Thread.Sleep(20);
                            selectPort.DiscardInBuffer();
                            int count = 3;
                            while (count > 0)
                            {
                                try
                                {
                                    var request = ReadValue(0x01, 20, selectPort);
                                    if (request.state == true && request.value[1] == 1)
                                    {
                                        _currentPort = selectPort;
                                        result = true;
                                        break;
                                    }
                                    else
                                    {
                                        count--;
                                    }
                                }
                                catch (Exception)
                                {
                                    count--;
                                }
                            }
                            if (count == 0)
                            {
                                if (selectPort != null)
                                {
                                    selectPort.Close();
                                    selectPort.Dispose();
                                }
                            }

                        }
                        catch (Exception)
                        {
                            if (selectPort != null)
                            {
                                selectPort.Close();
                                selectPort.Dispose();
                            }
                        }
                    }
                }
            }
            else
            {
                try
                {
                    if (!_currentPort.IsOpen) _currentPort.Open();
                    if (!_currentPort.IsOpen)
                    {
                        if (_currentPort != null)
                        {
                            _currentPort.Close();
                            _currentPort.Dispose();
                        }
                    }
                    else
                    {
                        Thread.Sleep(20);
                        _currentPort.DiscardInBuffer();
                        int count = 3;
                        while (count > 0)
                        {
                            try
                            {
                                var request = ReadValue(0x01, 20);
                                if (request.state == true && request.value[1] == 1)
                                {
                                    result = true;
                                    break;
                                }
                                else
                                {
                                    count--;
                                }
                            }
                            catch (Exception)
                            {
                                count--;
                            }
                        }
                        if (count <= 0)
                        {
                            if (_currentPort != null)
                            {
                                _currentPort.Close();
                                _currentPort.Dispose();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    if (_currentPort != null)
                    {
                        _currentPort.Close();
                        _currentPort.Dispose();
                    }
                }
            }
            return result;
        }

        public void Close()
        {
            if (_currentPort != null)
            {
                if (_currentPort.IsOpen)
                {
                    _currentPort.Close();
                }
            }
        }

        private ReturnRequestKP500Controller ReadValue(byte command, int bufferLength)
        {
            ReturnRequestKP500Controller result = new ReturnRequestKP500Controller
            {
                state = false,
                messaga = string.Empty
            };
            lock (_locker)
            {
                if (_currentPort != null)
                {
                    if (!_currentPort.IsOpen)
                    {
                        result.state = false;
                        result.messaga = "COM порт закрыт";
                        return result;
                    }

                    byte[] bufer = new byte[] { 0x0A, command }; //адрес, команда (адрес по умолчанию -- 10(HEX = 0x0A)
                    try
                    {
                        _currentPort.Write(bufer, 0, 2);
                    }
                    catch (Exception ex)
                    {
                        result.state = false;
                        result.messaga = ex.Message;
                        return result;
                    }

                    try
                    {
                        byte[] buferReq = new byte[bufferLength];
                        Thread.Sleep(100);
                        int res = _currentPort.Read(buferReq, 0, bufferLength);
                        if (res != -1)
                        {

                            result.state = true;
                            result.value = buferReq;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.state = false;
                        result.messaga = ex.Message;
                        return result;
                    }
                }


            }
            return result;
        }

        private ReturnRequestKP500Controller ReadValue(byte command, int bufferLength, SerialPort port)
        {
            ReturnRequestKP500Controller result = new ReturnRequestKP500Controller
            {
                state = false,
                messaga = string.Empty
            };
            lock (_locker)
            {
                if (port != null)
                {
                    if (!port.IsOpen)
                    {
                        result.state = false;
                        result.messaga = "COM порт закрыт";
                        return result;
                    }
                }

                byte[] bufer = new byte[] { 0x0A, command };
                port.Write(bufer, 0, 2);

                try
                {
                    byte[] buferReq = new byte[bufferLength];
                    Thread.Sleep(100);
                    int res = port.Read(buferReq, 0, bufferLength);
                    if (res != -1)
                    {

                        result.state = true;
                        result.value = buferReq;
                    }
                }
                catch (Exception ex)
                {
                    result.state = false;
                    result.messaga = ex.Message;
                    return result;
                }
            }
            return result;
        }

        private void SetValue(byte command)
        {

            lock (_locker)
            {
                if (_currentPort != null)
                {
                    if (!_currentPort.IsOpen)
                    {
                        return;
                    }

                    byte[] bufer = new byte[] { 0x0A, command };
                    _currentPort.Write(bufer, 0, 2);
                }
            }
        }

        public ReturnRequestKP500Controller GetCurrentInfo()
        {
            return ReadValue(0x01, 20);
        }

        public ReturnRequestKP500Controller GetInfo05()
        {
            return ReadValue(0x05, 20);
        }

        public void DownButtonPlus()
        {
            SetValue(0x15);
        }

        public void UpButtonPlus()
        {
            SetValue(0x16);
        }

        public void DownButtonMinus()
        {
            SetValue(0x0B);
        }

        public void UpButtonMinus()
        {
            SetValue(0x0C);
        }

        public void ButtonReset()
        {
            SetValue(0x1E);
        }

        public void ButtonMode()
        {
            SetValue(0x28);
        }

        public void ButtonMatching()
        {
            SetValue(0x50);
        }

        public void SetManualFrequency()
        {
            SetValue(0x1E);
        }

        public void SaveFrequency()
        {
            SetValue(0x63);
        }

        public void ButtonResetAll()
        {
            SetValue(0x5A);
            resetAll = false;
        }

        public void SetFrequency(KP500FrequencyEnum frqequency)
        {
            int step = 0;
            int countError = 0;
            bool result = false;
            ReturnRequestKP500Controller request = new ReturnRequestKP500Controller { state = false };
            int oldMode = 0;
            int oldFrequency = 0;

            while (!result || resetAll)
            {
                switch (step)
                {
                    case 0:
                        // Чтение текущего состояния
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError = 0;
                        step = 1;
                        break;
                    case 1:
                        // Переход в режим изменения сопротивления
                        if (request.value[2] == 43)
                        {
                            countError = 0;
                            step = 3;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 2;
                        }
                        break;
                    case 2:
                        // Ожидание что режим изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50) return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 1;
                        }
                        break;
                    case 3:
                        // Изменение режима
                        // Проверка что это окно переключение частоты
                        if (request.value[2] != 43)
                        {
                            step = 0;
                            break;
                        }
                        // Проверка состояния переключения импульсов
                        if ((request.value[3] & 3) == (byte)frqequency)
                        {
                            // перепроверка текущего состояния
                            request = GetCurrentInfo();
                            if (request.state == false) return;
                            if ((request.value[3] & 3) == (byte)frqequency)
                            {
                                step = 6;
                                countError = 0;
                            }
                            else
                            {
                                oldFrequency = request.value[3] & 3;
                                step = 4;
                            }
                        }
                        else
                        {
                            // Сохраняем текущее значение
                            oldFrequency = request.value[3] & 3;
                            step = 4;
                        }
                        break;
                    case 4:
                        // Изменение частоты
                        DownButtonPlus();
                        Thread.Sleep(50);
                        UpButtonPlus();
                        Thread.Sleep(250);
                        step = 5;
                        break;
                    case 5:
                        // Проверка что это окно переключение сопротивления
                        if (request.value[2] != 43)
                        {
                            step = 0;
                            break;
                        }
                        // Ожидание что изменился режим сопротивления
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50) return;
                        if (oldFrequency == (request.value[3] & 3))
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 3;
                        }
                        break;
                    case 6:
                        // Возврат в главное меню
                        ButtonMode();
                        Thread.Sleep(200);
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        step = 7;
                        break;
                    case 7:
                        // Переход в главное меню
                        if (request.value[2] == 11)
                        {
                            countError = 0;
                            return;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 8;
                        }
                        break;
                    case 8:
                        // Ожидание что режим смены импульсов изменился
                        request = GetCurrentInfo();
                        if (request.state == false)
                            return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 7;
                        }
                        break;
                }
            }
            resetAll = false;
        }

        public void SetResistance(KP500Resistance resist)
        {
            int step = 0;
            int countError = 0;
            bool result = false;
            ReturnRequestKP500Controller request = new ReturnRequestKP500Controller { state = false };
            int oldMode = 0;
            int oldResist = 0;

            while (!result || resetAll)
            {

                switch (step)
                {
                    case 0:
                        // Чтение текущего состояния
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        step = 1;
                        countError = 0;
                        break;
                    case 1:
                        // Переход в режим изменения сопротивления
                        if (request.value[2] == 75)
                        {
                            step = 3;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 2;
                        }
                        break;
                    case 2:
                        // Ожидание что режим изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 1;
                        }
                        break;
                    case 3:
                        // Изменение режима
                        // Проверка что это окно переключение сопротивления
                        if (request.value[2] != 75)
                        {
                            step = 0;
                            break;
                        }
                        // Проверка состояния переключения импульсов
                        if ((request.value[3] & 240) == (byte)resist)
                        {
                            // перепроверка текущего состояния
                            request = GetCurrentInfo();
                            if (request.state == false) return;
                            if ((request.value[3] & 240) == (byte)resist)
                            {
                                step = 6;
                            }
                            else
                            {
                                oldResist = request.value[3] & 240;
                                step = 4;
                            }
                        }
                        else
                        {
                            // Сохраняем текущее значение
                            oldResist = request.value[3] & 240;
                            step = 4;
                        }
                        break;
                    case 4:
                        // Изменение сопротивления
                        if ((request.value[3] & 240) > (byte)resist)
                        {
                            DownButtonMinus();
                            Thread.Sleep(50);
                            UpButtonMinus();
                            Thread.Sleep(500);
                        }
                        else
                        {
                            DownButtonPlus();
                            Thread.Sleep(50);
                            UpButtonPlus();
                            Thread.Sleep(500);
                        }
                        step = 5;
                        break;
                    case 5:
                        // Проверка что это окно переключение сопротивления
                        if (request.value[2] != 75)
                        {
                            step = 0;
                            break;
                        }
                        // Ожидание что изменился режим сопротивления
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldResist == (request.value[3] & 240))
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 3;
                        }
                        break;
                    case 6:
                        // Возврат в главное меню
                        ButtonMode();
                        Thread.Sleep(200);
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        step = 7;
                        break;
                    case 7:
                        // Переход в главное меню
                        if (request.value[2] == 11)
                        {
                            countError = 0;
                            return;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 8;
                        }
                        break;
                    case 8:
                        // Ожидание что режим смены импульсов изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 7;
                        }
                        break;
                }
            }
            resetAll = false;
        }

        public void SetModeImp(KP500ModeImp modeImp)
        {
            int step = 0;
            int countError = 0;
            bool result = false;
            ReturnRequestKP500Controller request = new ReturnRequestKP500Controller { state = false };
            byte oldMode = 0;
            byte oldModeImp = 0;

            while (!result || resetAll)
            {
                switch (step)
                {
                    case 0:
                        // Чтение текущего состояния
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        step = 1;
                        countError = 0;
                        break;
                    case 1:
                        // Переход в режим смены импульсов
                        if (request.value[2] == 59)
                        {
                            step = 3;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 2;
                        }
                        break;
                    case 2:
                        // Ожидание что режим смены импульсов изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 1;
                        }
                        break;
                    case 3:
                        // Изменение режима
                        // Проверка что это окно переключение импульсов
                        if (request.value[2] != 59)
                        {
                            step = 0;
                            break;
                        }
                        // Проверка состояния переключения импульсов
                        if (request.value[11] == (byte)modeImp)
                        {
                            // перепроверка текущего состояния
                            request = GetCurrentInfo();
                            if (request.state == false) return;
                            if (request.value[11] == (byte)modeImp)
                            {
                                step = 6;
                            }
                            else
                            {
                                oldModeImp = request.value[11];
                                step = 4;
                            }
                        }
                        else
                        {
                            // Сохраняем текущее значение
                            oldModeImp = request.value[11];
                            step = 4;
                        }
                        break;
                    case 4:
                        // Изменение импульсов
                        DownButtonPlus();
                        Thread.Sleep(50);
                        UpButtonPlus();
                        Thread.Sleep(500);
                        step = 5;
                        break;
                    case 5:
                        // Проверка что это окно переключение импульсов
                        if (request.value[2] != 59)
                        {
                            step = 0;
                            break;
                        }
                        // Ожидание что изменился режим импульсов
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldModeImp == request.value[11])
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 3;
                        }
                        break;
                    case 6:
                        // Возврат в главное меню
                        ButtonMode();
                        Thread.Sleep(200);
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        step = 7;
                        break;
                    case 7:
                        // Переход в главное меню
                        if (request.value[2] == 11)
                        {
                            result = true;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 8;
                        }
                        break;
                    case 8:
                        // Ожидание что режим смены импульсов изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 7;
                        }
                        break;
                }
            }
            resetAll = false;
        }

        public void SetModeMCH2(KP500SetMCH2 setMCH2)
        {
            int step = 0;
            int countError = 0;
            bool result = false;
            ReturnRequestKP500Controller request = new ReturnRequestKP500Controller { state = false };
            byte oldMode = 0;
            byte oldModeImp = 0;
            int oldFrequency = 0;

            while (!result || resetAll)
            {

                switch (step)
                {
                    case 0:
                        // Чтение текущего состояния
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        step = 1;
                        //countError = 0;
                        break;
                    case 1:
                        // Переход в режим смены частоты
                        if (request.value[2] == 43)
                        {
                            step = 3;
                            countError = 0;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 2;
                        }
                        break;
                    case 2:
                        // Ожидание что режим изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50) return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 1;
                        }
                        break;
                    case 3:
                        // Изменение режима
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        // Проверка что это окно переключение частоты
                        if (request.value[2] != 43 || request.value[2] == 10)
                        {
                            step = 0;
                            countError++;
                            break;
                        }
                        switch (setMCH2)
                        {
                            case KP500SetMCH2.Set12:
                                // Проверка состояния переключения импульсов
                                if ((request.value[3] & 3) == (byte)KP500FrequencyEnum.Frq_1069)
                                {
                                    // перепроверка текущего состояния
                                    request = GetCurrentInfo();
                                    if (request.state == false) return;
                                    if ((request.value[3] & 3) == (byte)KP500FrequencyEnum.Frq_1069)
                                    {
                                        step = 6;
                                        countError = 0;
                                    }
                                    else
                                    {
                                        oldFrequency = request.value[3] & 3;
                                        step = 4;
                                    }
                                }
                                else
                                {
                                    // Сохраняем текущее значение
                                    oldFrequency = request.value[3] & 3;
                                    step = 4;
                                }
                                break;
                            case KP500SetMCH2.Set23:
                                // Проверка состояния переключения импульсов
                                if ((request.value[3] & 3) == (byte)KP500FrequencyEnum.Frq_9796)
                                {
                                    // перепроверка текущего состояния
                                    request = GetCurrentInfo();
                                    if (request.state == false) return;
                                    if ((request.value[3] & 3) == (byte)KP500FrequencyEnum.Frq_9796)
                                    {
                                        step = 6;
                                        countError = 0;
                                    }
                                    else
                                    {
                                        oldFrequency = request.value[3] & 3;
                                        step = 4;
                                    }
                                }
                                else
                                {
                                    // Сохраняем текущее значение
                                    oldFrequency = request.value[3] & 3;
                                    step = 4;
                                }
                                break;
                            case KP500SetMCH2.Set13:
                                // Проверка состояния переключения импульсов
                                if ((request.value[3] & 3) == (byte)KP500FrequencyEnum.Frq_480)
                                {
                                    // перепроверка текущего состояния
                                    request = GetCurrentInfo();
                                    if (request.state == false) return;
                                    if ((request.value[3] & 3) == (byte)KP500FrequencyEnum.Frq_480)
                                    {
                                        step = 6;
                                        countError = 0;
                                    }
                                    else
                                    {
                                        oldFrequency = request.value[3] & 3;
                                        step = 4;
                                    }
                                }
                                else
                                {
                                    // Сохраняем текущее значение
                                    oldFrequency = request.value[3] & 3;
                                    step = 4;
                                }
                                break;
                        }
                        break;
                    case 4:
                        // Изменение частоты
                        DownButtonPlus();
                        Thread.Sleep(50);
                        UpButtonPlus();
                        Thread.Sleep(250);
                        step = 5;
                        break;
                    case 5:
                        request = GetCurrentInfo();
                        // Проверка что это окно переключение частот
                        if (request.value[2] != 43 || request.value[2] == 10)
                        {
                            step = 0;
                            break;
                        }
                        // Ожидание что изменился режим сопротивления

                        if (request.state == false) return;
                        countError++;
                        if (countError > 50) return;
                        if (oldFrequency == (request.value[3] & 3))
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 3;
                        }
                        break;
                    case 6:
                        // Чтение текущего состояния
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        step = 7;
                        //countError = 0;
                        break;

                    case 7:
                        // Переход в режим изменения частоты
                        if (request.value[2] == 59)
                        {
                            step = 9;
                            countError = 0;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 8;
                        }
                        break;
                    case 8:
                        // Ожидание что режим смены импульсов изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 7;
                        }
                        break;
                    case 9:
                        // Изменение режима
                        // Проверка что это окно переключение импульсов
                        if (request.value[2] != 59 || request.value[2] == 10)
                        {
                            step = 6;
                            countError++;
                            break;
                        }
                        // Проверка состояния переключения импульсов
                        if (request.value[11] == (byte)KP500ModeImp.IM2)
                        {
                            // перепроверка текущего состояния
                            request = GetCurrentInfo();
                            if (request.state == false) return;
                            if (request.value[11] == (byte)KP500ModeImp.IM2)
                            {
                                step = 12;
                            }
                            else
                            {
                                oldModeImp = request.value[11];
                                step = 10;
                            }
                        }
                        else
                        {
                            // Сохраняем текущее значение
                            oldModeImp = request.value[11];
                            step = 10;
                        }
                        break;
                    case 10:
                        // Изменение импульсов
                        DownButtonPlus();
                        Thread.Sleep(50);
                        UpButtonPlus();
                        Thread.Sleep(500);
                        step = 11;
                        break;
                    case 11:
                        request = GetCurrentInfo();
                        // Проверка что это окно переключение импульсов
                        if (request.value[2] != 59 || request.value[2] == 10)
                        {
                            step = 6;
                            break;
                        }
                        // Ожидание что изменился режим импульсов                       
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldModeImp == request.value[11])
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 9;
                            countError = 0;
                        }
                        break;
                    case 12:
                        // Возврат в главное меню
                        ButtonMode();
                        Thread.Sleep(200);
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        step = 13;
                        break;
                    case 13:
                        // Переход в главное меню
                        if (request.value[2] == 11)
                        {
                            result = true;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 14;
                        }
                        break;
                    case 14:
                        // Ожидание что режим смены импульсов изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 13;
                        }
                        break;
                }
            }
            resetAll = false;
        }

        public bool OpenSetFrequency()
        {
            int step = 0;
            int countError = 0;
            bool result = false;
            ReturnRequestKP500Controller request = new ReturnRequestKP500Controller { state = false };
            byte oldMode = 0;

            while (!result || resetAll)
            {
                switch (step)
                {
                    case 0:
                        // Чтение текущего состояния
                        request = GetCurrentInfo();
                        if (request.state == false) return result;
                        step = 1;
                        countError = 0;
                        break;
                    case 1:
                        // Переход в режим смены импульсов
                        if (request.value[2] == 43)
                        {
                            step = 3;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(100);
                            step = 2;
                        }
                        break;
                    case 2:
                        // Ожидание что режим смены импульсов изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return result;
                        countError++;
                        if (countError > 50)
                        {
                            return result;
                        }
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 1;
                        }
                        break;
                    case 3:
                        // Переход в режим изменения частоты
                        request = GetCurrentInfo();
                        if (request.state == false) return result;
                        Thread.Sleep(100);
                        var currentModeFreq = request.value[3] & 7;
                        if (currentModeFreq == 3 || currentModeFreq == 4 || currentModeFreq == 5)
                        {
                            result = true;
                        }
                        else
                        {
                            // перепроверка
                            Thread.Sleep(100);
                            request = GetCurrentInfo();
                            if (request.state == false) return result;
                            countError++;
                            if (countError > 50)
                            {
                                return result;
                            }
                            currentModeFreq = request.value[3] & 7;
                            if (currentModeFreq == 3 || currentModeFreq == 4 || currentModeFreq == 5)
                            {
                                result = true;
                            }
                            else
                            {
                                SetManualFrequency();
                                Thread.Sleep(100);
                            }
                        }
                        break;
                }
            }
            resetAll = false;
            return result;
        }

        public void SetMainMenu()
        {
            int step = 0;
            int countError = 0;
            bool result = false;
            ReturnRequestKP500Controller request = new ReturnRequestKP500Controller { state = false };
            byte oldMode = 0;

            while (!result)
            {
                switch (step)
                {
                    case 0:
                        // Чтение текущего состояния
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        step = 1;
                        countError = 0;
                        break;
                    case 1:
                        // Переход в режим смены импульсов
                        if (request.value[2] == 11)
                        {
                            result = true;
                        }
                        else
                        {
                            oldMode = request.value[2];
                            ButtonMode();
                            Thread.Sleep(200);
                            step = 2;
                        }
                        break;
                    case 2:
                        // Ожидание что режим смены импульсов изменился
                        request = GetCurrentInfo();
                        if (request.state == false) return;
                        countError++;
                        if (countError > 50)
                            return;
                        if (oldMode == request.value[2] || request.value[2] == 10)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            step = 1;
                        }
                        break;
                }
            }
        }
    }
}
