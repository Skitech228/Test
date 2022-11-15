using ANG24.Sys.Infrastructure.Helpers;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;
using System.Diagnostics;
using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;

namespace ANG24.Sys.Infrastructure.Services
{
    public sealed class Autoburn : Executer, IAutoburner
    {
        #region Property
        private Step minBurnStep;
        private bool ViewUnlocker = true;
        private bool IsWaiting;
        private int PressedStep;
        private Step MaxBurnStep;
        private bool isAutoBurnRequested;

        private readonly IMainControllerOperator mainController;
        private readonly INotificationService message;
        private readonly int BurnTime;
        private readonly double burnIStepDownMultiplier;
        private readonly double BurnIStepUpMultiplier;
        private readonly double burnVlotageMultiplier;
        private CancellationTokenSource tokenSource;
        private readonly double hysteresis = 0.1; // 10%
        //private int sevenStepPowerDownTime = 120; // через 2 минуты прожига 7ой ступени вырубать питание
        private readonly Filter<double> filterI;
        private readonly Filter<double> filterU;
        private double Voltage;
        private double Current;
        private readonly IList<Step> Steps; // Не будет изменяться в ходе работы программы
        private readonly Stopwatch Watch;
        private readonly Stopwatch ResetWatch;
        private readonly Stopwatch SevenStepWatch; // Для автоотключения
        private readonly AutoResetEvent NextStepBurn = new(true);
        private Step CurrentStep;
        private bool CurrentTriggered;
        private bool VoltageTriggered;
        private double PowerBurn;
        private bool IsBurning;

        private bool IsStepBackBlocked;
        private Step MinBurnStep
        {
            get => minBurnStep;
            set
            {
                minBurnStep = value;
                MinBurnStep = value;
                foreach (var step in Steps)
                {
                    if (step.IsEnabled == false && step.StepNumber != 0) // Всё включаем .. кроме нулевой ступени
                        step.IsEnabled = true;
                    if (step.StepNumber > 0 && step.StepNumber < value.StepNumber) // ненужные выключаем
                        step.IsEnabled = false;
                }
                if (MaxBurnStep?.IsEnabled == false)
                    MaxBurnStep = Steps.FirstOrDefault(x => x.StepNumber > MaxBurnStep.StepNumber && x.IsEnabled == true) ?? Steps[^1];
            }
        }
        #endregion
        public Autoburn(IMainControllerOperator mainController,
                               INotificationService message,
                               ILabConfigurationService config,
                               Autofac.ILifetimeScope container) : base(container)
        {
            this.mainController = mainController;
            this.message = message;
            filterI = Filter<double>.NewFilter(5);
            filterU = Filter<double>.NewFilter(5);
            Watch = new Stopwatch();
            ResetWatch = new Stopwatch();
            SevenStepWatch = new Stopwatch();
            tokenSource = new CancellationTokenSource();

            Steps = new Step[]
            {
                new Step(){ StepNumber = 0, I = 0,     U = 0, IsEnabled = false},
                new Step(){ StepNumber = 1, I = 0.34F, U = 15},
                new Step(){ StepNumber = 2, I = 0.65F, U = 8},
                new Step(){ StepNumber = 3, I = 1.3F,  U = 4},
                new Step(){ StepNumber = 4, I = 2.6F,  U = 2},
                new Step(){ StepNumber = 5, I = 7,     U = 0.75F},
                new Step(){ StepNumber = 6, I = 24,    U = 0.22F},
                new Step(){ StepNumber = 7, I = 91,    U = 0.058F}
            };
            MinBurnStep = Steps[1];
            CurrentStep = Steps[0];
            MaxBurnStep = Steps[^1];

            BurnTime = (int)config["BurnTime"];
            BurnIStepUpMultiplier = (double)config["BurnIStepUpPercent"] / 100;
            burnIStepDownMultiplier = (double)config["BurnIStepDownPercent"] / 100;
            burnVlotageMultiplier = (double)config["BurnVlotagePercent"] / 100;

            mainController.OnDataReceived += MainController_OnDataReceived;
            mainController.ModuleStateChanged += Service_ModuleStateChanged;
        }
        public void SetStepBackBlocked(bool blocked) => IsStepBackBlocked = blocked;
        public async Task<bool> SetPreviousStep()
            => await ChangeStep(Steps.FirstOrDefault(x => x.StepNumber == CurrentStep.StepNumber - 1 && x.IsEnabled == true)?.StepNumber ?? 0);
        public async Task<bool> SetNextStep()
            => await ChangeStep(Steps.FirstOrDefault(x => x.StepNumber > CurrentStep.StepNumber && x.IsEnabled == true)?.StepNumber ?? 8);
        public void SetAutoBurning(bool workingMode) => isAutoBurnRequested = workingMode;

        private async Task<bool> ChangeStep(int step)
        {
            if (step > Steps.Count || step < 0) return false; // Проверка валидности переключаемой ступени            
            if (IsWaiting) // Если уже пытаемся переключить ступень
                message.SendNotificationOK("Нельзя переключать ступень во время попытки переключения ступени", null);

            ResetWatch.Reset();
            Watch.Reset();

            switch (step)
            {
                case 2:
                    if (CurrentStep.StepNumber == step - 1) // Переход с 1 ступени на 2
                    {
                        if (Voltage > 8000 && Current < 4)
                        {
                            Thread.Sleep(2000);
                            return false;
                        }
                    }
                    break;
                case 3:
                    if (CurrentStep.StepNumber == step - 1) // Переход со 2 ступени на 3
                    {
                        if (Voltage > 4000 && Current < 4)
                        {
                            Thread.Sleep(2000);
                            return false;
                        }
                    }
                    break;
            }
            PressedStep = step;
            IsWaiting = true;
            mainController.SetStep(PressedStep);
            return await CheckMovingStep();
            async Task<bool> CheckMovingStep()
            {
                var sw = new Stopwatch();
                sw.Start();
                return await Task<bool>.Factory.StartNew(() =>
                {
                    bool set = false;
                    while (sw.Elapsed < TimeSpan.FromSeconds(10) && CurrentStep.StepNumber != PressedStep)//10s
                    {
                        Thread.Sleep(200);
                        Watch.Reset(); // Так надо
                        ResetWatch.Reset();
                    }
                    IsWaiting = false;
                    if (CurrentStep.StepNumber == PressedStep) // Если получилось переключится
                    {
                        CurrentStep = Steps[PressedStep]; // Задание ступеней для View
                        set = true; // return
                    }
                    PressedStep = CurrentStep.StepNumber; // Защита, на случай, если ступень не смогла переключится
                    return set;
                });
            }
        }
        private async void MainController_OnDataReceived(MEAData data)
        {
            if (data.Module != LabModule.NoMode)
            {
                if (CurrentStep.StepNumber != data.Step && data.PowerInfoMessage.BurnEnable) // переключать ступень только если модуль прожига включен
                    CurrentStep = Steps.First(x => x.StepNumber == data.Step);
                Voltage = filterU.AVG = data.BurnVoltage;
                Current = filterI.AVG = data.BurnCurrentStep / 100.0; // ток прожига приходит в мА
                PowerBurn = data.PowerBurn switch
                {
                    BurnPower.Power100 => 1,
                    BurnPower.Power50 => 0.5,
                    _ => 0,
                };
                if (IsBurning) await CheckCurrentStepBurn();
            }
            async Task CheckCurrentStepBurn()
            {
                if (Current >= PowerBurn * BurnIStepUpMultiplier * CurrentStep.I * (CurrentTriggered ? 1 - hysteresis : 1 + hysteresis))
                // присутствует ток прожига на текущей ступени (происходит прожигание)
                { // гистерезис 10%, если текущее значение больше (необходимого * 0.9), то триггер срабатывает
                    ResetWatch.Reset();
                    CurrentTriggered = true;
                    if (Voltage <= CurrentStep.U * 1000 * burnVlotageMultiplier * (VoltageTriggered ? 1 + hysteresis : 1 - hysteresis)) // напряжение прожига ниже 50% от максимального в течение определенного, задаваемого промежутка времени
                    {// гистерезис 10%, если текущее значение меньше (необходимого * 1.1), то триггер срабатывает
                        VoltageTriggered = true;
                        if (!Watch.IsRunning) Watch.Start();

                        if (Watch.Elapsed > TimeSpan.FromSeconds(BurnTime))
                        {
                            Watch.Reset();
                            NextStepBurn.Set();
                            if (CurrentStep.StepNumber == MaxBurnStep.StepNumber)
                                SevenStepWatch.Start();
                        }
                    }
                    else
                    {
                        VoltageTriggered = false;
                        Watch.Reset();
                    }
                }
                else
                {
                    CurrentTriggered = false;
                    VoltageTriggered = false;
                    if (CurrentStep.StepNumber > MinBurnStep.StepNumber)
                        if (Current < CurrentStep.I * burnIStepDownMultiplier)
                        {
                            Watch.Reset();
                            if (!ResetWatch.IsRunning) ResetWatch.Start();
                            if (ResetWatch.Elapsed > TimeSpan.FromSeconds(5)) // если 5 секунд не было тока прожига
                            {
                                if (!IsStepBackBlocked)
                                {
                                    //IsBurning = false;
                                    await SetPreviousStep();
                                    //IsBurning = true;
                                }
                                Watch.Reset();
                                ResetWatch.Reset();
                            }
                        }
                }
            }
        }
        private void Service_ModuleStateChanged(bool started, int ExitCode)
        {
            if (isAutoBurnRequested && started && ExitCode == 0)
            {
                if (tokenSource == null || tokenSource.IsCancellationRequested)
                    tokenSource = new CancellationTokenSource();
                NextStepBurn.Set();
                StartAutoBurn();
                return;
            }
            tokenSource.Cancel();
            IsBurning = false;
            ViewUnlocker = true;
        }
        private void StartAutoBurn()
        {
            var s = Stopwatch.StartNew();
            while (CurrentStep.StepNumber != 0) // Защита
            {
                if (s.Elapsed < TimeSpan.FromSeconds(10))
                {
                    Thread.Sleep(200);
                }
                else
                {
                    s.Stop();
                    message.SendNotificationOK("Не удалось начать автоматический прожиг", null);
                    return;
                }
            }
            var token = tokenSource.Token;
            Task.Factory.StartNew(async () =>
            {
                while (PowerBurn == 3) // не начинать пока оператор не выставит мощность прожига 50 или 100%
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(200);
                }
                try
                {
                    ViewUnlocker = false;
                    IsBurning = true;

                    while (CurrentStep.StepNumber < MaxBurnStep.StepNumber)
                    {
                        NextStepBurn.WaitOne();

                        var tempStep = CurrentStep.StepNumber; // Ступень до попытки включения следующей
                        for (int i = 0; i < 3 && !ViewUnlocker && CurrentStep.StepNumber < MaxBurnStep.StepNumber; i++)// 3 попытки переключения
                        {
                            token.ThrowIfCancellationRequested();
                            if (await SetNextStep()) break;
                        }
                        if (CurrentStep.StepNumber == tempStep && tempStep != MaxBurnStep.StepNumber)
                        {// если после 3х попыток переключения мы остались на той же ступени...
                            if (!IsStepBackBlocked)
                                await SetPreviousStep();
                            else
                                NextStepBurn.Set();
                        } //ALARM! если не переключились на следующую ступень за 3 попытки

                        while (CurrentStep.StepNumber >= MaxBurnStep.StepNumber) //придумать что-нибудь получше
                        {
                            token.ThrowIfCancellationRequested();
                            Thread.Sleep(100);
                            //if (SevenStepWatch.Elapsed > TimeSpan.FromSeconds(sevenStepPowerDownTime)) // Защита от перегрева, автовыключение на последней ступени
                            //    break;
                        }
                    }
                    IsBurning = false;
                    mainController.PowerOff();
                    message.SendNotificationOK($"Прожиг завершен успешно", null);
                    ViewUnlocker = true;
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        IsBurning = false;
                        ViewUnlocker = true;
                        //message.CustomMessage($"Автоматический прожиг остановлен");
                    }
                    else
                    {
                        IsBurning = false;
                        mainController.PowerOff();
                        message.SendNotificationOK($"Прожиг завершен с ошибкой: {ex.Message}", null);
                        ViewUnlocker = true;
                    }
                }
            }, token);
        }
    }
    public class Step
    {
        public int StepNumber { get; set; }
        public float U { get; set; } // в Кв
        public float I { get; set; } // в А
        public bool IsEnabled { get; set; } // в А
    }
}
