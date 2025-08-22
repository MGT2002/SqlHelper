# SqlHelper
SqlHelper
Overview
SqlHelper is a utility for T-SQL, designed to simplify generating SQL scripts for database management. Currently it automates CREATE TABLE, CREATE COLUMN, and DROP COLUMN scripts, consolidating all related SQL objects within a single table context.
Features

Generate CREATE TABLE Scripts: Automate T-SQL table creation with customizable attributes.
Add Columns: Generate T-SQL ALTER TABLE statements to add columns.
Drop Columns: Automate T-SQL ALTER TABLE DROP COLUMN scripts.
Single Table Context: Manage related SQL objects (e.g., constraints, indexes) in one table context.
T-SQL Exclusive: Optimized for Microsoft SQL Server.

# Usage

1. Add all the required information to the appsettings.json file.

<img width="430" height="305" alt="image" src="https://github.com/user-attachments/assets/1a206543-49f5-4e5d-9091-8b68bc11adbb" />

2. Call RunScripter, passing your desired scripter as the generic type parameter. Run the app.

<img width="872" height="249" alt="image" src="https://github.com/user-attachments/assets/2c9f3429-b155-41c2-b331-b07056cba588" />

3. The resulting SQL files will be generated and placed in the Generated folder of  the project.

<img width="1764" height="420" alt="image" src="https://github.com/user-attachments/assets/57afbee9-089b-4d5a-9c8d-5c875ae1ee40" />

<img width="995" height="312" alt="image" src="https://github.com/user-attachments/assets/9a2cd89a-6935-4568-b4d9-725b43873593" />


# Contributing

Fork the repository.
Create a new branch.
Open a Pull Request.

# License

This project is licensed under the MIT License - see the LICENSE file for details.
Contact
