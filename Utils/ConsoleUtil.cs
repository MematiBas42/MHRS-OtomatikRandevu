using System;
using System.Text;
using System.Threading;

namespace MHRS_OtomatikRandevu.Utils
{
    public static class ConsoleUtil
    {
        public static void WriteText(string text, int delay)
        {
            Console.WriteLine(text);
            Thread.Sleep(delay);
        }

        public static string ReadPassword()
        {
            var pass = new StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace tuşuna basıldığında
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass.Remove(pass.Length - 1, 1);
                    Console.Write("\b \b"); // İmleci geri al, boşluk yaz, imleci tekrar geri al
                }
                // Geçerli bir karakter girildiğinde (Enter veya Backspace değil)
                else if (!char.IsControl(key.KeyChar))
                {
                    pass.Append(key.KeyChar);
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine(); // Yeni satıra geç
            return pass.ToString();
        }
    }
}
