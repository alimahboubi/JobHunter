using System.Runtime.CompilerServices;
using JobHunter.Domain.Job.Enums;

namespace JobHunter.Domain.Job.Constants;

public static class KeywordConstants
{
    private static readonly string[] SoftwareKeywords =
    [
        // Backend Development
        ".NET", "Java", "C#", "Python", "Django", "Flask", "FastAPI", "C++", "Laravel", "PHP", "Ruby", "Ruby on Rails",
        "Swift", "Perl", "GoLang", "NodeJs", "ExpressJs", "ASP.NET",

        // Frontend Development
        "HTML", "CSS", "JavaScript", "React", "NextJs", "Angular", "VueJs", "TypeScript",

        // Database
        "SQL", "NoSQL", "MongoDB", "PostgreSQL", "MySQL", "SQLite", "Redis", "RabbitMQ", "Kafka", "Elasticsearch",

        // Data Science, Machine Learning, Data Analysis
        "Pandas", "NumPy", "SciPy", "scikit-learn", "TensorFlow", "Keras", "PyTorch", "Matplotlib", "Seaborn",
        "Jupyter", "R", "RStudio", "SPSS", "SAS", "Stata", "Excel", "Tableau", "Power BI", "Hadoop", "Spark", "Hive",
        "Pig", "HBase", "Mahout", "MLlib", "Data Mining", "Data Wrangling", "Data Visualization",
        "Predictive Analytics", "Statistical Analysis", "Deep Learning", "Neural Networks",
        "Natural Language Processing", "Computer Vision", "Big Data", "Time Series Analysis", "Bayesian Inference",
        "A/B Testing", "Feature Engineering", "Model Deployment", "AutoML", "Data Cleaning", "ETL",
        "Dimensionality Reduction", "Clustering", "Classification", "Regression", "Machine Learning Algorithms",
        "Data Preprocessing", "Data Collection", "Data Warehousing",

        // Tools and Technologies
        "AWS", "Azure", "Docker", "Kubernetes", "Jenkins", "Git", "GitHub", "GitLab", "Bitbucket", "Jira", "Confluence",
        "Slack", "Trello", "Azure DevOps", "AWS CodePipeline", "AWS CodeBuild", "AWS CodeDeploy", "AWS CodeCommit",
        "AWS CodeStar", "AWS CodeArtifact", "AWS CodeGuru", "Ubuntu", "Debian", "CentOS", "RedHat", "Fedora", "Windows",
        "MacOS", "Linux", "Unix", "Shell Scripting", "PowerShell", "Bash", "Zsh", "Terraform", "Ansible", "Chef",
        "Puppet", "SaltStack", "Nginx", "Apache", "IIS", "Logstash", "Kibana", "Prometheus", "Grafana", "Splunk",
        "Datadog", "New Relic", "Sentry", "AppDynamics", "Dynatrace", "Postman", "Swagger", "OpenAPI", "GraphQL",
        "gRPC", "SOAP", "WebSockets", "WebRTC", "OAuth", "JWT", "SAML", "OpenID", "LDAP", "Active Directory",

        // Others
        "OAuth2", "OIDC", "SAML2", "OpenID2", "LDAP2", "Active Directory2", "OAuth3", "OIDC3", "SAML3", "OpenID3",
        "LDAP3", "Active Directory3", "OAuth4", "OIDC4", "SAML4", "OpenID4", "LDAP4", "Active Directory4", "OAuth5",
        "OIDC5", "SAML5", "OpenID5", "LDAP5", "Active Directory5", "OAuth6", "OIDC6", "SAML6", "OpenID6", "LDAP6",
        "Active Directory6", "OAuth7", "OIDC7", "SAML7", "OpenID7", "LDAP7", "Active Directory7", "OAuth8", "OIDC8",
        "SAML8", "OpenID8", "LDAP8", "Active Directory8", "OAuth9", "OIDC9", "SAML9", "OpenID9", "LDAP9",
        "Active Directory9", "OAuth10", "OIDC10", "SAML10", "OpenID10", "LDAP10", "Active Directory10", "OAuth11",
        "OIDC11", "SAML11", "OpenID11", "LDAP11", "Active Directory11", "OAuth12", "OIDC12", "SAML12", "OpenID12",
        "LDAP12", "Active Directory"
    ];

    private static Dictionary<JobCategory, string[]> keywordMap = new()
    {
        { JobCategory.Software, SoftwareKeywords }
    };

    public static string[]? GetKeywords(JobCategory jobCategory)
    {
        keywordMap.TryGetValue(jobCategory, out var keywords);
        return keywords;
    }
}