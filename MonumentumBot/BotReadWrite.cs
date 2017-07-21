using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram;
using Telegram.Bot.Types;

namespace MonumentumBot
{
    class BotReadWrite
    {
        //BotReadWrite Turns Updates into Memos

        public BotReadWrite()
        {

        }
        public ScheduledMemo WriteMemo(Update downloadedUpdate)
        {
            //Checks to see if the update is a request for a /format command before doing anything else
            if(downloadedUpdate.Message.Text == "/formato@MonumentumBot" || downloadedUpdate.Message.Text == "/Formato@MonumentumBot")
            {
                string formatMemoID;
                formatMemoID = downloadedUpdate.Message.Chat.Id.ToString() + "-" + downloadedUpdate.Message.MessageId;
                return new ScheduledMemo(DateTime.Now, "El formato es /minuto/hora(24)/día/mes/año/mensaje. Ejemplo: 0/13/30/4/2017/Cumpleaños de Emilia", false, formatMemoID, downloadedUpdate.Message.Chat.Id.ToString());
            }



            // El formato para cada mensaje de usuario es "/minuto/hora/día/mes/año/mensaje"
            // Datetime = YMDHMS
            //Main properties of the Memo class
            String scheduledMessage;
            DateTime scheduledTime;

            //Operative variables to parse the userMessage into a scheduledMessage String and a scheduledTime DateTime
            if (downloadedUpdate.Message.Text == null)
            {
                string formatmemoID = downloadedUpdate.Message.Chat.Id.ToString() + "-" + downloadedUpdate.Message.MessageId;
                return new ScheduledMemo(DateTime.Now, "El comando no puede estar vacío", false, formatmemoID, downloadedUpdate.Message.Chat.Id.ToString());
            }

            List<string> userMessageSubrstringList = new List<string>(downloadedUpdate.Message.Text.Split('/'));
                List<int> dateDataList = new List<int>();

            //Generate a unique memo Id out of the chat ID and the Memo ID
            string newID;
            newID = downloadedUpdate.Message.Chat.Id.ToString() + "-" + downloadedUpdate.Message.MessageId;

            //Rip intended user message from update text and snip the string out, snip out the head of the string if it is long enough
            scheduledMessage = userMessageSubrstringList[userMessageSubrstringList.Count - 1];
            userMessageSubrstringList.RemoveAt(userMessageSubrstringList.Count - 1);

            if (userMessageSubrstringList.Count() > 1)
            {
                userMessageSubrstringList.RemoveAt(0);
            }

            //Final redundant security check to make sure the substring list is long enough to parse into a DateTime. Also, in case the snipping resulted in a null list does a final check. This check is redundant.
            
            if (userMessageSubrstringList.Count < 5)
            {
                return new ScheduledMemo(DateTime.Now.AddSeconds(15), "Alerta: mal formato. Utiliza /formato para ver el formato correcto y un ejemplo de recordatorio", false, newID, downloadedUpdate.Message.Chat.Id.ToString());
            }


            //Check that the rest of the message is convertible to integers for formatting as DateTime
            foreach (String element in userMessageSubrstringList)
            {
                if (Int32.TryParse(element, out int convertedElement))
                {
                    dateDataList.Add(convertedElement);
                }
                else
                {
                    return new ScheduledMemo(DateTime.Now.AddSeconds(15), "Alerta: mal formato. Utiliza /formato para ver el formato correcto y un ejemplo de recordatorio", false, newID, downloadedUpdate.Message.Chat.Id.ToString());
                }
            }

            //Generate scheduled time from snipped user text (the message is snipped above)
            scheduledTime = new DateTime(dateDataList[4], dateDataList[3], dateDataList[2], dateDataList[1], dateDataList[0], 0);

            //Return memo. Message, Time and Memo ID have been added, the chat ID is ripped here, validity is given out as true due to the IF before.
            return new ScheduledMemo(scheduledTime, scheduledMessage, true, newID, downloadedUpdate.Message.Chat.Id.ToString());
        }
    }
}
