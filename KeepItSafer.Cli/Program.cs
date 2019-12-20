using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using KeepItSafer.Crypto;

namespace KeepItSafer.Cli
{
    public class Program
    {
        private static string masterPassword;

        public static void Main(string[] args)
        {
            var passwordDb = args.Length > 0 ? args[0] : null;

            var firstPass = true;
            while (true)
            {
                if (!string.IsNullOrWhiteSpace(passwordDb) && File.Exists(passwordDb))
                {
                    break;
                }
                if (firstPass || !string.IsNullOrWhiteSpace(passwordDb))
                {
                    Console.WriteLine($"Password DB '{passwordDb}' does not exist, please try again.");
                }
                firstPass = false;

                Console.Write("Password DB: ");
                passwordDb = Console.ReadLine();
            }

            Console.WriteLine($"Using password db {passwordDb}...");

            try
            {
                var passwordDbContent = File.ReadAllText(passwordDb);
                UsePasswordDb(JsonSerializer.Deserialize<PasswordDb>(passwordDbContent));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application error, exiting! {ex}");
                Environment.Exit(1);
            }
        }

        private static void UsePasswordDb(PasswordDb db)
        {
            while (true)
            {
                Console.WriteLine();

                Console.WriteLine("0: Exit");
                var menuItem = 1;
                var passwordGroups = db.PasswordGroups.OrderBy(pg => pg.GroupName).ToList();
                foreach (var passwordGroup in passwordGroups)
                {
                    Console.WriteLine($"{menuItem}: {passwordGroup.GroupName}");
                    menuItem++;
                }
                Console.WriteLine();
                Console.Write("Option: ");
                if (!int.TryParse(Console.ReadLine().Trim(), out var choice) ||
                    choice < 0 || choice >= menuItem)
                {
                    continue;
                }
                if (choice == 0)
                {
                    return;
                }
                UsePasswordGroup(db, passwordGroups[choice - 1]);
            }
        }

        private static void UsePasswordGroup(PasswordDb db, PasswordGroup passwordGroup)
        {
            while (true)
            {
                Console.WriteLine();

                Console.WriteLine($"Group: {passwordGroup.GroupName}");
                Console.WriteLine("0: Back to main menu");
                var menuItem = 1;
                var entries = passwordGroup.PasswordEntries.OrderBy(pe => pe.Name).ToList();
                foreach (var entry in entries)
                {
                    var entryValue = entry.IsValueEncrypted ? "****" : entry.Value;
                    Console.WriteLine($"{menuItem}: {entry.Name}\t{entryValue}");
                    menuItem++;
                }
                Console.WriteLine();
                Console.Write("Option: ");
                if (!int.TryParse(Console.ReadLine().Trim(), out var choice) ||
                    choice < 0 || choice >= menuItem)
                {
                    continue;
                }
                if (choice == 0)
                {
                    return;
                }
                DecryptPassword(db, passwordGroup, entries[choice - 1]);                
            }
        }

        private static void DecryptPassword(PasswordDb db, PasswordGroup passwordGroup, PasswordEntry passwordEntry)
        {
            while (true)
            {
                if (!passwordEntry.IsValueEncrypted || masterPassword != null)
                {
                    break;
                }
                Console.WriteLine();
                Console.Write("Enter master password: ");
                var enteredMasterPassword = ReadPasswordFromConsole();
                using (var secure = new Secure())
                {
                    if (!secure.ValidateHash(enteredMasterPassword, db.MasterPassword))
                    {
                        Console.WriteLine("Invalid master password entered, please try again.");
                        continue;
                    }
                    masterPassword = enteredMasterPassword;
                    break;
                }                
            }

            Console.WriteLine();
            Console.WriteLine($"Group: {passwordGroup.GroupName}");
            Console.WriteLine($"Name: {passwordEntry.Name}");
            var passwordValue = passwordEntry.Value;
            if (passwordEntry.IsValueEncrypted)
            {
                using (var secure = new Secure())
                {
                    passwordValue = secure.Decrypt(masterPassword, db.IV, passwordEntry.Salt, passwordValue);
                }
            }
            Console.WriteLine($"Value: {passwordValue}");
            Console.WriteLine();
            Console.WriteLine("Press return to continue...");
            Console.ReadLine();
        }

        private static string ReadPasswordFromConsole()
        {
            string pass = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return pass;
        }
    }
}