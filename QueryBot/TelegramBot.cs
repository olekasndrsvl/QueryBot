namespace SimpleTGBot;

using System.IO;

using System.Runtime.Serialization.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = File;
using System;


using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Text;

public class Student
{
   
    public string Name { get;}
   
    [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public long chatid { get; }
 
    public Student(string name,long id)
    {
        Name = name;
        chatid = id;
    }
}


public class TelegramBot
{
    // Токен TG-бота. Можно получить у @BotFather
    private const string BotToken = "7178718884:AAHKlFtAt_uz9QqBKk1fBN0UfpgEuqVk0so";
    /// <summary>
    /// Инициализирует и обеспечивает работу бота до нажатия клавиши Esc
    /// </summary>
    public Dictionary<long,Student> registredStudents;
    private Queue<Student> Students = new Queue<Student>();
    public  List<long> AdminsID = new List<long> { 859018369 };

    public List<DateTime> EndOfLessons = new List<DateTime>() {
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 35, 0),
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 11, 25, 0),
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 0),
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 20, 0),
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 17, 30, 0)

        };

  

    public async Task Run()
    {
        // Если вам нужно хранить какие-то данные во время работы бота (массив информации, логи бота,
        // историю сообщений для каждого пользователя), то это всё надо инициализировать в этом методе.


        // TODO: Инициализация необходимых полей


        // Инициализируем наш клиент, передавая ему токен.
        var botClient = new TelegramBotClient(BotToken);

        // Служебные вещи для организации правильной работы с потоками
        using CancellationTokenSource cts = new CancellationTokenSource();

        // Разрешённые события, которые будет получать и обрабатывать наш бот.
        // Будем получать только сообщения. При желании можно поработать с другими событиями.
        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        // Привязываем все обработчики и начинаем принимать сообщения для бота
        botClient.StartReceiving(
            updateHandler: OnMessageReceived,
            pollingErrorHandler: OnErrorOccured,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        // Проверяем что токен верный и получаем информацию о боте
        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc...");

        // Ждём, пока будет нажата клавиша Esc, тогда завершаем работу бота
        while (Console.ReadKey().Key != ConsoleKey.Escape) 
        {
            if (EndOfLessons.Contains(DateTime.Now))
            {
                Students.Clear();
            }
        
        }

        // Отправляем запрос для остановки работы клиента.
        cts.Cancel();
    }

    /// <summary>
    /// Обработчик события получения сообщения.
    /// </summary>
    /// <param name="botClient">Клиент, который получил сообщение</param>
    /// <param name="update">Событие, произошедшее в чате. Новое сообщение, голос в опросе, исключение из чата и т. д.</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    async Task OnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        // инициализация словаря зарегистрированных пользователей
        using (var stream = File.Open("data.json", FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {

            using (var writer = new StreamReader(stream))
            {
                registredStudents = JsonConvert.DeserializeObject<Dictionary<long, Student>>(writer.ReadLine());
                if (registredStudents == null)
                {
                    registredStudents = new Dictionary<long, Student>();
                }
            }
        }


        // Работаем только с сообщениями. Остальные события игнорируем
        var message = update.Message;
        if (message is null)
        {
            return;
        }
       


        if (message.Text is not { } messageText)
        {
            return;
        }



        // Получаем ID чата, в которое пришло сообщение. Полезно, чтобы отличать пользователей друг от друга.
        var chatId = message.Chat.Id;
       
        // Печатаем на консоль факт получения сообщения
        Console.WriteLine($"Получено сообщение в чате {chatId}: '{messageText}'");




        // Клавиатура главного меню
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
           {
                new KeyboardButton[] {    "Я ответил!" },
                 new KeyboardButton[] { "Занять очередь" },
                new KeyboardButton[] { "Вывести очередь на текущую пару" },
               
               
             
                
             })
        {
            ResizeKeyboard = true
        };
        // Админ-клавиатура
        ReplyKeyboardMarkup AdminKeyBoard = new(new[]
          {
                new KeyboardButton[] { "Занять очередь" },
                new KeyboardButton[] { "Вывести очередь на текущую пару" },
                   new KeyboardButton[] { "Очистить очередь" },
                      new KeyboardButton[] { "Удалить из очереди пользователя" },

             })
        {
            ResizeKeyboard = true
        };





        // TODO: Обработка пришедших сообщений


        // регистрация студента
        void RegisterStudent(string name)
        {
            this.registredStudents.Add(chatId, new Student(message.Text, chatId));

            using (var stream = File.Open("data.json", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {

                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine( JsonConvert.SerializeObject(registredStudents));
                }
            }

        }
        switch (message.Text)
        {
            case "Занять очередь":
                if (registredStudents.ContainsKey(chatId))
                {
                    Students.Enqueue(new Student(registredStudents[chatId].Name,chatId)); // костыль из-за уебанской десериализации
                    foreach(var x in registredStudents)
                    {
                        Console.WriteLine(x.Value.Name+" "+x.Value.chatid);
                    }
                    Message msg = await botClient.SendTextMessageAsync(
                                 chatId: chatId,
                                 text: "Вы заняли очередь!",
                                 replyMarkup: replyKeyboardMarkup,
                                 cancellationToken: cancellationToken);
                }
                else
                {
                    Message msg1 = await botClient.SendTextMessageAsync(
                                 chatId: chatId,
                                 text: "Вы не зарегистрированы! Используйте /start",
                                 replyMarkup: replyKeyboardMarkup,
                                 cancellationToken: cancellationToken);
                }

                break;

            case "Вывести очередь на текущую пару":
                var s = new StringBuilder();
                int cnt = 0;
                foreach(var x in Students)
                {
                    cnt++;
                    s.Append($"{cnt}. {x.Name} \n");
                }

                if (s.ToString() != "")
                {
                    Message _ = await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: s.ToString(),
                               replyMarkup: replyKeyboardMarkup,
                               cancellationToken: cancellationToken);
                }
                else
                {
                    Message _ = await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: "Очередь пуста, вы будете первыми!",
                               replyMarkup: replyKeyboardMarkup,
                               cancellationToken: cancellationToken);
                }
                
                break;

            case "Я ответил!":
                if (Students.Count() != 0)
                {
                    var temp = Students.Peek();
                   
                    if (chatId == temp.chatid)
                    {
                        Students.Dequeue();
                        Message f_ = await botClient.SendTextMessageAsync(
                                   chatId: chatId,
                                   text: "Вы вышли из очереди!",
                                   replyMarkup: replyKeyboardMarkup,
                                   cancellationToken: cancellationToken);

                    }
                    else
                    {
                        Message f_ = await botClient.SendTextMessageAsync(
                                   chatId: chatId,
                                   text: "Ваша очередь ещё не подошла!",
                                   replyMarkup: replyKeyboardMarkup,
                                   cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    Message f_ = await botClient.SendTextMessageAsync(
                                  chatId: chatId,
                                  text: "Очередь пуста!",
                                  replyMarkup: replyKeyboardMarkup,
                                  cancellationToken: cancellationToken);
                }
               



                break;

            case "/start":


                Message startmsg = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Введите вашу фамилию и имя",
                cancellationToken: cancellationToken);
                break;
            default:
                if(!registredStudents.ContainsKey(chatId))
                {
                    RegisterStudent(messageText);
                    Message ms = await botClient.SendTextMessageAsync(
                                   chatId: chatId,
                                   text: "Вы успешно зарегистрированны!",
                                   replyMarkup: replyKeyboardMarkup,
                                   cancellationToken: cancellationToken);
                   foreach (var x in registredStudents)
                    {
                        Console.WriteLine(x.ToString());
                    }
                }
                else
                {
                    if (AdminsID.Contains(chatId))
                    {
                        switch (messageText)
                        {
                            case "/apanel":
                                Message _f = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Have a great time!",
                                replyMarkup: AdminKeyBoard,
                                cancellationToken: cancellationToken);
                                break;
                            case "Очистить очередь":
                                Students.Clear();
                                Message h_ = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Queque cleaned!",
                                replyMarkup: replyKeyboardMarkup,
                                cancellationToken: cancellationToken);
                                break;
                            case "Удалить из очереди пользователя":
                                
                                break;


                        }



                    }
                    else
                    {
                        Message h_ = await botClient.SendTextMessageAsync(
                                   chatId: chatId,
                                   text: "Некорректная команда!",
                                   replyMarkup: replyKeyboardMarkup,
                                   cancellationToken: cancellationToken);
                    }
                    
                }


                break;

        }





    }

    /// <summary>
    /// Обработчик исключений, возникших при работе бота
    /// </summary>
    /// <param name="botClient">Клиент, для которого возникло исключение</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    /// <returns></returns>
    Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // В зависимости от типа исключения печатаем различные сообщения об ошибке
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",

            _ => exception.ToString()
        };
      
        Console.WriteLine(errorMessage);

        // Завершаем работу
        return Task.CompletedTask;
    }
}