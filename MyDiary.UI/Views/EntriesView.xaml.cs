using System.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;
using System.Linq;

namespace MyDiary.UI.Views;

public partial class EntriesView : UserControl
{
    private DateTime _monthCursor = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private readonly ObservableCollection<EntryPreview> _items = new();

    public EntriesView()
    {
        InitializeComponent();

        EntriesList.ItemsSource = _items;
        Seed();
        ApplySort();
        UpdateHeader();
    }

    private void ApplySort()
    {
        var sorted = _items.OrderByDescending(x => x.CreatedAt).ToList();
        _items.Clear();
        foreach (var item in sorted)
        {
            _items.Add(item);
        }
    }

    private void Seed()
    {
        _items.Clear();
        _items.Add(new EntryPreview(
            Title: "Хороший день",
            Summary: "Сегодня было спокойно. Прогулялся, сделал задачи, вечером немного устал — но в целом ок.",
            Mood: "😊",
            CreatedAt: DateTime.Now.AddDays(-1),
            Activities: new[] { "спорт", "прогулка", "работа" }
        ));
        _items.Add(new EntryPreview(
            Title: "Сложный день",
            Summary: "Много дел и стресс. Хочется выспаться и меньше думать о мелочах...",
            Mood: "😣",
            CreatedAt: DateTime.Now.AddDays(-3),
            Activities: new[] { "учёба", "дом" }
        ));

        var moods = new[] { "😔", "😣", "😐", "🙂", "😊", "😁" };
        var titles = new[]
        {
            "Немного устал",
            "Спокойный вечер",
            "Продуктивный день",
            "Переутомление",
            "Хорошие новости",
            "День без спешки",
            "Новая привычка",
            "Слишком много задач",
            "Приятная прогулка",
            "Тёплый разговор",
            "Сфокусировался",
            "Сбился режим",
            "Вернулся в форму",
            "Немного тревожно",
            "Отличное настроение"
        };

        var summaries = new[]
        {
            "Сил сегодня меньше обычного, но смог закончить главное. Завтра хочу лечь пораньше и разгрузить голову.",
            "Вечер прошёл ровно: чай, музыка и тишина. Кажется, именно этого не хватало всю неделю.",
            "Получилось сделать то, что откладывал. Когда держишь фокус — становится легче и спокойнее.",
            "Слишком плотный день: задачи, звонки, дедлайны. Важно не забыть про отдых.",
            "Услышал хорошие новости, стало теплее внутри. Поймал ощущение стабильности.",
            "Никуда не торопился, делал всё медленно. Это реально помогает расслабиться.",
            "Попробовал маленькую новую привычку. Хочу удержать её хотя бы неделю.",
            "Список дел вырос и начал давить. Разбил на шаги — стало проще.",
            "Дышалось легко, много ходил. После прогулки настроение поднялось.",
            "Поговорили с близким человеком. Сразу отпустило напряжение.",
            "Удалось сосредоточиться и не отвлекаться. Результат радует.",
            "Поздно лег и весь день сонный. Завтра — перезагрузка.",
            "Немного спорта вернул энергию. Хочу продолжать.",
            "Есть лёгкая тревога, но я её замечаю и не накручиваю. Дыхание помогает.",
            "Чувствую прилив сил и мотивации. Поймал хороший ритм!"
        };

        var activitiesPool = new[] { "спорт", "прогулка", "работа", "учёба", "дом", "друзья", "сон", "хобби", "музыка", "чтение" };

        for (var i = 0; i < 15; i++)
        {
            var createdAt = DateTime.Now.AddDays(-(4 + i));
            var mood = moods[i % moods.Length];

            var a1 = activitiesPool[i % activitiesPool.Length];
            var a2 = activitiesPool[(i + 3) % activitiesPool.Length];
            var a3 = activitiesPool[(i + 6) % activitiesPool.Length];

            _items.Add(new EntryPreview(
                Title: titles[i],
                Summary: summaries[i],
                Mood: mood,
                CreatedAt: createdAt,
                Activities: new[] { a1, a2, a3 }
            ));
        }
    }

    private void UpdateHeader()
    {
        MonthText.Text = _monthCursor.ToString("MMMM yyyy");
        var maxMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        NextMonthButton.IsEnabled = _monthCursor < maxMonth;
    }

    private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
    {
        _monthCursor = _monthCursor.AddMonths(-1);
        ApplySort();
        UpdateHeader();
    }

    private void NextMonthButton_Click(object sender, RoutedEventArgs e)
    {
        _monthCursor = _monthCursor.AddMonths(1);
        ApplySort();
        UpdateHeader();
    }

    private void EntryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: EntryPreview entry })
        {
            UiServices.Navigation.Navigate(AppPage.EntryDetails, entry);
        }
    }
}
