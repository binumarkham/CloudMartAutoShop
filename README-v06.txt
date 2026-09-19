CloudMart Auto Shop Manager v06 - Parts, Suppliers and Reports

Based on the user's working v05 ZIP and preserves:
- Admin / Service Advisor / Technician roles
- Technician My Repair Orders workflow
- Separate Work and Labor success messages
- Repair Order serialization-cycle response fix
- Multi-tenant BusinessId filtering

Added in v06:
1. Repair Order Parts workflow for Admin/Service Advisor
   - Part number, description, quantity, unit cost, unit price, line total
   - Server recalculates PartsSubtotal, Subtotal and TotalAmount
   - Technician UI remains financial-data restricted
2. Supplier administration
   - Create, edit, activate/deactivate
   - Contact/address/notes fields
   - BusinessId tenant isolation
3. Reports
   - Outstanding Balances
   - Repair Order Summary with date/status filters
   - Sales Summary with date filters
   - Payments Received
   - Parts Usage / Sales
   - Technician Labor Hours

Database migration:
- No new migration is required for these v06 changes because Supplier and RepairOrderPart tables/fields already exist in the v05 schema.

Suggested test sequence:
1. dotnet build CloudMartAutoShop.sln
2. Run API and PWA against autoshopdb_test only.
3. Test Supplier create/edit/deactivate/reactivate.
4. Open an existing Repair Order and add a part. Verify Parts/Subtotal/Total update.
5. Run each report and verify date/status filtering.
6. Login as Technician and confirm Suppliers/Reports/financial Parts remain unavailable.
