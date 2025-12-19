using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using MyDiary.Domain.Entities;
using MyDiary.Services.Payments;
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
        CardNumberBox.KeyDown += CardNumberBox_KeyDown;
        

        CardExpiryMonthBox.PreviewTextInput += DigitsOnly_PreviewTextInput;
        DataObject.AddPastingHandler(CardExpiryMonthBox, DigitsOnly_Pasting);
        CardExpiryMonthBox.TextChanged += CardExpiryMonthBox_TextChanged;
        CardExpiryMonthBox.PreviewKeyDown += CardExpiryMonthBox_PreviewKeyDown;
        CardExpiryMonthBox.KeyDown += CardExpiryMonthBox_KeyDown;

        CardExpiryYearBox.PreviewTextInput += DigitsOnly_PreviewTextInput;
        DataObject.AddPastingHandler(CardExpiryYearBox, DigitsOnly_Pasting);
        CardExpiryYearBox.PreviewKeyDown += CardExpiryYearBox_PreviewKeyDown;
        CardExpiryYearBox.KeyDown += CardExpiryYearBox_KeyDown;

        CardCvcBox.PreviewTextInput += DigitsOnly_PreviewTextInput;
        DataObject.AddPastingHandler(CardCvcBox, DigitsOnly_Pasting);
        CardCvcBox.PreviewKeyDown += CardCvcBox_PreviewKeyDown;
        CardCvcBox.KeyDown += CardCvcBox_KeyDown;
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
    private void CardExpiryMonthBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Back)
        {
            return;
        }

        if (CardExpiryMonthBox.SelectionLength > 0)
        {
            return;
        }

        if (CardExpiryMonthBox.Text.Length == 0)
        {
            CardNumberBox.Focus();
            CardNumberBox.CaretIndex = CardNumberBox.Text.Length;
            e.Handled = true;
        }
    }
    private void CardCvcBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Back)
        {
            return;
        }

        if (CardCvcBox.SelectionLength > 0)
        {
            return;
        }

        if (CardCvcBox.Text.Length == 0)
        {
            CardExpiryYearBox.Focus();
            CardExpiryYearBox.CaretIndex = CardExpiryYearBox.Text.Length;
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
    private void CardNumberBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        e.Handled = true; 
        CardExpiryMonthBox.Focus();
        CardExpiryMonthBox.SelectAll();
    }
    private void CardExpiryYearBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        e.Handled = true;
        CardCvcBox.Focus();
        CardCvcBox.SelectAll();
    }

    private void CardCvcBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        e.Handled = true;
        _ = PayAsync();
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
        _ = PayAsync();
    }

    private async System.Threading.Tasks.Task PayAsync()
    {
        //if (UiServices.CurrentUser is null)
        //{
        //    ErrorMessageText.Text = "Пользователь не авторизованн";
        //    ErrorMessageText.Margin = new Thickness(0, -9, 0, 9);
        //    UiServices.Navigation.Navigate(AppPage.Login);
        //    return;
        //}

        var number = CardNumberBox.Text.Trim();
        var cvc = CardCvcBox.Text.Trim();
        var expMonth = string.IsNullOrEmpty(CardExpiryMonthBox.Text) ? 0 : int.Parse(CardExpiryMonthBox.Text.Trim());
        var expYear2 = string.IsNullOrEmpty(CardExpiryYearBox.Text) ? 0 : int.Parse(CardExpiryYearBox.Text.Trim());

        if (UiServices.CurrentUser is null)
        {
            ErrorMessageText.Text = "Пользователь не авторизован";
            ErrorMessageText.Margin = new Thickness(0, -9, 0, 9);
            UiServices.Navigation.Navigate(AppPage.Login);
            return;
        }

        var userId = UiServices.CurrentUser.Id;
        var plan = MapPlan(_plan.Kind);

        var result = await PaymentAppService.PaySubscriptionAsync(
            UiServices.PaymentMethodRepository,
            UiServices.SubscriptionRepository,
            userId,
            plan,
            number,
            expMonth,
            expYear2,
            cvc);

        if (!result.IsSuccess)
        {
            ErrorMessageText.Text = result.Error;
            ErrorMessageText.Margin = new Thickness(0, -9, 0, 9);
            return;
        }

        UiServices.Navigation.Navigate(AppPage.Entries);
    }

    private static SubscriptionPlan MapPlan(SubscriptionPlanKind kind)
    {
        return kind switch
        {
            SubscriptionPlanKind.Month => SubscriptionPlan.Monthly,
            SubscriptionPlanKind.HalfYear => SubscriptionPlan.HalfEar,
            SubscriptionPlanKind.Year => SubscriptionPlan.Yearly,
            _ => SubscriptionPlan.Monthly
        };
    }
}
