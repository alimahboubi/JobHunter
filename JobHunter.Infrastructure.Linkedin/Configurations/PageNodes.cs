namespace JobHunter.Infrastructure.Linkedin.Configurations;

public class PageNodes
{
    public const string JobCards = "//div[contains(@class, 'base-card') and contains(@class, 'job-search-card')]";
    
    public const string JobTitleNode = ".//h3[contains(@class, 'base-search-card__title')]";
    public const string CompanyNode = ".//h4[contains(@class, 'base-search-card__subtitle')]";
    public const string LocationNode = ".//span[contains(@class, 'job-search-card__location')]";
    public const string JobUrlNode = ".//a[contains(@class, 'base-card__full-link')]";
    public const string PostedDateNode = ".//time[contains(@class, 'job-search-card__listdate')]";
    
    public const string EmploymentTypeNode = "//span[contains(@class, 'description__job-criteria-text') and (text()='Full-time' or text()='Part-time')]";
    public const string LocationTypeNode = "//span[contains(@class, 'job-criteria__text') and (text()='Remote' or text()='On-site' or text()='Hybrid')]";
    public const string NumberOfEmployeesNode = "//span[contains(@class, 'num-applicants__caption')]";
    
    public const string JobDescriptionNode = ".job-description";
}