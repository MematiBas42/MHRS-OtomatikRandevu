// Dosya Yolu: Utils/Logger.cs
#nullable enable
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace MHRS_OtomatikRandevu.Utils
{
    public enum LogLevel
    {
        INFO,
        PROMPT,
        INPUT,
        OUTPUT,
        WARN,
        ERROR,
        API_REQUEST,
        API_RESPONSE_SUCCESS,
        API_RESPONSE_FAIL
    }

    public static class Logger
    {
        private static bool _isLoggingEnabled;
        private static string _logFilePath = Path.Combine(AppContext.BaseDirectory, "mhrs_automator_log_generic.txt");
        private static readonly object _lock = new object();
        public static volatile bool IsExiting = false;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void Initialize(IConfiguration configuration, string tcKimlikNo)
        {
            try
            {
                _isLoggingEnabled = configuration.GetValue<bool>("isLogging");
            }
            catch (Exception ex)
            {
                _isLoggingEnabled = false;
                Console.WriteLine($"!!! CONFIG OKUMA HATASI: Loglama devre dışı bırakıldı. Hata: {ex.Message} !!!");
            }

            if (!_isLoggingEnabled || string.IsNullOrWhiteSpace(tcKimlikNo)) return;
            
            string oldLogPath = _logFilePath;
            _logFilePath = Path.Combine(AppContext.BaseDirectory, $"mhrs_automator_log_{tcKimlikNo}.txt");

            lock (_lock)
            {
                if (File.Exists(oldLogPath) && oldLogPath != _logFilePath)
                {
                    try
                    {
                        string initialLogs = File.ReadAllText(oldLogPath);
                        File.AppendAllText(_logFilePath, initialLogs);
                        File.Delete(oldLogPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Eski log dosyası taşınırken hata: {ex.Message}");
                    }
                }
            }

            Log(LogLevel.INFO, $"Logger, T.C. kimlik numarasına özel loglama yapacak şekilde ayarlandı. Log dosyası: {_logFilePath}");
        }

        private static void Log(LogLevel level, string message)
        {
            if (!_isLoggingEnabled)
            {
                return;
            }

            try
            {
                lock (_lock)
                {
                    using (StreamWriter sw = File.AppendText(_logFilePath))
                    {
                        sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] - {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! LOGLAMA HATASI !!! - {ex.Message}");
            }
        }

        public static void Info(string message) => Log(LogLevel.INFO, message);
        
        public static void Warn(string message) => Log(LogLevel.WARN, message);

        public static void Error(string message, Exception? ex = null)
        {
            string logMessage = message;
            if (ex != null)
            {
                logMessage += $"\n--- EXCEPTION DETAILS ---\n{ex}\n-------------------------";
            }
            Log(LogLevel.ERROR, logMessage);
        }

        public static void WriteLineAndLog(string message)
        {
            Console.WriteLine(message);
            Log(LogLevel.OUTPUT, message);
        }

        public static void WriteTextAndLog(string message, int sleep = 0)
        {
            ConsoleUtil.WriteText(message, sleep);
            Log(LogLevel.OUTPUT, $"WriteText: {message}");
        }

        public static string? ReadLineAndLog(string promptToShow, bool isPassword = false)
        {
            if (IsExiting) return null;

            Console.Write(promptToShow);
            Log(LogLevel.PROMPT, promptToShow);

            string? input;
            if (isPassword)
            {
                input = ConsoleUtil.ReadPassword();
            }
            else
            {
                input = Console.ReadLine();
            }

            if (input == null || IsExiting)
            {
                return null;
            }

            if (isPassword)
            {
                Log(LogLevel.INPUT, "**********");
            }
            else
            {
                Log(LogLevel.INPUT, input);
            }

            return input;
        }

        public static void LogObject(LogLevel level, object? data, string? description = null)
        {
            if (!_isLoggingEnabled) return;
            
            if (data == null)
            {
                Log(level, $"{description ?? "Object"}: (null)");
                return;
            }
            
            string message = "";
            if (!string.IsNullOrEmpty(description))
            {
                message += $"{description}\n";
            }
            try
            {
                message += JsonSerializer.Serialize(data, _jsonOptions);
            }
            catch (Exception ex)
            {
                message += $"[Serialization Error] Nesne JSON'a dönüştürülemedi: {ex.Message}";
            }
            
            Log(level, message);
        }
    }
}