# Digital Verification Register - Admin Portal

> Note: The DVS Register repo must be installed before this repo.

**Requirements:**
  - Visual Studio or JetBrain Rider
  - download and install .NET 8 - https://learn.microsoft.com/en-us/dotnet/core/install/macos
  - dotnet tool install --global dotnet-ef --version 8.*
  - Node
  - NPM

## Getting Started
1. Clone repo
2. Install dev certs for https - why
   - `dotnet dev-certs https --trust`
3. Install docker desktop - is this to install the docker destop application
   - `docker run -d  --name postgres-container -e POSTGRES_PASSWORD=postgres  -p 5432:5432  postgres:15-alpine`
4. Install postico or pgadmin4 (or equivalent)
   - create local DB/server
      - host: localhost
      - database: postgres
      - user: postgres
      - password: postgres
5. Add user secrets - ask member of team for them and update the postgres details with the correct credentials - the username and password you created when setting up the database.
    - To add user secret file to the project, right click the project -> add user secrets
6. `cd` into `dsit-dvs-register` repo - the migrations are in register repo, in this repo only the updates
   - Run DB update - `dotnet ef database update --project DVSRegister.Data  --startup-project DVSRegister`
7. Install localstack
    ## Installation steps : Mac
    - `brew install localstack/tap/localstack-cli`
    - `localstack start -d`
    - run `docker ps` to get the localstack container id (or copy the container ID from Docker Desktop)
    - `docker exec -it 'CONTAINER_ID' bash`
    - `awslocal s3api create-bucket --bucket s3-dvs-dev20240529103145426300000001`
    - http://s3-dvs-dev20240529103145426300000001.s3.localhost.localstack.cloud:4566
    
    ## Installation steps windows

        1. Start power shell, execute below commands to run local stack
        - PS C:\Users\AA> python -m venv myenv
        - PS C:\Users\AA> myenv\Scripts\activate
        - (myenv) PS C:\Users\AA> pip install localstack
        - (myenv) PS C:\Users\AA> .\localstack start

        2. Start command prompt, execute below commands to configure aws credentials and create bucket
        - C:\Users\AA>aws configure
        - AWS Access Key ID [****************test]: test
        - AWS Secret Access Key [****************test]: test
        - Default region name [eu-west-2]: eu-west-2
        - Default output format [json]: json
        - C:\Users\AA>aws --endpoint-url=http://localhost:4566 s3api create-bucket --bucket s3-dvs-dev20240529103145426300000001 --create-bucket-configuration LocationConstraint=eu-west-2
        - C:\Users\AA>aws --endpoint-url=http://localhost:4566 s3api list-buckets (to verify buckets created)
        - Update the credentials in user secrets
8. Login :  Ask team member to add the dsit email id to user pool and sign up using MFA

> Note: Make sure to update the s3 bucket in user secrets to use path style instead of host style.

## GOV.UK Frontend and Styling
This project relies on the [GOV.UK Frontend NPM package](https://www.npmjs.com/package/govuk-frontend) which contains images, fonts, styling, and JavaScript.

### Updating styles
The project loosely follows and ITCSS folder structure(LINK) and BEM(LINK) for writing CSS
If you're adding new styles to the codebase be sure to run `npm run compile-sass` inside the project folder `DVSAdmin`.

## Contributing
On this project we use [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary). Conventional Commits establish a formal convention for structuring commit messages.

## Development credentials
We store credentials for the dev site inside the UserSecrets of the project. Speak to someone on the team to get these.

## Database
At the root of **The DVS Register repo** - run `dotnet ef database update --project DVSRegister.Data  --startup-project DVSRegister`

