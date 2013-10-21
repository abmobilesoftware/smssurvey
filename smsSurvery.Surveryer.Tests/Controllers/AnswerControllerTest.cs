using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using smsSurvey.dbInterface;
using smsSurvery.Surveryer.Controllers;

namespace smsSurvery.Surveryer.Tests.Controllers
{
   [TestClass]
   public class AnswerControllerTest
   {
      [TestMethod]
      public void PrepareSMSTextForQuestion_En_Rating_NotFirstQuestion_QuestionConformsToTemplate()
      {
         var expectedAnswer = 
@"Q1/5: Question text
Reply with:
1 for 1 out of 5 stars
2 for 2 out of 5 stars
3 for 3 out of 5 stars
4 for 4 out of 5 stars
5 for 5 out of 5 stars";
         var q = new Question();
         q.Order = 1;
         q.Text = "Question text";
         q.Type = ReportsController.cRatingsTypeQuestion;
         q.ValidAnswers = "1;2;3;4;5";
         q.ValidAnswersDetails = "Very bad;Bad;OK;Good;Very good";
         var intro = "Hello";
         var totalNrOfQuestions = 5;
         var isFirstQuestion = false;
         System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
         var result = AnswerController.PrepareSMSTextForQuestion(q, totalNrOfQuestions, isFirstQuestion, intro);
         Assert.AreEqual(expectedAnswer, result);
      }

      [TestMethod]
      public void PrepareSMSTextForQuestion_Ro_Rating_NotFirstQuestion_QuestionConformsToTemplate()
      {
         var expectedAnswer =
@"Intrebarea 1/5: Text intrebare
Raspundeti cu:
1 pentru 1 din 5 stele
2 pentru 2 din 5 stele
3 pentru 3 din 5 stele
4 pentru 4 din 5 stele
5 pentru 5 din 5 stele";
         var q = new Question();
         q.Order = 1;
         q.Text = "Text intrebare";
         q.Type = ReportsController.cRatingsTypeQuestion;
         q.ValidAnswers = "1;2;3;4;5";
         q.ValidAnswersDetails = "Very bad;Bad;OK;Good;Very good";
         var intro = "Salut";
         var totalNrOfQuestions = 5;
         var isFirstQuestion = false;
         System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("ro-RO");
         var result = AnswerController.PrepareSMSTextForQuestion(q, totalNrOfQuestions, isFirstQuestion, intro);
         Assert.AreEqual(expectedAnswer, result);
      }

      [TestMethod]
      public void PrepareSMSTextForQuestion_En_Numeric_NotFirstQuestion_QuestionConformsToTemplate()
      {
         var expectedAnswer =
@"Q1/5: Question text
Reply with:
1 for Very bad
2 for Bad
3 for OK
4 for Good
5 for Very good";
         var q = new Question();
         q.Order = 1;
         q.Text = "Question text";
         q.Type = ReportsController.cNumericTypeQuestion;
         q.ValidAnswers = "1;2;3;4;5";
         q.ValidAnswersDetails = "Very bad;Bad;OK;Good;Very good";
         var intro = "Hello";
         var totalNrOfQuestions = 5;
         var isFirstQuestion = false;
         System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
         var result = AnswerController.PrepareSMSTextForQuestion(q, totalNrOfQuestions, isFirstQuestion, intro);
         Assert.AreEqual(expectedAnswer, result);
      }

      [TestMethod]
      public void PrepareSMSTextForQuestion_Ro_Numeric_NotFirstQuestion_QuestionConformsToTemplate()
      {
         var expectedAnswer =
@"Intrebarea 1/5: Text intrebare
Raspundeti cu:
1 pentru Naspa rau
2 pentru Naspa
3 pentru OK
4 pentru Marfa
5 pentru Super marfa";
         var q = new Question();
         q.Order = 1;
         q.Text = "Text intrebare";
         q.Type = ReportsController.cNumericTypeQuestion;
         q.ValidAnswers = "1;2;3;4;5";
         q.ValidAnswersDetails = "Naspa rau;Naspa;OK;Marfa;Super marfa";
         var intro = "Hello";
         var totalNrOfQuestions = 5;
         var isFirstQuestion = false;
         System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("ro-RO");
         var result = AnswerController.PrepareSMSTextForQuestion(q, totalNrOfQuestions, isFirstQuestion, intro);
         Assert.AreEqual(expectedAnswer, result);
      }

      [TestMethod]
      public void PrepareSMSTextForQuestion_En_Rating_FirstQuestion_QuestionConformsToTemplate()
      {
         var expectedAnswer =
@"Hello

Q1/5: Question text
Reply with:
1 for 1 out of 5 stars
2 for 2 out of 5 stars
3 for 3 out of 5 stars
4 for 4 out of 5 stars
5 for 5 out of 5 stars";
         var q = new Question();
         q.Order = 1;
         q.Text = "Question text";
         q.Type = ReportsController.cRatingsTypeQuestion;
         q.ValidAnswers = "1;2;3;4;5";
         q.ValidAnswersDetails = "Very bad;Bad;OK;Good;Very good";
         var intro = "Hello";
         var totalNrOfQuestions = 5;
         var isFirstQuestion = true;
         System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
         var result = AnswerController.PrepareSMSTextForQuestion(q, totalNrOfQuestions, isFirstQuestion, intro);
         Assert.AreEqual(expectedAnswer, result);
      }

      [TestMethod]
      public void PrepareSMSTextForQuestion_Ro_Rating_FirstQuestion_QuestionConformsToTemplate()
      {
         var expectedAnswer =
@"Salut

Intrebarea 1/5: Text intrebare
Raspundeti cu:
1 pentru 1 din 5 stele
2 pentru 2 din 5 stele
3 pentru 3 din 5 stele
4 pentru 4 din 5 stele
5 pentru 5 din 5 stele";
         var q = new Question();
         q.Order = 1;
         q.Text = "Text intrebare";
         q.Type = ReportsController.cRatingsTypeQuestion;
         q.ValidAnswers = "1;2;3;4;5";
         q.ValidAnswersDetails = "Very bad;Bad;OK;Good;Very good";
         var intro = "Salut";
         var totalNrOfQuestions = 5;
         var isFirstQuestion = true;
         System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("ro-RO");
         var result = AnswerController.PrepareSMSTextForQuestion(q, totalNrOfQuestions, isFirstQuestion, intro);
         Assert.AreEqual(expectedAnswer, result);
      }

   }
}
