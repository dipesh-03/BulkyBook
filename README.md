# BulkyBook
Website from which user can buy books in Bulk.

E-Commerce website that has four types of users (Admin, Employee, End User, Company User)
Use of MSSQL server to store the data

## Features
+ Has implementation of **Identity Management** using dotnet MVC
+ **30 Days payment** policy for company users & Direct payment for End users
+ Implemented **repository & UnitOfWork** to manage models at the same place
+ **Payment page** integration done using stripe
+ Users will **receive mails** when order is placed and approved. (Via SMTP)
+ **Different price** for different quantities

### Admin/Employee 
+ Admin/Employee has access to **modify** order details
+ Admin/Employee can update the **order status** (Processing, Approved, Shipped, Delivered)
+ Admin/Employee can manage products, categories, companies, orders etc.

### Users
+ Users can **browse accross** website for different products
+ Users can **check the orders** they have placed with details and filteration
+ Company users can make payment within 30 days

