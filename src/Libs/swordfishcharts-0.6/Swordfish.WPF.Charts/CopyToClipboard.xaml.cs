#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using

using System;
using System.Windows;
using System.Windows.Input;

#endregion

namespace Swordfish.WPF.Charts
{
    /// <summary>
    /// Interaction logic for UIElementToClipboard.xaml
    /// </summary>
    public partial class CopyToClipboard
    {
        #region Delegates

        public delegate void CopyToClipboardDelegateType(FrameworkElement target, double width, double height);

        #endregion

        public static readonly DependencyProperty CopyTargetProperty =
            DependencyProperty.Register("CopyTarget", typeof (FrameworkElement), typeof (CopyToClipboard),
                                        new UIPropertyMetadata(null));

        public CopyToClipboardDelegateType CopyToClipboardDelegate;
        public CopyToClipboardDelegateType SaveToFileDelegate;

        public CopyToClipboard()
        {
            InitializeComponent();
            copyOptions.Visibility = Visibility.Collapsed;
            MouseEnter += UIElementToClipboard_MouseEnter;
            MouseLeave += UIElementToClipboard_MouseLeave;
            CopyToClipboardDelegate = ChartUtilities.CopyFrameworkElementToClipboard;
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...

        public FrameworkElement CopyTarget
        {
            get { return (FrameworkElement) GetValue(CopyTargetProperty); }
            set { SetValue(CopyTargetProperty, value); }
        }

        private void OnCopyToClipboard(double width, double height)
        {
            try
            {
                if (saveToFile.IsChecked == true)
                {
                    SaveToFileDelegate(CopyTarget, width, height);
                }
                else
                {
                    CopyToClipboardDelegate(CopyTarget, width, height);
                }
            }
            catch (Exception)
            {
            }
        }

        private void UIElementToClipboard_MouseLeave(object sender, MouseEventArgs e)
        {
            copyOptions.Visibility = Visibility.Collapsed;
            Margin = new Thickness(0, 0, 0, 0);
        }

        private void UIElementToClipboard_MouseEnter(object sender, MouseEventArgs e)
        {
            copyOptions.Visibility = Visibility.Visible;
            Margin = new Thickness(0, 0, 0, 8);
        }

        private void bCopy640x480_Click(object sender, RoutedEventArgs e)
        {
            OnCopyToClipboard(640, 480);
        }

        private void bCopy800x600_Click(object sender, RoutedEventArgs e)
        {
            OnCopyToClipboard(800, 600);
        }

        private void bCopy1024x768_Click(object sender, RoutedEventArgs e)
        {
            OnCopyToClipboard(1024, 768);
        }

        private void bCopy1280x1024_Click(object sender, RoutedEventArgs e)
        {
            OnCopyToClipboard(1280, 1024);
        }

        private void bCopyCustom_Click(object sender, RoutedEventArgs e)
        {
            double width;
            double height;
            Double.TryParse(tbWidth.Text, out width);
            Double.TryParse(tbHeight.Text, out height);
            OnCopyToClipboard(width, height);
        }
    }
}