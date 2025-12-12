using System;
using System.Windows;
using System.Windows.Controls;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class EntryDetailsView : UserControl
{
    private readonly EntryPreview? _entry;

    public EntryDetailsView(object? parameter)
    {
        InitializeComponent();

        _entry = parameter as EntryPreview;

        TitleText.Text = _entry?.Title ?? "Запись";
        MetaText.Text = _entry is null ? "" : $"{_entry.Mood} • {_entry.CreatedAt:dd.MM.yyyy HH:mm}";
        ContentText.Text = _entry?.Summary ?? "";
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Entries);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Entries);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Entries);
    }
}
