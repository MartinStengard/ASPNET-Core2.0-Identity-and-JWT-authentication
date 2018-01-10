# Use both Cookie and JWT token authentication.
This MVC project apply both Cookie and JWT authentication.

## Project
* Cookie -> through login page out-of-the-box when creating a MVC project for Core 2.0.
* JWT - > login without login form, eg service. To expose controller / action or other API to accept JWT authentication you must add [Authorize(Policy = "Jwt")]
* JWT protected API is registered as Policy named "Jwt".
* Cors is set to accept calls from same origin but can easily be changed by adding more origins.
* Database is attached in wwwroot/app_data folder.
* User test@test.test and two roles exists in database or will be created on startup.

## API's
* GET -> /Test/IsCookieAuthenticated
User need to be login by using form login.
* GET -> /Test/IsJwtAuthenticated
User need to login and send request with Jwt Bearer in header.
* GET -> /Test/IsAnonymous
Open for all.
* GET -> /Test/IsAdmin
Check to see if cookie autheticated user have Admin role.
* POST -> /Account/LoginWithJwt
Login and get token to be supplied in request header.