﻿//#define TRIAL_SIMULATION
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DjvuApp.Common;
using DjvuApp.Misc;
using DjvuApp.Misc.TrialExperience;
using Microsoft.Practices.ServiceLocation;

namespace DjvuApp.Pages
{
    public sealed partial class MainPage : Page
    {
        private static bool? _hasLicense = null;

        private readonly NavigationHelper _navigationHelper;
        private readonly ResourceLoader _resourceLoader;

        public MainPage()
        {
            InitializeComponent();
            _navigationHelper = new NavigationHelper(this);
            _resourceLoader = ResourceLoader.GetForCurrentView();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedTo(e);

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            if (_hasLicense == null)
            {
                _hasLicense = await LicenseValidator.GetLicenseStatusAsync();
            }
            if (_hasLicense != true)
            {
                var dialog = new MessageDialog("I don't like being pirated...");
                await dialog.ShowAsync();
                Application.Current.Exit();
            }

            UpdateLicenseStatus();

            ShowRateAppDialog();
        }

        private async void ShowRateAppDialog()
        {
            const string key = "RateDialog_AppLaunchCount";
            var settings = ApplicationData.Current.LocalSettings;
            if (!settings.Values.ContainsKey(key))
            {
                settings.Values[key] = 0;
            }

            var count = (int) settings.Values[key];
            
            settings.Values[key] = count + 1;
            count++;

            if (count == 5 || count == 10 || count == 15)
            {
                var content = _resourceLoader.GetString("RateAppDialog_Content");
                var title = _resourceLoader.GetString("RateAppDialog_Title");
                var rateButtonCaption = _resourceLoader.GetString("RateAppDialog_RateButton_Caption");
                var cancelButtonCaption = _resourceLoader.GetString("RateAppDialog_CancelButton_Caption");
                var dialog = new MessageDialog(content, title);

                dialog.Commands.Add(new UICommand(rateButtonCaption, async command =>
                {
                    settings.Values[key] = 1000;
                    await App.RateApp();
                }));
                dialog.Commands.Add(new UICommand(cancelButtonCaption));
                dialog.CancelCommandIndex = unchecked((uint) -1);

                await dialog.ShowAsync();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }
        
        private async void UpdateLicenseStatus()
        {
#if TRIAL_SIMULATION
            var isTrial = true;
#else
            var licenseInformation = CurrentApp.LicenseInformation;
            var isTrial = licenseInformation.IsTrial;
#endif
            

            if (isTrial)
            {
                buyAppBarButton.Visibility = Visibility.Visible;

                DateTimeOffset expirationDate;
                try
                {
                    var trialService = ServiceLocator.Current.GetInstance<ITrialService>();
                    expirationDate = await trialService.GetExpirationDate<DjvuReaderUserInfo>();
                }
                catch (Exception)
                {
                    ShowLicenseCheckErrorMessage();
                    return;
                }
                
                var isExpired = expirationDate < DateTimeOffset.Now;

                if (!isExpired)
                {
                    var trialTimeLeft = expirationDate - DateTimeOffset.Now;
                    var formatString = _resourceLoader.GetString("TrialNotificationFormat");
                    trialExpirationTextBlock.Text = string.Format(formatString, Math.Ceiling(trialTimeLeft.TotalDays));
                }
                else
                {
                    await ShowExpirationMessage();
                }
            }
        }

        private async Task ShowExpirationMessage()
        {
            var title = _resourceLoader.GetString("ExpirationDialog_Title");
            var content = _resourceLoader.GetString("ExpirationDialog_Content");
            var buyButtonCaption = _resourceLoader.GetString("ExpirationDialog_BuyButton_Caption");
            var dialog = new MessageDialog(content, title);
            dialog.Commands.Add(new UICommand(buyButtonCaption, async command =>
            {
                await CurrentApp.RequestAppPurchaseAsync(false);
                    UpdateLicenseStatus();
            }));
            var result = await dialog.ShowAsync();
            if (result == null)
                Application.Current.Exit();
        }

        private async void ShowLicenseCheckErrorMessage()
        {
            var title = _resourceLoader.GetString("LicenseCheckErrorDialog_Title");
            var content = _resourceLoader.GetString("LicenseCheckErrorDialog_Content");
            var exitButtonCaption = _resourceLoader.GetString("LicenseCheckErrorDialog_ExitButton_Caption");
            var dialog = new MessageDialog(content, title);
            dialog.Commands.Add(new UICommand(exitButtonCaption, null));
            await dialog.ShowAsync();
            Application.Current.Exit();
        }

        private void ItemClickHandler(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof (ViewerPage), e.ClickedItem);
        }

        private void AboutButtonClickHandler(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (AboutPage));
        }

        private async void BuyButtonClickHandler(object sender, RoutedEventArgs e)
        {
            await CurrentApp.RequestAppPurchaseAsync(false);
        }
    }
}