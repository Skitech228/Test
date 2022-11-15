using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ANG24.Sys.Application.Helpers
{
    public static class ScriptManager
    {
        private static ScriptState? state;
        private static ScriptOptions Options { get; } = ScriptOptions.Default
            .AddImports("System", "System.IO", "System.Collections.Generic", "System.Diagnostics",
                "System.Linq", "System.Linq.Expressions", "System.Text",
                "System.Threading.Tasks", "System.Globalization",

                "Application.Types",
                "Application.Types.Enum",
                "Application.Types.CommunicationControllerTypes",
                "Application.Types.CommunicationControllerTypes.Enums",
                "Application.Types.CommunicationControllerTypes.MEADataModels",
                "Application.Types.CommunicationControllerTypes.MEADataModels.Enum",
                "Application.Types.ServiceTypes",
                "Application.Interfaces.Services",
                "Application.Interfaces.CommunicationControllerInterfaces",
                "Application.Interfaces.CommunicationControllerInterfaces.Base")
            .AddReferences("System", "System.Core", "Microsoft.CSharp", "Application", "Communication", "Infrastructure");

        /// <summary>
        /// Запуск/постановка в очередь выполнения скрипта
        /// </summary>
        /// <param name="sender">объект выполнения скрипта</param>
        /// <param name="scriptStr">скрипт</param>
        /// <param name="imports">дополнительные пространства имён</param>
        /// <returns>процесс выполнения скрипта</returns>
        public static async Task StartScript(object sender, string? scriptStr, IEnumerable<string>? imports = null)
        {
            var options = SetUpOptions(imports);
            if (state is null)
            {
                state = await CSharpScript.RunAsync(scriptStr, options, globals: sender);
                return;
            }
            state = await state.ContinueWithAsync(scriptStr, options: options);
        }

        /// <summary>
        /// Задать настройки скриптов
        /// </summary>
        /// <param name="imports">пространства имён, требующихся для работы скрипта (помимо встроенных)</param>
        /// <returns>готовый к работе ScriptOptions</returns>
        private static ScriptOptions SetUpOptions(IEnumerable<string>? imports = null)
        {
            var options = Options;
            if (imports is not null)
                Options.AddImports(imports);
            return options;
        }
    }
}

