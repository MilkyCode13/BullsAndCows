using System;
using System.Collections.Generic;

namespace BullsAndCows
{
    /// <summary>
    /// Main class of the program.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// <c>Random</c> object used for pseudo-random number generation.
        /// </summary>
        private static Random s_rand;

        /// <summary>
        /// Static initializer of <c>Program</c> class. It initializes all static fields of the class.
        /// </summary>
        static Program()
        {
            s_rand = new Random();
        }

        /// <summary>
        /// Main entrypoint of a program.
        /// </summary>
        private static void Main()
        {
            // Welcome the user.
            Console.WriteLine("Добро пожаловать в игру Быки и коровы! Это известная игра на отгадывание шифра.");

            // Main loop.
            var exit = false;
            while (!exit)
            {
                // Prompt user for new game, help or exit and get user input.
                Console.Write("Вы хотите начать новую игру (n), показать справку (h) или выйти (x)? ");
                string input = Console.ReadLine();
                switch (input)
                {
                    // New game.
                    case "n":
                    case "new":
                        RunGame();
                        break;

                    // Show help.
                    case "h":
                    case "help":
                        ShowHelp();
                        break;

                    // Exit.
                    case "x":
                    case "exit":
                        exit = true;
                        Console.WriteLine("Пока!");
                        break;

                    // Invalid input.
                    default:
                        Console.WriteLine("Неверный ввод");
                        break;
                }
            }
        }

        /// <summary>
        /// Shows main information about the game.
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine(
                @"Компьютер генерирует секретное число, все цифры которого различны и которое не начинается с нуля.
Задача игрока - отгадать это число. Каждый раз, когда игрок делает свою догадку, компьютер выводит
количество ""быков"" и ""коров"". Количество ""быков"" соответствует количеству отгаданных цифр,
находящихся на верных позициях в то время, когда ""коровы"" - отгаданные цифры, находящиеся
на других позициях.

Пример (4 цифры в числе):
Секретное число: 4271
Догадка игрока:  1234
Вывод:           1 бык и 2 коровы
(Бык - 2, коровы - 1 и 4)");
        }

        /// <summary>
        /// Runs a game.
        /// </summary>
        private static void RunGame()
        {
            // Get number length from user.
            int numberLength = GetNumberLength();

            // Generate number of selected length suitable for guess.
            // Number is stored as string because it makes dealing with digits easier and it is only method that allows
            // generating big numbers with needed properties reliably. 
            string number = GenerateNumber(numberLength);

            bool guessed;
            do
            {
                // Get user guess.
                string guess = GetUserGuess(numberLength, out bool exit);

                // If user decided to exit, do so.
                if (exit)
                {
                    Console.WriteLine($"Жаль! Загаданное число: {number}");
                    return;
                }

                // Count cows and bulls; if guess is fully correct, set guessed to true.
                guessed = CountBullsAndCows(number, guess, out int bulls, out int cows);

                // If not guessed, tell user number of bulls and cows.
                if (!guessed)
                {
                    // Defining Russian singular and plural forms of "bull" and "cow".
                    string[] bullForms = {"бык", "быка", "быков"};
                    string[] cowForms = {"корова", "коровы", "коров"};
                    Console.WriteLine($"{MakePlural(bullForms, bulls)}, {MakePlural(cowForms, cows)}");
                }
            } while (!guessed);

            Console.WriteLine("Поздравляем, вы угадали число!");
        }

        /// <summary>
        /// Makes valid plural forms in Russian.
        /// </summary>
        /// <param name="wordForms">Array of plural forms: singular, specific plural and plural.</param>
        /// <param name="number">Number for which to make plural.</param>
        /// <returns>Number with corresponding valid plural form.</returns>
        /// <exception cref="ArgumentException">Array is not in correct format.</exception>
        private static string MakePlural(string[] wordForms, int number)
        {
            // Assert that wordForms parameter is in correct format.
            if (wordForms.Length != 3)
            {
                throw new ArgumentException("Wrong word forms count");
            }

            // Singular: ends on 1 and not between 10 and 20.
            if (number % 10 == 1 && (number % 100 is < 10 or > 20))
            {
                return $"{number} {wordForms[0]}";
            }

            // Specific plural: ends on 2, 3 or 4 and not between 10 and 20.
            if ((number % 10 is > 1 and < 5) && (number % 100 is < 10 or > 20))
            {
                return $"{number} {wordForms[1]}";
            }

            // Plural: all other cases.
            return $"{number} {wordForms[2]}";
        }

        /// <summary>
        /// Gets a valid number length from user.
        /// </summary>
        /// <returns>Number length.</returns>
        private static int GetNumberLength()
        {
            bool numberOk;
            int numberLength;
            do
            {
                Console.Write("Введите длину числа (1-10, нажмите Enter для 4): ");
                // Get user input.
                string input = Console.ReadLine();

                // Empty input: using default value.
                if (input == "")
                {
                    numberLength = 4;
                    numberOk = true;
                }
                else
                {
                    // Trying to parse entered number.
                    numberOk = int.TryParse(input, out numberLength);

                    // Number is out of range.
                    if (numberOk && numberLength is < 1 or > 10)
                    {
                        numberOk = false;
                    }
                }

                // Notify user if input is invalid.
                if (!numberOk)
                {
                    Console.WriteLine("Неверный ввод, необходимо число от 1 до 10.");
                }
            } while (!numberOk);

            return numberLength;
        }

        /// <summary>
        /// Gets a valid guess from a user.
        /// </summary>
        /// <param name="numberLength">Required number length.</param>
        /// <param name="exit">Output parameter, returns <c>true</c> if user wants to exit,
        /// <c>false</c> otherwise.</param>
        /// <returns>User guess as a string.</returns>
        private static string GetUserGuess(int numberLength, out bool exit)
        {
            string guess;
            string validationResult;
            do
            {
                // Get user input.
                Console.Write("Введите вашу догадку (-1, чтобы закончить игру): ");
                guess = Console.ReadLine();

                // If user decided to exit.
                if (guess == "-1")
                {
                    exit = true;
                    return null;
                }

                exit = false;

                // Validate user input.
                validationResult = ValidateNumberInput(guess, numberLength);
                // If input is invalid, tell user what is wrong.
                if (validationResult != null)
                {
                    Console.WriteLine(validationResult);
                }
            } while (validationResult != null);

            return guess;
        }

        /// <summary>
        /// Validates number input as for game.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="numberLength">Required number length.</param>
        /// <returns><c>null</c> if number is valid, error message otherwise.</returns>
        private static string ValidateNumberInput(string input, int numberLength)
        {
            // Assert that string is numeric and number is non-negative.
            if (!ulong.TryParse(input, out _) || input[0] == '-')
            {
                return "Ваш ввод не является неотрицательным числом!";
            }

            // Assert that number has appropriate length.
            if (input.Length != numberLength)
            {
                return "У вашего числа неверная длина!";
            }

            // Assert that number begins with non-zero.
            if (input[0] == '0')
            {
                return "Ваше число начинается с нуля!";
            }

            // Assert that all digits are unique.
            for (var i = 0; i < numberLength - 1; i++)
            {
                for (var j = i + 1; j < numberLength; j++)
                {
                    if (input[i] == input[j])
                    {
                        return "В числе имеются повторяющиеся цифры!";
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Generates random number of specified length, all digits are different, begins with any digit except 0.
        /// Number is returned in string representation.
        /// </summary>
        /// <param name="numberLength">Number of digits in number, must be between 1 and 10.</param>
        /// <returns>Returns number in string representation.</returns>
        /// <exception cref="ArgumentException"></exception>
        private static string GenerateNumber(int numberLength)
        {
            // If number length is out of range, throw exception.
            if (numberLength is < 1 or > 10)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberLength), "Number length must be between 1 and 10.");
            }

            // List of every decimal digit.
            var digitsLeft = new List<char> {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};

            // Generate first digit between 1 and 9 (number must not begin with 0).
            int index = s_rand.Next(1, 9);
            string number = digitsLeft[index].ToString();
            // Remove generated digit from list as used (digits must be unique within a number).
            digitsLeft.RemoveAt(index);

            // Generate the rest of numbers.
            for (var i = 1; i < numberLength; i++)
            {
                index = s_rand.Next(10 - i);
                number += digitsLeft[index];
                digitsLeft.RemoveAt(index);
            }

            return number;
        }

        /// <summary>
        /// Counts "bulls" and "cows" of guess, also returning if guess is correct.
        /// </summary>
        /// <param name="number">Secret number as string.</param>
        /// <param name="guess">User guess as string.</param>
        /// <param name="bulls">Number of bulls counted.</param>
        /// <param name="cows">Number of cows counted.</param>
        /// <returns><c>true</c> if guess is correct, <c>false</c> otherwise.</returns>
        private static bool CountBullsAndCows(string number, string guess, out int bulls, out int cows)
        {
            // If guess is correct, return true and all bulls.
            if (guess == number)
            {
                bulls = number.Length;
                cows = 0;
                return true;
            }

            bulls = 0;
            cows = 0;

            // Check every digit of guess.
            for (var i = 0; i < number.Length; i++)
            {
                // If digit is in place, increase bulls counter.
                if (guess[i] == number[i])
                {
                    bulls++;
                }
                // If digit is not in place, but is present in number, increase cows counter.
                else if (number.Contains(guess[i]))
                {
                    cows++;
                }
            }

            return false;
        }
    }
}