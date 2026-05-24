using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Nexus
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        public IServiceProvider Services { get; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            services.AddDbContext<NexusDbContext>(options =>
                options.UseSqlite($"Data Source={AppDb.DbPath}"));
            services.AddSingleton<AppDbInitializer>();
            services.AddSingleton<NexusDataService>(sp =>
                new NexusDataService(AppDb.CreateOptions()));
            services.AddTransient<Nexus.ViewModels.HomeViewModel>();
            services.AddTransient<Nexus.ViewModels.ExploreViewModel>();
            services.AddTransient<Nexus.ViewModels.NotificationsViewModel>();
            services.AddTransient<Nexus.ViewModels.MessagesViewModel>();
            services.AddTransient<Nexus.ViewModels.ProfileViewModel>();
            services.AddTransient<Nexus.ViewModels.BookmarksViewModel>();
            services.AddTransient<Nexus.ViewModels.SettingsViewModel>();
            services.AddTransient<Nexus.ViewModels.ShellViewModel>();
            services.AddTransient<Nexus.ViewModels.LoginViewModel>();
            services.AddTransient<Nexus.ViewModels.RegisterViewModel>();

            Services = services.BuildServiceProvider();

            this.UnhandledException += (s, e) =>
            {
                WriteSafeErrorLog("error_global.txt", e.Exception);
                e.Handled = true;
            };
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                var initializer = Services.GetRequiredService<AppDbInitializer>();
                initializer.Initialize();

                // Zawsze startuj na ekranie logowania.
                ShowAuthWindow();
            }
            catch (Exception ex)
            {
                WriteSafeErrorLog("launch.txt", ex);
                throw;
            }
        }

        /// <summary>
        /// Otwiera okno logowania i zamyka aktualne (jeśli było). Wywoływane przy starcie
        /// i po wylogowaniu / usunięciu konta.
        /// </summary>
        public void ShowAuthWindow()
        {
            var oldWindow = _window;
            _window = new AuthWindow();
            _window.Activate();
            oldWindow?.Close();
        }

        /// <summary>
        /// Otwiera główne okno aplikacji i zamyka okno autoryzacji. Wywoływane po
        /// pomyślnym logowaniu (LoginViewModel.LoginSucceeded).
        /// </summary>
        public void ShowMainWindow()
        {
            var dataService = Services.GetRequiredService<NexusDataService>();
            var currentUserId = dataService.GetCurrentUserId();

            if (currentUserId > 0)
            {
                var userSettings = dataService.GetUserSettings(currentUserId);
                ApplyTheme(userSettings.Theme);
            }

            var oldWindow = _window;
            _window = new MainWindow();
            _window.Activate();

            // Wymuś motyw na drzewie wizualnym (sam swap merged dictionary nie odświeża
            // już wyrenderowanych kontrolek; ElementTheme tak).
            if (currentUserId > 0 && _window.Content is FrameworkElement root)
            {
                var settings = dataService.GetUserSettings(currentUserId);
                root.RequestedTheme = ResolveElementTheme(settings.Theme);
            }

            oldWindow?.Close();
        }

        public void ApplyTheme(string theme)
        {
            var resourceUri = theme switch
            {
                "Light" => new Uri("ms-appx:///Themes/LightTheme.xaml"),
                "Dark" => new Uri("ms-appx:///Themes/DarkTheme.xaml"),
                _ => new Uri("ms-appx:///Themes/DarkTheme.xaml"),
            };

            // Usuń stary słownik motywu (zostawiając konwertery i XamlControlsResources)
            var existingThemes = Resources.MergedDictionaries
                .Where(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"))
                .ToList();

            foreach (var t in existingThemes)
            {
                Resources.MergedDictionaries.Remove(t);
            }

            var resourceDict = new ResourceDictionary { Source = resourceUri };
            Resources.MergedDictionaries.Add(resourceDict);

            // Wymuś re-temowanie aktualnego drzewa wizualnego
            if (_window?.Content is FrameworkElement root)
            {
                root.RequestedTheme = ResolveElementTheme(theme);
            }
        }

        private static ElementTheme ResolveElementTheme(string theme) => theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };

        private static void WriteSafeErrorLog(string fileName, Exception ex)
        {
            try
            {
                var dir = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                var path = System.IO.Path.Combine(dir, fileName);
                File.AppendAllText(path,
                    $"--- {DateTime.UtcNow:O} ---{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
            }
            catch
            {
                // Last resort — silently swallow, nigdy nie wyrzucamy z handlera błędów.
            }
        }
    }
}
