using Exiled.API.Interfaces;
using System.ComponentModel;

namespace FermixAPI
{
    /// <summary>
    /// Конфигурация FermixAPI с расширенными настройками.
    /// </summary>
    public sealed class Config : IConfig
    {
        [Description("Включить или выключить FermixAPI")]
        public bool IsEnabled { get; set; } = true;

        [Description("Режим отладки - выводит дополнительную информацию в консоль")]
        public bool Debug { get; set; } = false;

        [Description("Показывать ASCII-логотип при запуске")]
        public bool ShowLogo { get; set; } = true;

        [Description("Показывать информацию о зависимостях при запуске")]
        public bool ShowDependencyInfo { get; set; } = true;

        [Description("Автоматически интегрироваться с HintServiceMeow если доступен")]
        public bool AutoIntegrateHSM { get; set; } = true;

        [Description("Автоматически интегрироваться с LabAPI если доступен")]
        public bool AutoIntegrateLabAPI { get; set; } = true;

        [Description("Логировать все действия API (для отладки)")]
        public bool LogAllActions { get; set; } = false;

        [Description("Максимальное количество отложенных задач в очереди")]
        public int MaxScheduledTasks { get; set; } = 100;
    }
}
