using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
//using System.Threading.Tasks;

namespace VCC
{
    class MailService
    {
        public static string Address;
        private static string robotAddress ="";
        private static string robotPass="";

        public static void config(string Address, string Pass)
        {
            robotAddress = Address;
            robotPass = Pass;
        }
        public static void send(string subject,string msg)
        {
            try
            {
                MailMessage mail = new MailMessage(robotAddress, Address, subject,
                                    "Auto generated mail: " + Environment.NewLine + msg);
                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(robotAddress, robotPass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Host = "smtp.gmail.com"; 
                client.Send(mail);
            }
            catch
            {
                Logger.add("MAIL", "Failed to send");
                return;
            }
            Logger.add("MAIL","Mail was sended");
        }
    }
}
