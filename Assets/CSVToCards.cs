using FileHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Outraged
{
    public static class CSVToCards
    {
        public static void Convert()
        {
            IEnumerable<CardData> result = new List<CardData>();
            var engine = new FileHelperEngine<CardData>();

            var questions = Resources.Load("Data/CSVQuestions").ToString();
            if (questions != null)
            {
                var csv = engine.ReadString(questions);
                for (int i = 0; i < csv.Length; i++) { csv[i].Type = Card.CardType.Question; }
                result = result.Concat(csv);
            }

            var answers = Resources.Load("Data/CSVAnswers").ToString();
            if (answers != null)
            {
                var csv = engine.ReadString(answers);
                for (int i = 0; i < csv.Length; i++) { csv[i].Type = Card.CardType.Answer; }
                result = result.Concat(csv);
            }

            string output = JsonConvert.SerializeObject(result, DataHeaders.GetFormatting(Formatting.Indented, DefaultValueHandling.Include)); // include or CardType.Question (== 0) will not be serialized
            string LogFolderPath = Path.Combine(Path.Combine(Application.dataPath, ".."), "Logs");
            if (!Directory.Exists(LogFolderPath))
            {
                Directory.CreateDirectory(LogFolderPath);
            }
            var fileName = Path.Combine(LogFolderPath, "NewCards-" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            using (var stream = new StreamWriter(fileName, true))
            {
                stream.Write(output + Environment.NewLine);
            }
        }
    }
}
