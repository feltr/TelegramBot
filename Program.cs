using System;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using System.Text;

Dictionary<long, Dictionary<string,int>> keyPersonValue = new Dictionary<long, Dictionary<string, int>>();

string[] ArrText = Directory.GetFiles("Text","*",SearchOption.AllDirectories);
string[] ArrImage = Directory.GetFiles("Image","*",SearchOption.AllDirectories);
string[] ArrVideo = Directory.GetFiles("Video","*",SearchOption.AllDirectories);
string[] ArrVoice = Directory.GetFiles("Voice","*",SearchOption.AllDirectories);


var botClient = new TelegramBotClient("6508423852:AAFN9d_B_dNasMJQli_py-XjIZupXvh3q28");
using CancellationTokenSource cts = new();


// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    if (!keyPersonValue.ContainsKey(chatId))
        keyPersonValue[chatId] = new Dictionary<string, int>();

    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
{
    new KeyboardButton[] { "Стикер", "Картиночки🖼️", "Анекдот🤣", "Музыка🎧", "Видюхи🎬" },
})
    {
        ResizeKeyboard = true
    };

    if (message.Text == "/start")
    {
        await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Выберите, что хотите увидеть",
        replyMarkup: replyKeyboardMarkup,
        cancellationToken: cancellationToken);
    }
    if (message.Text == "Стикер") 
    {
        await botClient.SendStickerAsync(
            chatId: chatId,
            sticker: InputFile.FromFileId("CAACAgIAAxkBAAELC1xliw-aqTtM2l4iVsCXhBDNTSpT5AACWQADQzOdIdtjFMFzmotoMwQ"),
            cancellationToken: cancellationToken);
    }
    else if(message.Text == "Картиночки🖼️")
    {
        if (!keyPersonValue[chatId].ContainsKey("Image") ||
                keyPersonValue[chatId]["Image"] >= ArrImage.Length)
            keyPersonValue[chatId]["Image"] = 0;

        string filePath = ArrImage[keyPersonValue[chatId]["Image"]];
        using (Stream stream = System.IO.File.OpenRead(filePath))
        {
            await botClient.SendPhotoAsync(
            chatId: chatId,
            photo: InputFile.FromStream(stream),
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
        }

        keyPersonValue[chatId]["Image"]++;
    }
    else if (message.Text == "Анекдот\U0001f923")
    {

        if (!keyPersonValue[chatId].ContainsKey("Text") ||
                keyPersonValue[chatId]["Text"] >= ArrText.Length)
            keyPersonValue[chatId]["Text"] = 0;

        string filePath = ArrText[keyPersonValue[chatId]["Text"]];
        using (StreamReader streamRead = new StreamReader(filePath, Encoding.UTF8))
        {
            string textRead = await streamRead.ReadToEndAsync();
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: textRead,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        keyPersonValue[chatId]["Text"]++;
    }
    else if (message.Text == "Музыка🎧")
    {
        if (!keyPersonValue[chatId].ContainsKey("Voice") ||
                keyPersonValue[chatId]["Voice"] >= ArrVoice.Length)
            keyPersonValue[chatId]["Voice"] = 0;

        string filePath = ArrVoice[keyPersonValue[chatId]["Voice"]];
        using (Stream stream = System.IO.File.OpenRead(filePath))
        {
            await botClient.SendAudioAsync(
        chatId: chatId,
        replyMarkup: replyKeyboardMarkup,
        audio: InputFile.FromStream(stream),
        cancellationToken: cancellationToken);
        }

        keyPersonValue[chatId]["Voice"]++;

    }
    else if (message.Text == "Видюхи🎬")
    {
        if (!keyPersonValue[chatId].ContainsKey("Video") ||
                keyPersonValue[chatId]["Video"] >= ArrVideo.Length)
            keyPersonValue[chatId]["Video"] = 0;

        string filePath = ArrVideo[keyPersonValue[chatId]["Video"]];
        using (Stream stream = System.IO.File.OpenRead(filePath))
        {
            await botClient.SendVideoNoteAsync(
                chatId: chatId,
                videoNote: InputFile.FromStream(stream),
                cancellationToken: cancellationToken);
        }

        keyPersonValue[chatId]["Video"]++;
    }
    else 
    {
        await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Кликай по кнопкам",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
    }

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}