using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MyDiary.Services.Payments;

public sealed record FakeCard
{
    public required string Number { get; init; }
    public required int ExpMonth { get; init; }
    public required int ExpYear2 { get; init; }
    public required string Cvc { get; init; }
    public required decimal BalanceRub { get; set; }
    public string Brand { get; init; } = "Unknown";
}

public sealed record FakePaymentMethod
{
    public required string Token { get; init; }
    public required string Last4 { get; init; }
    public string Brand { get; init; } = "Unknown";
}

public static class FakePaymentProvider
{
    private static readonly object Sync = new();
    private static readonly Random Random = new();

    private const string CardsFileName = "FakeCards.txt";
    private const string CardsPathEnvVar = "MYDIARY_FAKE_CARDS_PATH";
    private static bool CardsLoaded;
    private static readonly string CardsFilePath = ResolveCardsFilePath();

    private static readonly List<FakeCard> CardsInternal = new();

    private static readonly Dictionary<string, FakeCard> TokenToCard = new(StringComparer.Ordinal);

    static FakePaymentProvider()
    {
        LoadCardsFromFile();
    }

    public static IReadOnlyList<FakeCard> Cards
    {
        get
        {
            lock (Sync)
            {
                EnsureLoaded();
                return CardsInternal.AsReadOnly();
            }
        }
    }

    private static void EnsureLoaded()
    {
        if (CardsLoaded)
        {
            return;
        }

        LoadCardsFromFile();
    }

    private static string GetCardsFilePath()
    {
        return CardsFilePath;
    }

    private static string ResolveCardsFilePath()
    {
        var env = Environment.GetEnvironmentVariable(CardsPathEnvVar);
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env;
        }

        try
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            for (var i = 0; i < 12 && current is not null; i++)
            {
                var sln = Path.Combine(current.FullName, "MyDiary.sln");
                if (File.Exists(sln))
                {
                    return Path.Combine(current.FullName, "MyDiary.Services", "Payments", CardsFileName);
                }

                current = current.Parent;
            }
        }
        catch
        {
            // ignore
        }

        return Path.Combine(AppContext.BaseDirectory, CardsFileName);
    }

    private static void LoadCardsFromFile()
    {
        lock (Sync)
        {
            if (CardsLoaded)
            {
                return;
            }

            try
            {
                var path = GetCardsFilePath();
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(path))
                {
                    File.WriteAllText(path, string.Empty);
                }

                var seenNumbers = new HashSet<string>(StringComparer.Ordinal);
                var dedupChanged = false;

                foreach (var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (!TryParseCardLine(line, out var card))
                    {
                        continue;
                    }

                    if (!seenNumbers.Add(card.Number))
                    {
                        dedupChanged = true;
                        continue;
                    }

                    CardsInternal.Add(card);
                }

                if (dedupChanged)
                {
                    SaveCardsToFileUnsafe();
                }
            }
            catch
            {
                // ignore file IO errors; provider will operate with in-memory cards only
            }
            finally
            {
                CardsLoaded = true;
            }
        }
    }

    private static bool TryParseCardLine(string line, out FakeCard card)
    {
        card = default!;

        var parts = line.Split('\\');
        if (parts.Length != 6)
        {
            return false;
        }

        var number = (parts[0] ?? string.Empty).Trim();
        if (!int.TryParse((parts[1] ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var expMonth))
        {
            return false;
        }

        if (!int.TryParse((parts[2] ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var expYear2))
        {
            return false;
        }

        var cvc = (parts[3] ?? string.Empty).Trim();
        var balanceRaw = (parts[4] ?? string.Empty).Trim();
        if (balanceRaw.EndsWith("m", StringComparison.OrdinalIgnoreCase))
        {
            balanceRaw = balanceRaw[..^1];
        }

        if (!TryParseDecimal(balanceRaw, out var balanceRub))
        {
            return false;
        }

        var brand = (parts[5] ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(brand))
        {
            brand = DetectBrand(number);
        }

        card = new FakeCard
        {
            Number = number,
            ExpMonth = expMonth,
            ExpYear2 = expYear2,
            Cvc = cvc,
            BalanceRub = balanceRub,
            Brand = brand
        };

        return true;
    }

    private static bool TryParseDecimal(string raw, out decimal value)
    {
        raw = (raw ?? string.Empty).Trim();

        if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        var normalized = raw.Replace(',', '.');
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
    }

    private static void SaveCardsToFile()
    {
        lock (Sync)
        {
            SaveCardsToFileUnsafe();
        }
    }

    private static void SaveCardsToFileUnsafe()
    {
        try
        {
            var path = GetCardsFilePath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var lines = new List<string>(CardsInternal.Count);
            foreach (var c in CardsInternal)
            {
                lines.Add(SerializeCardLine(c));
            }

            File.WriteAllLines(path, lines);
        }
        catch
        {
            // ignore file IO errors
        }
    }

    private static string SerializeCardLine(FakeCard card)
    {
        var brand = string.IsNullOrWhiteSpace(card.Brand) ? DetectBrand(card.Number) : card.Brand;
        return string.Concat(
            card.Number,
            "\\",
            card.ExpMonth.ToString(CultureInfo.InvariantCulture),
            "\\",
            card.ExpYear2.ToString(CultureInfo.InvariantCulture),
            "\\",
            card.Cvc,
            "\\",
            card.BalanceRub.ToString(CultureInfo.InvariantCulture),
            "m\\",
            brand);
    }

    public static bool TryCreateToken(
        string cardNumber,
        int expMonth,
        int expYear2,
        string cvc,
        out FakePaymentMethod paymentMethod,
        out string error)
    {
        EnsureLoaded();
        paymentMethod = default!;
        error = string.Empty;

        if (!TryGetOrAddCard(cardNumber, expMonth, expYear2, cvc, out var card, out error))
        {
            return false;
        }

        var token = "tok_" + Guid.NewGuid().ToString("N");

        lock (Sync)
        {
            TokenToCard[token] = card;
        }

        paymentMethod = new FakePaymentMethod
        {
            Token = token,
            Last4 = card.Number.Length >= 4 ? card.Number[^4..] : card.Number,
            Brand = card.Brand
        };

        return true;
    }

    public static bool TryChargeByToken(string token, decimal amountRub, out string error)
    {
        EnsureLoaded();
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            error = "Токен пуст";
            return false;
        }

        if (amountRub <= 0)
        {
            error = "Сумма должна быть положительной";
            return false;
        }

        FakeCard card;
        lock (Sync)
        {
            if (!TokenToCard.TryGetValue(token, out card!))
            {
                error = "Неизвестный токен";
                return false;
            }

            if (card.BalanceRub < amountRub)
            {
                error = "Недостаточно средств";
                return false;
            }

            card.BalanceRub -= amountRub;
            SaveCardsToFileUnsafe();
        }

        return true;
    }

    public static bool TryChargeByCard(
        string cardNumber,
        int expMonth,
        int expYear2,
        string cvc,
        decimal amountRub,
        out string error)
    {
        EnsureLoaded();
        error = string.Empty;

        if (amountRub <= 0)
        {
            error = "Сумма должна быть положительной";
            return false;
        }

        if (!TryGetOrAddCard(cardNumber, expMonth, expYear2, cvc, out var card, out error))
        {
            return false;
        }

        lock (Sync)
        {
            if (card.BalanceRub < amountRub)
            {
                error = "Недостаточно средств";
                return false;
            }

            card.BalanceRub -= amountRub;
            SaveCardsToFileUnsafe();
        }

        return true;
    }

    public static bool CanCardExist(
        string cardNumber,
        int expMonth,
        int expYear2,
        string cvc,
        out string error)
    {
        error = string.Empty;

        if (!ValidateCardInput(cardNumber, expMonth, expYear2, cvc, out error))
        {
            return false;
        }

        if (!IsLuhnValid(cardNumber))
        {
            error = "Номер карты не действителен";
            return false;
        }

        if (IsExpired(expMonth, expYear2))
        {
            error = "Срок действия карты истёк";
            return false;
        }

        return true;
    }

    private static bool TryGetOrAddCard(
        string cardNumber,
        int expMonth,
        int expYear2,
        string cvc,
        out FakeCard card,
        out string error)
    {
        card = default!;
        error = string.Empty;

        EnsureLoaded();

        if (!CanCardExist(cardNumber, expMonth, expYear2, cvc, out error))
        {
            return false;
        }

        lock (Sync)
        {
            FakeCard? byNumber = null;
            foreach (var c in CardsInternal)
            {
                if (string.Equals(c.Number, cardNumber, StringComparison.Ordinal))
                {
                    byNumber = c;
                    break;
                }
            }

            if (byNumber is not null)
            {
                if (byNumber.ExpMonth != expMonth || byNumber.ExpYear2 != expYear2)
                {
                    error = "Неверный срок действия карты";
                    card = default!;
                    return false;
                }

                if (!string.Equals(byNumber.Cvc, cvc, StringComparison.Ordinal))
                {
                    error = "Неверный CVC";
                    card = default!;
                    return false;
                }

                card = byNumber;
                return true;
            }

            var newCard = new FakeCard
            {
                Number = cardNumber,
                ExpMonth = expMonth,
                ExpYear2 = expYear2,
                Cvc = cvc,
                BalanceRub = GenerateRandomBalanceRub(),
                Brand = DetectBrand(cardNumber)
            };

            CardsInternal.Add(newCard);
            card = newCard;
            SaveCardsToFileUnsafe();
            return true;
        }
    }

    private static decimal GenerateRandomBalanceRub()
    {
        // 0..10000 RUB
        lock (Sync)
        {
            return Random.Next(0, 10001);
        }
    }

    public static bool ValidateCardInput(string cardNumber, int expMonth, int expYear2, string cvc, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(cardNumber) || string.IsNullOrWhiteSpace(cvc) || string.IsNullOrWhiteSpace(expMonth.ToString()) || string.IsNullOrWhiteSpace(expYear2.ToString()))
        {
            error = "Данные карты не заполнены";
            return false;
        }

        if (!IsDigits(cardNumber) || !IsDigits(cvc))
        {
            error = "Номер карты и CVC должны содержать только цифры";
            return false;
        }

        if (cardNumber.Length is < 13 or > 19)
        {
            error = "Длина номера карты должна быть от 13 до 19 цифр";
            return false;
        }

        if (expMonth is < 1 or > 12)
        {
            error = "Некорректный месяц";
            return false;
        }

        if (expYear2 is < 0 or > 99)
        {
            error = "Некорректный год";
            return false;
        }

        return true;
    }

    private static bool IsExpired(int expMonth, int expYear2)
    {
        var now = DateTime.UtcNow;
        var year = 2000 + expYear2;

        // карта действует до конца месяца
        var expEnd = new DateTime(year, expMonth, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
        return now >= expEnd;
    }

    private static bool IsLuhnValid(string digits)
    {
        var sum = 0;
        var alternate = false;

        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var ch = digits[i];
            if (ch < '0' || ch > '9')
            {
                return false;
            }

            var n = ch - '0';
            if (alternate)
            {
                n *= 2;
                if (n > 9)
                {
                    n -= 9;
                }
            }

            sum += n;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    private static string DetectBrand(string cardNumber)
    {
        if (cardNumber.StartsWith("4", StringComparison.Ordinal))
        {
            return "VISA";
        }

        if (cardNumber.StartsWith("5", StringComparison.Ordinal))
        {
            return "MASTERCARD";
        }

        if (cardNumber.StartsWith("220", StringComparison.Ordinal))
        {
            return "MIR";
        }

        return "Unknown";
    }

    private static bool IsDigits(string value)
    {
        foreach (var ch in value)
        {
            if (ch < '0' || ch > '9')
            {
                return false;
            }
        }

        return true;
    }
}
