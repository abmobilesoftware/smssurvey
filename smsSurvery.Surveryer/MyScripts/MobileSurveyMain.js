$(function () {
   //initialize mobile survey given the id of the survey plan
   var surveyId = $("#surveyTemplateId").val();
   var survey = new SurveyBuilder.SurveyModel({
      Id: surveyId,
      Description: "",
      ThankYouMessage: "",
      StartDate: "15/6/2013",
      EndDate: "17/7/2013",
      IsRunning: false
   });
   var urlParts = document.URL.split("/");
   var urlLastPart = urlParts[urlParts.length - 1];
   var surveyResultID = parseInt($("#isFeedback").val()) === 1 ? -1 : urlLastPart;
   survey.fetch({
      data: {
         Id: surveyId
      },
      success: function (model, response, options) {
         var surveyModel = new Question.QuestionSetModel(
            {
               SurveyResultId: surveyResultID,
               SurveyTemplateId: surveyId
            });
         surveyModel.updateQuestionSetCollection(model.get("QuestionSet"));
         //DA sure they are ordered
         var surveyView = new MobileSurvey.SurveyView({
            el: $("#survey"),
            model: surveyModel
         });
         surveyView.render();
         //location code
         //if (navigator.geolocation)
         //{
         //   navigator.geolocation.getCurrentPosition(function (position) {
         //      alert("Latitude: " + position.coords.latitude + " Longitude: " + position.coords.longitude);
         //   });
         //}
         //else
         //{ alert("no location available") }
      },
      error: function (model, response, options) {
         alert(response)
      }
   });
});