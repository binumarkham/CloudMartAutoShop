CloudMart Auto Shop Manager - integrated API + Blazor WebAssembly PWA build

Included:
- Existing EF Core model and InitialCreate migration preserved
- JWT login and tenant claim architecture preserved
- Multi-tenant Customers, Vehicles, Suppliers, Repair Orders, Labor, Parts, Payments and Reports APIs
- Server-side repair-order total/payment recalculation
- Dashboard summary and outstanding report
- Blazor WASM login, dashboard, customer, vehicle, repair-order, supplier and report pages
- CloudMart navy/blue/teal responsive UI
- Local API base URL: http://localhost:5169/

Important local setup remains outside source ZIP:
- ConnectionStrings:Postgres in .NET User Secrets
- Jwt:* settings in .NET User Secrets
- SeedAdmin:* settings in .NET User Secrets

Database schema was not changed beyond the existing InitialCreate migration, so no additional migration is required for this version.

Build verification note:
The ChatGPT sandbox used to assemble this ZIP does not have the dotnet CLI installed, so run `dotnet build CloudMartAutoShop.sln` on the Windows development machine before testing.
