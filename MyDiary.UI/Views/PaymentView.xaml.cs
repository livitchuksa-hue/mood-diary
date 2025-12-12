using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class PaymentView : UserControl
{
    private readonly SubscriptionPlanSelection _plan;
    private bool _updatingMonth;

    public PaymentView(object? parameter)
    {
        InitializeComponent();

        _plan = parameter as SubscriptionPlanSelection ?? SubscriptionPlanSelection.For(SubscriptionPlanKind.Month);
        PlanTitleText.Text = _plan.Title;
        PlanPriceText.Text = _plan.PriceText;

        CardNumberBox.PreviewTextInput += DigitsOnly_PreviewTextInput;
        DataObject.AddPastingHandler(CardNumberBox, DigitsOnly_Pasting);

        CardExpiryMonthBox.PreviewTextInput += DigitsOnly_PreviewTextInput;
        DataObject.AddPastingHandler(CardExpiryMonthBox, DigitsOnly_Pasting);
        CardExpiryMonthBox.TextChanged += CardExpiryMonthBox_TextChanged;
        CardExpiryMonthBox.KeyDown += CardExpiryMonthBox_KeyDown;

        CardExpiryYearBox.PreviewTextInput += DigitsOnly_PreviewTextInput;
        DataObject.AddPastingHandler(CardExpiryYearBox, DigitsOnly_Pasting);
        CardExpiryYearBox.PreviewKeyDown += CardExpiryYearBox_PreviewKeyDown;

        CardCvcBox.PreviewTextInput += DigitsOnly_PreviewTextInput;
        DataObject.AddPastingHandler(CardCvcBox, DigitsOnly_Pasting);
    }

    private static void DigitsOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (!IsDigits(e.Text))
        {
            e.Handled = true;
        }
    }

    private void CardExpiryYearBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Back)
        {
            return;
        }

        if (CardExpiryYearBox.SelectionLength > 0)
        {
            return;
        }

        if (CardExpiryYearBox.Text.Length == 0)
        {
            CardExpiryMonthBox.Focus();
            CardExpiryMonthBox.CaretIndex = CardExpiryMonthBox.Text.Length;
            e.Handled = true;
        }
    }

    private static void DigitsOnly_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(typeof(string)))
        {
            e.CancelCommand();
            return;
        }

        var text = e.DataObject.GetData(typeof(string)) as string;
        if (string.IsNullOrEmpty(text) || !IsDigits(text))
        {
            e.CancelCommand();
        }
    }

    private static bool IsDigits(string value)
    {
        foreach (var ch in value)
        {
            if (!char.IsDigit(ch))
            {
                return false;
            }
        }

        return true;
    }

    private void CardExpiryMonthBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updatingMonth)
        {
            return;
        }

        if (CardExpiryMonthBox.Text.Length >= 2)
        {
            CardExpiryYearBox.Focus();
            CardExpiryYearBox.SelectAll();
        }
    }

    private void CardExpiryMonthBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (CardExpiryMonthBox.Text.Length == 1)
        {
            try
            {
                _updatingMonth = true;
                CardExpiryMonthBox.Text = "0" + CardExpiryMonthBox.Text;
                CardExpiryMonthBox.CaretIndex = CardExpiryMonthBox.Text.Length;
            }
            finally
            {
                _updatingMonth = false;
            }
        }

        if (CardExpiryMonthBox.Text.Length >= 2)
        {
            CardExpiryYearBox.Focus();
            CardExpiryYearBox.SelectAll();
            e.Handled = true;
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Subscription);
    }

    private void PayButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Entries);
    }
}
