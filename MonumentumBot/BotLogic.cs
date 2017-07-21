using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram;
using Telegram.Bot.Types;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types

namespace MonumentumBot
{
    class BotLogic
    {
        //█████    ████         ██         ██  █████████         ██  ██████████████  ██         ██████████████████████████
        //███  ████  ██  █████████  █████████  █████████████  █████    ████████████  ██  █████████████████████████████████
        //███  ████  ██  █████████  █████████  █████████████  █████  ██  ██████████  ██  █████████████████████████████████
        //███  ████  ██      █████      █████  █████████████  █████  ████  ████████  ██  █████████████████████████████████
        //███  ████  ██  █████████  █████████  █████████████  █████  ██████  ██████  ██         ██████████████████████████
        //███  ████  ██  █████████  █████████  █████████████  █████  ████████  ████  ██  █████████████████████████████████
        //███  ████  ██  █████████  █████████  █████████████  █████  ██████████  ██  ██  █████████████████████████████████
        //███  ████  ██  █████████  █████████  █████████████  █████  ████████████    ██  █████████████████████████████████
        //█████    ████  █████████  █████████         ██         ██  ██████████████  ██         ██████████████████████████

        // Token: 440873630:AAHTMSXU3-Sp93PL3-9sU2r55vedMVoPfEA
        /*
            TODO LIST IMPORTANTE:
            Comando te telegram bot para ver recordatorios activos
            Confirmación de nuevo recordatorio

            LIMITACIONES
            - Sólo puede checar queries de 100 entidades a la vez con Azure. Es decir, sólo pueden haber unos 200 memos activos entre todos los usuarios

        */

        public Telegram.Bot.TelegramBotClient monumentumBot;
        public BotReadWrite monumentumReadWrite;

        public Telegram.Bot.Types.Update[] updateArray;
        public List<Update> updateList;
        public List<ScheduledMemo> memoList;
        public CloudTableClient cloudTableClient;
        public CloudStorageAccount storageAccount;
        public CloudTable cloudMemoTable;

        public BotLogic()
        {
            updateList = new List<Update>();
            memoList = new List<ScheduledMemo>();
            monumentumBot = new Telegram.Bot.TelegramBotClient("440873630:AAHTMSXU3-Sp93PL3-9sU2r55vedMVoPfEA");
            monumentumReadWrite = new BotReadWrite();
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            cloudTableClient = storageAccount.CreateCloudTableClient();
            cloudMemoTable = cloudTableClient.GetTableReference("memos");
            cloudMemoTable.CreateIfNotExists();

            //This is where the magic happens

            //First of all, the program checks for existing memos in the cloud and sends messages as needed. The memo list is saved as "memoList"
            DownloadMemos();
            //Multiple iterations of PostMessage() here, called by CheckMemos;

            //Then, it downloads the updates from Telegram's bot server. The downloaded list is called "updateList"
            DownloadUpdates();

            //memoList is checked against updateList and new memos are created in memoList
            UpdateMemos();

            //all memos in memoList get compared with the azure table. Completed memos replace incomplete memos, new memos fill new space.
            UploadMemos();
        }

        public void DownloadUpdates()
        {
            try
            {
                updateArray = monumentumBot.GetUpdatesAsync().Result;
            }
            catch(Exception ex)
            {
                throw;
            }

            foreach (Update element in updateArray)
            {
                updateList.Add(element);
            }

        }

        public void DownloadMemos()
        {
            TableQuery<ScheduledMemo> query = new TableQuery<ScheduledMemo>().Where(TableQuery.GenerateFilterConditionForBool("MemoCompleted", QueryComparisons.Equal, false));
            var queryResult = cloudMemoTable.ExecuteQuery(query);

            foreach (ScheduledMemo entity in queryResult)
            {
                    memoList.Add(entity);
            }


            foreach (ScheduledMemo memo in memoList)
            {
                if (DateTime.Compare(memo.ScheduledTime, DateTime.Now) <= 0)
                {
                    PostMessage(memo);
                    memo.MemoCompleted = true;
                }
            }
        }

        public void UpdateMemos()
        {
            foreach (Update element in updateList)
            {
                var newMemo = monumentumReadWrite.WriteMemo(element);
                if (newMemo.MemoValidity == false)
                {
                    TableOperation invalidMemoRetrieve = TableOperation.Retrieve("memo", newMemo.MemoID);
                    TableResult invalidMemoRetrieveResult = cloudMemoTable.Execute(invalidMemoRetrieve);

                    if (invalidMemoRetrieveResult.Result == null)
                    {
                        PostMessage(newMemo);
                        newMemo.MemoCompleted = true;
                        memoList.Add(newMemo);
                    }
                }
                else
                {
                    memoList.Add(newMemo);
                }
            }
        }

        public void UploadMemos()
        {
            TableBatchOperation finalMemoUpdate = new TableBatchOperation();
            bool performUpdate = false;
            foreach (ScheduledMemo updatedMemo in memoList)
            {
                TableOperation memoRetrieve = TableOperation.Retrieve("memo", updatedMemo.MemoID);
                TableResult retrievedMemo = cloudMemoTable.Execute(memoRetrieve);

                if (retrievedMemo.Result == null)
                {
                    finalMemoUpdate.Insert(updatedMemo);
                    performUpdate = true;
                    
                    //User confirmation. The memo is new because Result == null, and validity means it is a correctly formatter memo
                    if(updatedMemo.MemoValidity == true)
                    {
                        PostMessage(updatedMemo.ChatId, "Recordatorio creado. Tiempo: " + updatedMemo.ScheduledTime.ToString() + " Mensaje: " + updatedMemo.ScheduledMessage);
                    }

                }
                else
                {
                    if ((((DynamicTableEntity)retrievedMemo.Result).Properties["MemoCompleted"].BooleanValue != true) && (updatedMemo.MemoCompleted == true))
                    {
                        finalMemoUpdate.Replace(updatedMemo);
                        performUpdate = true;
                    }
                }
            }

            if (performUpdate == true)
            {
                cloudMemoTable.ExecuteBatch(finalMemoUpdate);
            }
        }

        public void PostMessage(ScheduledMemo scheduledMemo)
        {
            var k = monumentumBot.SendTextMessageAsync(scheduledMemo.ChatId, scheduledMemo.ScheduledMessage).Result;
        }

        public void PostMessage(string ChatID, string message)
        {
            var k = monumentumBot.SendTextMessageAsync(ChatID, message).Result;
        }
    }
}
