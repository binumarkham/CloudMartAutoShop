# Technician role workflow

This test build adds role-based technician access.

## Roles
- `Admin` / `Administrator`: full current management access.
- `Service Advisor`: current management access except future user administration.
- `Technician`: only the dedicated **My Repair Orders** workflow.

## Security
The API enforces the restrictions. Technician users cannot call the normal Customers, Vehicles, RepairOrders, Suppliers or Reports controllers. The technician API only returns repair orders where `AssignedTechnicianUserId` matches the signed-in user's JWT user id, and it does not return labor rates, parts prices, totals, invoices or payments.

## Data-model changes
- `RepairOrder.AssignedTechnicianUserId` (nullable int)
- `RepairOrder.AssignedTechnicianName` (nullable string)
- `RepairOrderLabor.TechnicianNotes` (nullable string)

## Required local database migration
From `C:\CloudMartAutoShop_v04\CloudMartAutoShop.Api` run:

    dotnet ef migrations add AddTechnicianWorkflow
    dotnet ef database update

Use the existing local User Secrets. Do not put passwords/JWT keys into source files.

## Technician accounts
A technician must be an active row in `Users` for the same Business with `Role = 'Technician'`. The Admin/Service Advisor Repair Order screen gets its assignment dropdown from `GET api/Technicians`.

## Important billing behavior
A Technician can record task description, hours and technician notes, but cannot see or submit an hourly billing rate. Technician-created labor lines intentionally start with `HourlyRate = 0` and `LineTotal = 0`. An Admin/Service Advisor labor-rate editing workflow should be added before production billing is finalized.
