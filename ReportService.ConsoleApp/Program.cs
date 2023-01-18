using Cipher;
using EmailSender;
using ReportService.Core;
using ReportService.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportService.ConsoleApp
{
    class Program
    {
       

        static void Main(string[] args)
        {

            /*
            var stringCiper = new StringCipher("1");
            var encryptedPassword = stringCiper.Encrypt("hasło");

            var decryptedPassword = stringCiper.Decrypt(encryptedPassword);

            Console.WriteLine(encryptedPassword);
            Console.WriteLine(decryptedPassword);
            Console.ReadLine();



            return;
            */
             var emailReciver = "";



            var htmlEmail = new GenerateHtmlEmail();


            var email = new Email(new EmailParams
            {
                HostSmtp = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                SenderName = "",
                SenderEmail = "",
                SenderEmailPassword = ""
            });

            var report =   new Report
            {
                Id = 1,
                Title = "R/1/2022",
                Date = new DateTime(2022, 1, 1, 12, 0, 0),
                Positions = new List<ReportPosition>
                {
                    new ReportPosition
                    {
                        Id = 1,
                        ReportId = 1,
                        Title = "Position 1",
                        Description = "Description 1",
                        Value = 43.01m

                    },
                     new ReportPosition
                    {
                        Id = 2,
                        ReportId = 1,
                        Title = "Position 2",
                        Description = "Description 2",
                        Value = 1.99m


                    },
                      new ReportPosition
                    {
                        Id = 3,
                        ReportId = 1,
                        Title = "Position 3",
                        Description = "Description 3",
                        Value = 5001m


                    }
                }
            };


            var errors =  new List<Error>
            {
                new Error {Message = "Błąd testowy 1", Date = DateTime.Now},
                new Error {Message = "Błąd testowy 2", Date = DateTime.Now}
            };


            Console.WriteLine("Wysyłąnie email (Błedy w aplikacji)......");
            email.Send("Błedy w aplikacji", htmlEmail.GenerateErros(errors, 10), emailReciver).Wait();
            Console.WriteLine("Wysyłano Raport dobowy......");


            Console.WriteLine("Wysyłąnie email (Raport doboey)......");
            email.Send("Raport aplikacji", htmlEmail.GenerateReport(report), emailReciver).Wait();
            Console.WriteLine("Wysyłano błędy aplikacji.....");

        }
    }
}
