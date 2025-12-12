using System.Windows;
using System.Windows.Controls;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class CreateActivityView : UserControl
{
    public CreateActivityView()
    {
        InitializeComponent();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Settings);
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Settings);
    }
}
