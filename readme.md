# Waracle Tech Test

URL: https://waracle-api-uksouth-dev-cygward5b5efd0cx.uksouth-01.azurewebsites.net/

This is a simple API for a hotel booking system. It allows you to search for hotels and available rooms, and make bookings.

Data is seeded in the database. Run delete and seed in the database to reset.

The seed to will create 2 hotels with 6 rooms each. Each room is one of three room types:

- Single - 1 guest
- Double - 2 guests
- Deluxe - 3 guests

## Architecture 

- 🧑‍💻 API is built using ASP.NET Core 8.
- 📊 Database is code first using Entity Framework Core 9.
- 🛠️ Unopinionated pattern for application logic. 

## Infrastructure

- 📊 Database is hosted on Azure SQL Database.
- ☁ API is hosted on Azure App Service.
- 🏃 API is deployed using GitHub Actions.

## Testing

- ❌ Have chosen not to implement unit/integration tests for this project.
- ✅ Have manual tested using swagger for speed. 

## Future Improvements

- Implement unit/integration/e2e tests.
- Implement logging.
- Implement api versioning.
- Implement application insights.
- Implement pagination.
- Implement pattern for application logic

This code is not particulary SOLID, nor does it follow any specific design patterns. It is a simple API for a hotel booking system. I don't feel the additional complexity is necessary for this project. IRL I would consider a more SOLID and maintainable architecture.





