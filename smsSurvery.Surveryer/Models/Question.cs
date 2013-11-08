using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using smsSurvey.dbInterface;

namespace smsSurvery.Surveryer.ClientModels
{
   public class ClientQuestion
   {
      public int Id {get; set;}
      public string Text { get; set; }
      public int Order { get; set; }
      public string Type { get; set; }
      public string ValidAnswers { get; set; }
      public string ValidAnswersDetails { get; set; }
      public List<ClientQuestionAlert> QuestionAlertSet { get; set; }
      public bool Required { get; set; }
      public ClientQuestion() { }

      public ClientQuestion(int iId,
         string iText,
         int iOrder,
         string iType,
         string iValidAnswers,
         string iValidAnswersDetails,
         List<ClientQuestionAlert> iQuestionAlertSet,
         bool iRequred)
      {
         Id = iId;
         Text = iText;
         Order = iOrder;
         Type = iType;
         ValidAnswers = iValidAnswers;
         ValidAnswersDetails = iValidAnswersDetails;
         QuestionAlertSet = iQuestionAlertSet;
         Required = iRequred;
      }     
   }
}