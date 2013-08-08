using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using smsSurvey.dbInterface;

namespace smsSurvery.Surveryer.DbModels
{
   public class Question
   {
      public int Id {get; set;}
      public string Text { get; set; }
      public int Order { get; set; }
      public string Type { get; set; }
      public string ValidAnswers { get; set; }
      public string ValidAnswersDetails { get; set; }
      public List<QuestionAlert> QuestionAlertSet { get; set; }
      public Question() { }

      public Question(int iId,
         string iText,
         int iOrder,
         string iType,
         string iValidAnswers,
         string iValidAnswersDetails,
         List<QuestionAlert> iQuestionAlertSet)
      {
         Id = iId;
         Text = iText;
         Order = iOrder;
         Type = iType;
         ValidAnswers = iValidAnswers;
         ValidAnswersDetails = iValidAnswersDetails;
         QuestionAlertSet = iQuestionAlertSet;
      }     
   }
}