﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace smsSurvey.dbInterface
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class smsSurveyEntities : DbContext
    {
        public smsSurveyEntities()
            : base("name=smsSurveyEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Question> QuestionSet { get; set; }
        public DbSet<SurveyPlan> SurveyPlanSet { get; set; }
        public DbSet<Customer> CustomerSet { get; set; }
        public DbSet<SurveyResult> SurveyResultSet { get; set; }
        public DbSet<Result> ResultSet { get; set; }
        public DbSet<UserProfile> UserProfile { get; set; }
        public DbSet<webpages_Membership> webpages_Membership { get; set; }
        public DbSet<webpages_OAuthMembership> webpages_OAuthMembership { get; set; }
        public DbSet<webpages_Roles> webpages_Roles { get; set; }
    }
}
