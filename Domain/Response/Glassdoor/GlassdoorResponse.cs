namespace AutomatedWebscraper.Domain.Response.Glassdoor
{
    public class CareerOpportunitiesDistribution
    {
        public int one_star { get; set; }
        public int two_star { get; set; }
        public int three_star { get; set; }
        public int four_star { get; set; }
        public int five_star { get; set; }
    }

    public class Input
    {
        public string url { get; set; }
    }

    public class InterviewsExperience
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class GlassdoorResponse
    {
        public Input input { get; set; }
        public string id { get; set; }
        public string company { get; set; }
        public int ratings_overall { get; set; }
        public string details_size { get; set; }
        public int details_founded { get; set; }
        public string details_type { get; set; }
        public string country_code { get; set; }
        public string company_type { get; set; }
        public string url_overview { get; set; }
        public string url_reviews { get; set; }
        public string details_headquarters { get; set; }
        public string details_industry { get; set; }
        public string details_revenue { get; set; }
        public string details_website { get; set; }
        public string interviews_url { get; set; }
        public double ratings_career_opportunities { get; set; }
        public double ratings_ceo_approval { get; set; }
        public int ratings_ceo_approval_count { get; set; }
        public double ratings_compensation_benefits { get; set; }
        public int ratings_cutlure_values { get; set; }
        public string diversity_inclusion_score { get; set; }
        public string diversity_inclusion_count { get; set; }
        public double ratings_senior_management { get; set; }
        public double ratings_work_life_balance { get; set; }
        public double ratings_business_outlook { get; set; }
        public double ratings_recommend_to_friend { get; set; }
        public string ratings_rated_ceo { get; set; }
        public int salaries_count { get; set; }
        public CareerOpportunitiesDistribution career_opportunities_distribution { get; set; }
        public string interview_difficulty { get; set; }
        public int interviews_count { get; set; }
        public int benefits_count { get; set; }
        public int jobs_count { get; set; }
        public string photos_count { get; set; }
        public int reviews_count { get; set; }
        public List<InterviewsExperience> interviews_experience { get; set; }
        public string url { get; set; }
        public string industry { get; set; }
        public object additional_information { get; set; }
        public object stock_symbol { get; set; }
        public DateTime timestamp { get; set; }
    }

}
