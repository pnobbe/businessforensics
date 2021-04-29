using System;
using System.IO;
using Guesser.Core.Interfaces.Repositories;
using Guesser.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Guesser.Client
{
    class Program
    {
        private static readonly Random _random = new Random();
        private static readonly int DEFAULT_MIN = 0;
        private static readonly int DEFAULT_MAX = 10;
        private static readonly int DEFAULT_MAX_ATTEMPTS = 3;


        static void Main(string[] args)
        {
            // Setup our DI and configuration
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var host = AppStartup();
            var repository = ActivatorUtilities.CreateInstance<LoggingRepository>(host.Services);

            var min = config.GetSection("GameSettings").GetValue<int>("MinRange", DEFAULT_MIN);
            var max = config.GetSection("GameSettings").GetValue<int>("MaxRange", DEFAULT_MAX);
            var remainingAttempts = config.GetSection("GameSettings").GetValue<int>("MaxAttempts", DEFAULT_MAX_ATTEMPTS);

            var target = _random.Next(min, max + 1); // Random.Next has an inclusive lower/exclusive upper bound.

            // Display welcome message.
            Console.WriteLine("Guess a number between {0} and {1}!\r", min, max);
            Console.WriteLine("Type your guess and then press Enter");

            // Ask the user to take a guess.
            while (remainingAttempts > 0)
            {
                Console.WriteLine("You have {0} attempt(s) remaining", remainingAttempts);

                int guess;
                try
                {
                    // Parse user input.
                    guess = Convert.ToInt32(Console.ReadLine());

                    if (guess > max || guess < min)
                    {
                        Console.WriteLine("Please try again with a number between {0} and {1}", min, max);
                        continue;
                    }
                }
                catch (Exception e) when (e is FormatException or OverflowException)
                {
                    Console.WriteLine("Incorrect number, please try again with a number between {0} and {1}", min, max);
                    continue;
                }

                remainingAttempts--;
                repository.CreateLogEntry(string.Format("User guessed {0} while attempting to guess {1}. Remaining attempts: {2}. Correct: {3}", guess, target, remainingAttempts, guess == target));


                if (guess != target)
                    continue;

                Console.WriteLine("Your guess was correct, congratulations!");
                repository.CreateLogEntry(string.Format("User correctly guessed {0} in {1} attempt(s)!", target, 3 - remainingAttempts));
                return;
            }

            Console.WriteLine("You've run out of attempts, sorry :(");
            repository.CreateLogEntry(string.Format("User ran out of attempts while trying to guess {0}. Too bad!", target));
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            // Check the current directory that the application is running on
            // Then once the file 'appsetting.json' is found, we are adding it.
            // We add env variables, which can override the configs in appsettings.json
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        static IHost AppStartup()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            // Specifying the configuration for serilog
            Log.Logger = new LoggerConfiguration() // initiate the logger configuration
                            .ReadFrom.Configuration(builder.Build()) // connect serilog to our configuration folder
                            .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog
                            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Hour) // decide where the logs are going to be shown
                            .CreateLogger(); //initialise the logger

            Log.Logger.Information("Application Starting");

            var host = Host.CreateDefaultBuilder() // Initialising the Host
                        .ConfigureServices((context, services) =>
                        { // Adding the DI container for configuration
                            services.AddScoped<ILoggingRepository, LoggingRepository>();
                        })
                        .UseSerilog() // Add Serilog
                        .Build(); // Build the Host

            return host;
        }
    }
}
