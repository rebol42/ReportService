using Cipher;
using EmailSender;
using ReportService.Core;
using ReportService.Core.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ReportService
{
    public partial class ReportService : ServiceBase
    {
        //było const dla zmiennych SendHour,IntervalInMinutes
        private int SendHour;
        private int IntervalInMinutes;
        private bool WhetherSendReport;
        private Timer _timer;// = new Timer(IntervalInMinutes * 60000);
        private ErrorRepository _errorRepository = new ErrorRepository();
        private ReportRepository _reportRepository =  new ReportRepository();

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private GenerateHtmlEmail _htmlEmail = new GenerateHtmlEmail();
        private string _emailReciver;

        private StringCipher _stringCipher = new StringCipher("53CC1150-2E4D-49CB-82BA-A4B58EB8E9E3");

        private Email _email;

        public ReportService()
        {
            InitializeComponent();

            try
            {
                _emailReciver = ConfigurationManager.AppSettings["EmailReciver"];
                SendHour = Convert.ToInt32(ConfigurationManager.AppSettings["SendHour"]);
                IntervalInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalInMinutes"]);
                WhetherSendReport = Convert.ToBoolean(ConfigurationManager.AppSettings["WhetherSendReport"]);

                _timer = new Timer(IntervalInMinutes * 60000);




                _email = new Email(new EmailParams
                {
                    HostSmtp = ConfigurationManager.AppSettings["HostSmtp"],
                    Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]),
                    EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]),
                    SenderName = ConfigurationManager.AppSettings["SenderName"],
                    SenderEmail = ConfigurationManager.AppSettings["SenderEmail"],
                    SenderEmailPassword = DecryptSenderEmailPassword()
                    //_stringCipher.Decrypt(ConfigurationManager.AppSettings["SenderEmailPassword"])
                });
            }
            catch(Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }


        private string DecryptSenderEmailPassword()
        {
            // pobieramy hasło  z configa 
            var encryptedPassword = ConfigurationManager.AppSettings["SenderEmailPassword"];

            //sprawdzamy czy hasło jest zaszyfrowane
            if (encryptedPassword.StartsWith("encrypt:"))

            {
                encryptedPassword = _stringCipher.Encrypt(encryptedPassword.Replace("encrypt:", ""));

                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configFile.AppSettings.Settings["SenderEmailPassword"].Value = encryptedPassword;
                configFile.Save();
            }

            return _stringCipher.Decrypt(encryptedPassword);
        }

        protected override void OnStart(string[] args)
        {
            _timer.Elapsed += DoWork;
            _timer.Start();
            Logger.Info("Service started...");
        }

        private async void  DoWork(object sender, ElapsedEventArgs e)
        {
          
            try
            {
                await SendError();
                if(WhetherSendReport == true)
                    await SendReport();
            }
            catch(Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task SendError()
        {
            var error = _errorRepository.GetLastErrors(IntervalInMinutes);

            if (error == null || !error.Any())
                return;

            await _email.Send("Błedy w aplikacji", _htmlEmail.GenerateErros(error, IntervalInMinutes), _emailReciver);
            Logger.Info("Error sent...");
        }

        private async Task SendReport()
        {
            var actualHour = DateTime.Now.Hour;

            if (actualHour < SendHour)
                return;

            var report = _reportRepository.getLastNotSendReport();

            if (report == null)
                return;

            await _email.Send("Raport aplikacji", _htmlEmail.GenerateReport(report), _emailReciver);
            Logger.Info("Error sent...");

            _reportRepository.ReportSend(report);

            Logger.Info("Report sent...");

        }

        protected override void OnStop()
        {
            Logger.Info("Service stopped...");
        }
    }
}
