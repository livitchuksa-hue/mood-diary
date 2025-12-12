using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class SubscriptionView : UserControl
{
    private bool _updatingSelection;

    public SubscriptionView()
    {
        InitializeComponent();

        PlanMonthToggle.Checked += PlanToggle_Checked;
        PlanMonthToggle.Unchecked += PlanToggle_Unchecked;
        PlanHalfYearToggle.Checked += PlanToggle_Checked;
        PlanHalfYearToggle.Unchecked += PlanToggle_Unchecked;
        PlanYearToggle.Checked += PlanToggle_Checked;
        PlanYearToggle.Unchecked += PlanToggle_Unchecked;

        if (PlanMonthToggle.IsChecked == false && PlanHalfYearToggle.IsChecked == false && PlanYearToggle.IsChecked == false)
        {
            PlanMonthToggle.IsChecked = true;
        }
    }

    private void PlanToggle_Checked(object sender, RoutedEventArgs e)
    {
        if (_updatingSelection)
        {
            return;
        }

        try
        {
            _updatingSelection = true;

            if (sender == PlanMonthToggle)
            {
                PlanHalfYearToggle.IsChecked = false;
                PlanYearToggle.IsChecked = false;
            }
            else if (sender == PlanHalfYearToggle)
            {
                PlanMonthToggle.IsChecked = false;
                PlanYearToggle.IsChecked = false;
            }
            else if (sender == PlanYearToggle)
            {
                PlanMonthToggle.IsChecked = false;
                PlanHalfYearToggle.IsChecked = false;
            }
        }
        finally
        {
            _updatingSelection = false;
        }
    }

    private void PlanToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_updatingSelection)
        {
            return;
        }

        if (PlanMonthToggle.IsChecked == false && PlanHalfYearToggle.IsChecked == false && PlanYearToggle.IsChecked == false)
        {
            try
            {
                _updatingSelection = true;

                if (sender is ToggleButton tb)
                {
                    tb.IsChecked = true;
                }
                else
                {
                    PlanMonthToggle.IsChecked = true;
                }
            }
            finally
            {
                _updatingSelection = false;
            }
        }
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        var kind = PlanYearToggle.IsChecked == true
            ? SubscriptionPlanKind.Year
            : PlanHalfYearToggle.IsChecked == true
                ? SubscriptionPlanKind.HalfYear
                : SubscriptionPlanKind.Month;

        UiServices.Navigation.Navigate(AppPage.Payment, SubscriptionPlanSelection.For(kind));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Login);
    }
}
