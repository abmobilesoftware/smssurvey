$(document).ready(function () {
   var urlParts = document.URL.split("/");
   var surveyId = urlParts[urlParts.length - 1];
   var surveyModel = new SurveyBuilder.SurveyModel({
      Id: surveyId,
      Description: "Beer survey",
      ThankYouMessage: "Thank you!",
      StartDate: "15/6/2013",
      EndDate: "17/7/2013",
      IsRunning: false
   });
   var survey = new SurveyBuilder.SurveyView({
      el: $("#survey"),
      model: surveyModel
   })
});
