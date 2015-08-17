﻿using System;
using Windows.ApplicationModel;
using Microsoft.Xaml.Interactivity;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace DjvuApp.Common
{
    public class StatusBarBehavior : DependencyObject, IBehavior
    {
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible",
            typeof(bool),
            typeof(StatusBarBehavior),
            new PropertyMetadata(false, OnIsVisibleChanged));

        public double BackgroundOpacity
        {
            get { return (double)GetValue(BackgroundOpacityProperty); }
            set { SetValue(BackgroundOpacityProperty, value); }
        }

        public static readonly DependencyProperty BackgroundOpacityProperty =
            DependencyProperty.Register("BackgroundOpacity",
            typeof(double),
            typeof(StatusBarBehavior),
            new PropertyMetadata(0d, OnOpacityChanged));

        public Color ForegroundColor
        {
            get { return (Color)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }

        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register("ForegroundColor",
            typeof(Color),
            typeof(StatusBarBehavior),
            new PropertyMetadata(null, OnForegroundColorChanged));

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor",
            typeof(Color),
            typeof(StatusBarBehavior),
            new PropertyMetadata(null, OnBackgroundChanged));

        private static bool isVisible = false;

        static StatusBarBehavior()
        {
            StatusBar.GetForCurrentView().Showing += (s, e) => isVisible = true;
            StatusBar.GetForCurrentView().Hiding += (s, e) => isVisible = false;
        }
        
        public void Attach(DependencyObject associatedObject)
        {
            //OnIsVisibleChanged(this, null);
        }

        public void Detach() { }

        public DependencyObject AssociatedObject { get; private set; }

        private static async void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (StatusBarBehavior) d;

            if (DesignMode.DesignModeEnabled)
                return;

            if (sender.IsVisible)
            {
                if (!isVisible)
                    await StatusBar.GetForCurrentView().ShowAsync();
            }
            else
            {
                await StatusBar.GetForCurrentView().HideAsync();
            }
        }
        private static void OnOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            StatusBar.GetForCurrentView().BackgroundOpacity = (double)e.NewValue;
        }

        private static void OnForegroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            StatusBar.GetForCurrentView().ForegroundColor = (Color)e.NewValue;
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            var behavior = (StatusBarBehavior)d;
            StatusBar.GetForCurrentView().BackgroundColor = behavior.BackgroundColor;

            // if they have not set the opacity, we need to so the new color is shown
            if (behavior.BackgroundOpacity == 0)
            {
                behavior.BackgroundOpacity = 1;
            }
        }
    }

}
