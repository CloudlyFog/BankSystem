## Bank system
This package was developed for common using and learning definite technology.
It also let everybody to use bank system in its projects and change logic for your needs.
## When cause exception or error?
There are some points when you can catch an exception or error while using api of project:

 1. You use default connection string instead of your. Can happen so that on Your machine won't database with name which was specified in default connection string.
 2. You change content of repositories or context class. If You change some of these You can get an error. <br> **Example**: <br> 
 ````
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(queryConnection);
        }
````
 You'll write something like this
 ````
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer();
        }
````
 3. You use methods incorrectly. <br> **Example**:<br> You wanna delete operation therefore You have to use method `DeleteOperation(...)` but You use method `CreateOperation(...)`  and of course You'll get an exception because method `CreateOperation(...)`has return type `ExceptionModel` and it'll returns `ExceptionModel.OperationFailed` because same operation already exist in the database which You are using.

#### **Remember!**
Always change connection string either directly in context class (**You are responsible for it!**) or use constructors of repo-classes.

## Conclusion

Dowloading and next using this package is your respoinsible and only you decide use it or not. All exceptions and crashes of your projects is responsible on you. We was tested our product in many tests and have a conclusion in which says that all methods and logic of project are working correctly. So, we wish you luck.<br>
**Sincerely, CloudlyFog.**