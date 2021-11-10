In .NET 6, in the Properties folder, launchSettings.json it creates a flag <ImplicitUsings>enable</ImplicitUsings>,
this flag creates a new file, in the obj folder called APIGlobalUsings.g.cs that allows the use of global using modules
that minimise the ammount of reiteration of the same using modules in different classes of files in the project

The flag <nullable> allows to prevent a nullable reference to throw an exception when receiving a null value
 
A nullable reference is when a type of variable is followed by a '?', meaning that it may receive a null value
when requested

In the Properties folder appsettings.Development.json file changing the attribute 'Microsoft' from 'Warning' to 
'Information' allow to get more information in the terminal, when a new request is made to the project

The type of access modifier in a property such as 'protected' means that the property value can be setted or get from
the class in itself or any class that inherits from it.

Entity framework(EF) is an Object Relational Mapper(ORM), it translates the code into SQL commands to update
the table(s) in the DataBase(DB)

The DbContext class acts as a bridge between an entity and the table in the DataBase

Lambdas or '=>' are used when an expression is passed as a parameter

-o specifies the folder that the dotnet command will work on

The main reason for using a collection type of IEnumerable instead of List, it's due that List offers
way too many features that aren't needed for the project, and so it was used IEnumerable.