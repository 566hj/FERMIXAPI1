using System;
using System.Collections.Generic;
using System.Text;
using Exiled.API.Features;
using FermixAPI.Core;
using MEC;

namespace FermixAPI
{
    /// <summary>
    /// Расширенная система хинтов с поддержкой форматирования, анимаций и HintServiceMeow.
    /// </summary>
    public static class FermixHint
    {
        #region Colors - Цвета

        public const string White = "white";
        public const string Black = "black";
        public const string Red = "red";
        public const string Green = "green";
        public const string Blue = "blue";
        public const string Yellow = "yellow";
        public const string Cyan = "cyan";
        public const string Magenta = "magenta";
        public const string Orange = "orange";
        public const string Pink = "pink";
        public const string Purple = "purple";
        public const string Gray = "gray";
        public const string Gold = "#FFD700";
        public const string Silver = "#C0C0C0";
        public const string Lime = "#00FF00";
        public const string Aqua = "#00FFFF";

        #endregion

        #region Basic Hints - Базовые Хинты

        /// <summary>
        /// Отправляет простой хинт.
        /// </summary>
        public static void Send(Player player, string message, float duration = 5f)
        {
            player.ShowHint(message, duration);
        }

        /// <summary>
        /// Отправляет цветной хинт.
        /// </summary>
        public static void SendColored(Player player, string message, string color, float duration = 5f)
        {
            player.ShowHint(Color(message, color), duration);
        }

        /// <summary>
        /// Отправляет хинт успеха.
        /// </summary>
        public static void Success(Player player, string message, float duration = 3f)
        {
            player.ShowHint(Color(message, Green), duration);
        }

        /// <summary>
        /// Отправляет хинт ошибки.
        /// </summary>
        public static void Error(Player player, string message, float duration = 3f)
        {
            player.ShowHint(Color($"[!] {message}", Red), duration);
        }

        /// <summary>
        /// Отправляет хинт предупреждения.
        /// </summary>
        public static void Warning(Player player, string message, float duration = 3f)
        {
            player.ShowHint(Color($"[!] {message}", Yellow), duration);
        }

        /// <summary>
        /// Отправляет информационный хинт.
        /// </summary>
        public static void Info(Player player, string message, float duration = 3f)
        {
            player.ShowHint(Color(message, Cyan), duration);
        }

        #endregion

        #region Global Hints - Глобальные Хинты

        /// <summary>
        /// Отправляет хинт всем игрокам.
        /// </summary>
        public static void SendToAll(string message, float duration = 5f)
        {
            foreach (var player in Player.List)
            {
                player.ShowHint(message, duration);
            }
        }

        /// <summary>
        /// Отправляет цветной хинт всем игрокам.
        /// </summary>
        public static void SendToAllColored(string message, string color, float duration = 5f)
        {
            var formatted = Color(message, color);
            foreach (var player in Player.List)
            {
                player.ShowHint(formatted, duration);
            }
        }

        /// <summary>
        /// Отправляет хинт успеха всем.
        /// </summary>
        public static void SuccessToAll(string message, float duration = 3f)
        {
            SendToAllColored(message, Green, duration);
        }

        /// <summary>
        /// Отправляет хинт ошибки всем.
        /// </summary>
        public static void ErrorToAll(string message, float duration = 3f)
        {
            SendToAllColored($"[!] {message}", Red, duration);
        }

        /// <summary>
        /// Отправляет хинт по условию.
        /// </summary>
        public static void SendWhere(Func<Player, bool> predicate, string message, float duration = 5f)
        {
            foreach (var player in Player.List)
            {
                if (predicate(player))
                {
                    player.ShowHint(message, duration);
                }
            }
        }

        #endregion

        #region Formatted Hints - Форматированные Хинты

        /// <summary>
        /// Отправляет хинт с заголовком.
        /// </summary>
        public static void SendWithTitle(Player player, string title, string message, float duration = 5f)
        {
            var formatted = $"{Bold(Size(title, 30))}\n{message}";
            player.ShowHint(formatted, duration);
        }

        /// <summary>
        /// Отправляет хинт со списком.
        /// </summary>
        public static void SendList(Player player, string title, IEnumerable<string> items, float duration = 5f)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Bold(title));
            
            foreach (var item in items)
            {
                sb.AppendLine($"• {item}");
            }
            
            player.ShowHint(sb.ToString(), duration);
        }

        /// <summary>
        /// Отправляет хинт с прогресс-баром.
        /// </summary>
        public static void SendProgress(Player player, string label, float progress, int barLength = 20, float duration = 1f)
        {
            var filled = (int)(progress * barLength);
            var empty = barLength - filled;
            
            var bar = $"[{new string('█', filled)}{new string('░', empty)}] {(progress * 100):F0}%";
            var formatted = $"{label}\n{bar}";
            
            player.ShowHint(formatted, duration);
        }

        /// <summary>
        /// Отправляет многострочный хинт.
        /// </summary>
        public static void SendMultiline(Player player, float duration, params string[] lines)
        {
            player.ShowHint(string.Join("\n", lines), duration);
        }

        #endregion

        #region Animated Hints - Анимированные Хинты

        /// <summary>
        /// Отправляет печатающийся хинт.
        /// </summary>
        public static CoroutineHandle SendTyping(Player player, string message, float charDelay = 0.05f, float finalDuration = 2f)
        {
            return FermixCore.RunCoroutine(TypingCoroutine(player, message, charDelay, finalDuration));
        }

        private static IEnumerator<float> TypingCoroutine(Player player, string message, float charDelay, float finalDuration)
        {
            var current = "";
            
            foreach (var c in message)
            {
                current += c;
                player.ShowHint(current + "_", 1f);
                yield return Timing.WaitForSeconds(charDelay);
            }
            
            player.ShowHint(current, finalDuration);
        }

        /// <summary>
        /// Отправляет мигающий хинт.
        /// </summary>
        public static CoroutineHandle SendBlinking(Player player, string message, float interval = 0.5f, int blinks = 5)
        {
            return FermixCore.RunCoroutine(BlinkingCoroutine(player, message, interval, blinks));
        }

        private static IEnumerator<float> BlinkingCoroutine(Player player, string message, float interval, int blinks)
        {
            for (int i = 0; i < blinks * 2; i++)
            {
                player.ShowHint(i % 2 == 0 ? message : "", interval);
                yield return Timing.WaitForSeconds(interval);
            }
        }

        /// <summary>
        /// Отправляет хинт с обратным отсчётом.
        /// </summary>
        public static CoroutineHandle SendCountdown(Player player, string format, int seconds, Action onComplete = null)
        {
            return FermixCore.RunCoroutine(CountdownCoroutine(player, format, seconds, onComplete));
        }

        private static IEnumerator<float> CountdownCoroutine(Player player, string format, int seconds, Action onComplete)
        {
            for (int i = seconds; i > 0; i--)
            {
                player.ShowHint(string.Format(format, i), 1.1f);
                yield return Timing.WaitForSeconds(1f);
            }
            
            onComplete?.Invoke();
        }

        /// <summary>
        /// Отправляет последовательность хинтов.
        /// </summary>
        public static CoroutineHandle SendSequence(Player player, float interval, params string[] messages)
        {
            return FermixCore.RunCoroutine(SequenceCoroutine(player, interval, messages));
        }

        private static IEnumerator<float> SequenceCoroutine(Player player, float interval, string[] messages)
        {
            foreach (var message in messages)
            {
                player.ShowHint(message, interval);
                yield return Timing.WaitForSeconds(interval);
            }
        }

        /// <summary>
        /// Отправляет появляющийся/исчезающий хинт (симуляция через прозрачность).
        /// </summary>
        public static CoroutineHandle SendFade(Player player, string message, float fadeInTime = 0.5f, float displayTime = 2f, float fadeOutTime = 0.5f)
        {
            return FermixCore.RunCoroutine(FadeCoroutine(player, message, fadeInTime, displayTime, fadeOutTime));
        }

        private static IEnumerator<float> FadeCoroutine(Player player, string message, float fadeIn, float display, float fadeOut)
        {
            // Fade in (имитация через размер)
            for (float t = 0; t < fadeIn; t += 0.1f)
            {
                int size = (int)(20 + (t / fadeIn) * 10);
                player.ShowHint(Size(message, size), 0.15f);
                yield return Timing.WaitForSeconds(0.1f);
            }
            
            // Display
            player.ShowHint(Size(message, 30), display);
            yield return Timing.WaitForSeconds(display);
            
            // Fade out
            for (float t = 0; t < fadeOut; t += 0.1f)
            {
                int size = (int)(30 - (t / fadeOut) * 10);
                player.ShowHint(Size(message, size), 0.15f);
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        #endregion

        #region Text Formatting - Форматирование Текста

        /// <summary>
        /// Применяет цвет к тексту.
        /// </summary>
        public static string Color(string text, string color)
        {
            return $"<color={color}>{text}</color>";
        }

        /// <summary>
        /// Делает текст жирным.
        /// </summary>
        public static string Bold(string text)
        {
            return $"<b>{text}</b>";
        }

        /// <summary>
        /// Делает текст курсивом.
        /// </summary>
        public static string Italic(string text)
        {
            return $"<i>{text}</i>";
        }

        /// <summary>
        /// Подчёркивает текст.
        /// </summary>
        public static string Underline(string text)
        {
            return $"<u>{text}</u>";
        }

        /// <summary>
        /// Перечёркивает текст.
        /// </summary>
        public static string Strikethrough(string text)
        {
            return $"<s>{text}</s>";
        }

        /// <summary>
        /// Устанавливает размер текста.
        /// </summary>
        public static string Size(string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }

        /// <summary>
        /// Выравнивает текст.
        /// </summary>
        public static string Align(string text, string alignment)
        {
            return $"<align={alignment}>{text}</align>";
        }

        /// <summary>
        /// Центрирует текст.
        /// </summary>
        public static string Center(string text)
        {
            return Align(text, "center");
        }

        /// <summary>
        /// Добавляет отступ сверху (через переносы строк).
        /// </summary>
        public static string TopMargin(string text, int lines = 1)
        {
            return new string('\n', lines) + text;
        }

        /// <summary>
        /// Создаёт разделитель.
        /// </summary>
        public static string Separator(int length = 30, char c = '─')
        {
            return new string(c, length);
        }

        #endregion

        #region Builder - Строитель Хинтов

        /// <summary>
        /// Создаёт новый построитель хинтов.
        /// </summary>
        public static HintBuilder Builder()
        {
            return new HintBuilder();
        }

        /// <summary>
        /// Построитель для создания сложных хинтов.
        /// </summary>
        public class HintBuilder
        {
            private readonly StringBuilder _sb = new StringBuilder();
            private float _duration = 5f;

            public HintBuilder Line(string text)
            {
                _sb.AppendLine(text);
                return this;
            }

            public HintBuilder ColorLine(string text, string color)
            {
                _sb.AppendLine(Color(text, color));
                return this;
            }

            public HintBuilder Title(string text, string color = White)
            {
                _sb.AppendLine(Bold(Size(Color(text, color), 35)));
                return this;
            }

            public HintBuilder Subtitle(string text, string color = Gray)
            {
                _sb.AppendLine(Italic(Color(text, color)));
                return this;
            }

            public HintBuilder Empty()
            {
                _sb.AppendLine();
                return this;
            }

            public HintBuilder Divider(int length = 30)
            {
                _sb.AppendLine(Color(Separator(length), Gray));
                return this;
            }

            public HintBuilder Bullet(string text)
            {
                _sb.AppendLine($"• {text}");
                return this;
            }

            public HintBuilder Number(int num, string text)
            {
                _sb.AppendLine($"{num}. {text}");
                return this;
            }

            public HintBuilder WithDuration(float duration)
            {
                _duration = duration;
                return this;
            }

            public string Build()
            {
                return _sb.ToString().TrimEnd();
            }

            public void SendTo(Player player)
            {
                player.ShowHint(Build(), _duration);
            }

            public void SendToAll()
            {
                var hint = Build();
                foreach (var player in Player.List)
                {
                    player.ShowHint(hint, _duration);
                }
            }
        }

        #endregion
    }
}
