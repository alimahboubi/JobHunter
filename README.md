
# JobHunter

JobHunter is a job search and management tool designed to streamline the process of finding and tracking job applications. This application leverages various frameworks and tools to enhance its functionality, including integrations with LinkedIn, caching with Redis, and persistence through PostgreSQL.

## Features

- **LinkedIn Integration**: Pulls job listings and profile data to personalize job recommendations.
- **In-Memory and Redis Caching**: Improves data retrieval speeds.
- **Application Tracking**: Organizes job applications with status updates and notes.
- **OpenAI Integration**: Analyzes job descriptions to match user profiles.
- **PostgreSQL Database**: Manages persistent data for user accounts and job applications.

## Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/alimahboubi/JobHunter.git
   cd JobHunter
   ```

2. **Set Up Environment Variables**: Create a `.env` file and configure it as follows:
   ```
   DATABASE_URL=postgres://user:password@localhost:5432/jobhunter
   REDIS_URL=redis://localhost:6379
   OPENAI_API_KEY=your_openai_api_key
   ```

3. **Install Dependencies**: Install the necessary dependencies using Docker.
   ```bash
   docker-compose up -d
   ```

4. **Run the Application**:
   ```bash
   dotnet run --project JobHunter.WebApp
   ```

## Usage

- **Login/Register**: Create an account or log in to access your dashboard.
- **Job Listings**: Use the LinkedIn integration.
- **Track Applications**: Update job statuses and add notes to keep track of each application.

## Project Structure

- **JobHunter.Application**: Core application logic.
- **JobHunter.Domain**: Domain models and business rules.
- **JobHunter.Infrastructure**: Integrations with LinkedIn, Redis, and PostgreSQL.
- **JobHunter.WebApp**: The main web interface for the application.

## Contributing

Contributions are welcome! Please fork the repository, create a new branch, and submit a pull request.

## License

This project is licensed under the MIT License.
