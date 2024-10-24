namespace JobHunter.Infrastructure.Linkedin.Configurations;

public class PageNodes
{
    
    public const string JobTitleNode = ".job-card-list__title";
    public const string CompanyNode = ".job-card-container__primary-description ";
    public const string LocationNode = ".job-card-container__metadata-item ";
    public const string JobUrlNode = ".job-card-container__link";
    public const string PostedDateNode = ".job-card-container__footer-item";
    public const string Description = ".jobs-description__content";
    public const string ClickableDescription = ".job-card-container--clickable";
    
    public const string EmploymentTypeNode = "//span[contains(@class, 'description__job-criteria-text') and (text()='Full-time' or text()='Part-time')]";
    public const string LocationTypeNode = "//span[contains(@class, 'job-criteria__text') and (text()='Remote' or text()='On-site' or text()='Hybrid')]";
    public const string NumberOfEmployeesNode = "//span[contains(@class, 'num-applicants__caption')]";
    
    public const string JobListNode = ".jobs-search-results-list";
    public const string JobListItems = ".jobs-search-results__list-item";
}