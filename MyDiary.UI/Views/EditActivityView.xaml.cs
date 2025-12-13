using System.Windows;
using System.Windows.Controls;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class EditActivityView : UserControl
{
    private readonly ActivityEditModel? _activity;

    public EditActivityView(object? parameter)
    {
        InitializeComponent();

        _activity = parameter as ActivityEditModel;

        if (_activity is not null)
        {
            TitleBox.Text = _activity.Title;
            DescriptionBox.Text = _activity.Description;
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Settings);
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Settings);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Settings);
    }
}
