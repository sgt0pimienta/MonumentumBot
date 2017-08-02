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
            //Variables in IFS have been made method variables again
            bool isEdited = new bool();
            List<string> userMessageSubrstringList = null;
            List<int> dateDataList = new List<int>();
            string newID;

            //Checks to see if message is EditedMessage or Message
            if (downloadedUpdate.Message == null && downloadedUpdate.EditedMessage != null)
            {
                isEdited = true;
            }
            else if(downloadedUpdate.Message != null && downloadedUpdate.EditedMessage == null)
            {
                isEdited = false;
            }
            else if (downloadedUpdate.Message != null & downloadedUpdate.EditedMessage !=null)
            {
                isEdited = true;
            }

            //Checks to see if the update is a request for a /format command before doing anything else
            if (isEdited == false)
            {
                if (downloadedUpdate.Message.Text == "/formato@MonumentumBot" || downloadedUpdate.Message.Text == "/Formato@MonumentumBot" || downloadedUpdate.Message.Text == "/formato")
                {
                    string formatMemoID;
                    formatMemoID = downloadedUpdate.Message.Chat.Id.ToString() + "-" + downloadedUpdate.Message.MessageId;
                    return new ScheduledMemo(DateTime.Now, "El formato es /minuto/hora(24)/día/mes/año/mensaje. Ejemplo: 0/13/30/4/2017/Cumpleaños de Emilia", false, formatMemoID, downloadedUpdate.Message.Chat.Id.ToString());
                }
                if (downloadedUpdate.Message.Text == "/start" || downloadedUpdate.Message.Text == "/start@MonumentumBot")
                {
                    string formatMemoId;
                    formatMemoId = downloadedUpdate.Message.Chat.Id.ToString() + "-" + downloadedUpdate.Message.MessageId;
                    return new ScheduledMemo(DateTime.Now, "Bienvenido a RecuerdaBot / Welcome to RecuerdaBot", false, formatMemoId, downloadedUpdate.Message.Chat.Id.ToString());
                }
            }
            else
            {
                if (downloadedUpdate.EditedMessage.Text == "/formato@MonumentumBot" || downloadedUpdate.EditedMessage.Text == "/Formato@MonumentumBot" || downloadedUpdate.EditedMessage.Text == "/formato")
                {
                    string formatMemoID;
                    formatMemoID = downloadedUpdate.EditedMessage.Chat.Id.ToString() + "-" + downloadedUpdate.EditedMessage.MessageId;
                    return new ScheduledMemo(DateTime.Now, "El formato es /minuto/hora(24)/día/mes/año/mensaje. Ejemplo: 0/13/30/4/2017/Cumpleaños de Emilia", false, formatMemoID, downloadedUpdate.EditedMessage.Chat.Id.ToString());
                }
                if (downloadedUpdate.EditedMessage.Text == "/start" || downloadedUpdate.EditedMessage.Text == "/start@MonumentumBot")
                {
                    string formatMemoId;
                    formatMemoId = downloadedUpdate.EditedMessage.Chat.Id.ToString() + "-" + downloadedUpdate.EditedMessage.MessageId;
                    return new ScheduledMemo(DateTime.Now, "Bienvenido a RecuerdaBot / Welcome to RecuerdaBot", false, formatMemoId, downloadedUpdate.EditedMessage.Chat.Id.ToString());
                }
            }



            // El formato para cada mensaje de usuario es "/minuto/hora/día/mes/año/mensaje"
            // Datetime = YMDHMS
            //Main properties of the Memo class
            String scheduledMessage;
            DateTime scheduledTime;

            //Operative variables to parse the userMessage into a scheduledMessage String and a scheduledTime DateTime
            if (isEdited == false)
            {
                if (downloadedUpdate.Message.Text == null)
                {
                    string formatmemoID = downloadedUpdate.Message.Chat.Id.ToString() + "-" + downloadedUpdate.Message.MessageId;
                    return new ScheduledMemo(DateTime.Now, "El comando no puede estar vacío", false, formatmemoID, downloadedUpdate.Message.Chat.Id.ToString());
                }
            }
            else
            {
                if (downloadedUpdate.EditedMessage.Text == null)
                {
                    string formatmemoID = downloadedUpdate.EditedMessage.Chat.Id.ToString() + "-" + downloadedUpdate.EditedMessage.MessageId;
                    return new ScheduledMemo(DateTime.Now, "El comando no puede estar vacío", false, formatmemoID, downloadedUpdate.EditedMessage.Chat.Id.ToString());
                }
            }


            if (isEdited == false)
            {
                userMessageSubrstringList = new List<string>(downloadedUpdate.Message.Text.Split('/'));
            }
            else
            {
                userMessageSubrstringList = new List<string>(downloadedUpdate.EditedMessage.Text.Split('/'));
            }


            //Generate a unique memo Id out of the chat ID and the Memo ID
            if (isEdited == false)
            {
                newID = downloadedUpdate.Message.Chat.Id.ToString() + "-" + downloadedUpdate.Message.MessageId;
            }
            else
            {
                newID = downloadedUpdate.EditedMessage.Chat.Id.ToString() + "-" + downloadedUpdate.EditedMessage.MessageId;
            }

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
            if (isEdited == false)
            {
                return new ScheduledMemo(scheduledTime, scheduledMessage, true, newID, downloadedUpdate.Message.Chat.Id.ToString());
            }
            else
            {
                return new ScheduledMemo(scheduledTime, scheduledMessage, true, newID, downloadedUpdate.EditedMessage.Chat.Id.ToString());
            }
        }
    }
}
